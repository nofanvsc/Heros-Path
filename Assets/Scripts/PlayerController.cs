using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private CharacterController controller;
    private Transform cam;

    [Header("Weapon Reference")]
    public GameObject swordObject;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float jogSpeed = 10f;
    public float runSpeed = 14f;
    public float jumpForce = 3f;
    public float gravity = -40f;

    [Header("Dash Settings")]
    public float dashTotalDistance = 5f;
    public float dashTime = 0.35f;
    private bool isDashing = false;

    [Header("Attack Dash Settings")]
    public float lightAttackDashDistance = 2.5f;
    public float heavyAttackDashDistance = 3.5f;
    public float attackDashTime = 0.25f;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isWalking = false;
    private bool isRunning = false;
    private bool hasWeapon = false;

  
    private bool isDead = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        if (swordObject != null)
            swordObject.SetActive(hasWeapon);
    }

    private void Update()
    {
    
        if (isDead)
        {
            controller.Move(Vector3.zero);
            return;
        }

        if (!isDashing)
            HandleMovement();

        HandleMovementToggles();
        HandleJump();
        HandleWeaponToggle();
        HandleCombat();

        Vector3 fixedRot = transform.eulerAngles;
        fixedRot.x = 0f;
        fixedRot.z = 0f;
        transform.eulerAngles = fixedRot;
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        animator.SetBool("IsGrounded", isGrounded);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        Vector3 moveDir = Vector3.zero;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * v + camRight * h).normalized;

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        float currentSpeed = jogSpeed;
        if (isWalking) currentSpeed = walkSpeed;
        if (isRunning) currentSpeed = runSpeed;

        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        animator.SetFloat("Speed", inputDir.magnitude * currentSpeed);
    }

    void HandleMovementToggles()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            isWalking = !isWalking;
            isRunning = false;

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRunning = !isRunning;
            isWalking = false;

            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsWalking", false);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }
    }

    void HandleWeaponToggle()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            hasWeapon = !hasWeapon;

            animator.SetBool("HasWeapon", hasWeapon);

            if (swordObject != null)
                swordObject.SetActive(hasWeapon);
        }
    }

    void HandleCombat()
    {
        if (!hasWeapon)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("AttackLightTrigger");
            if (!isDashing)
                StartCoroutine(AttackDash(lightAttackDashDistance));
        }

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("AttackHeavyTrigger");
            if (!isDashing)
                StartCoroutine(AttackDash(heavyAttackDashDistance));
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isDashing)
            {
                animator.SetTrigger("DashTrigger");
                StartCoroutine(DashRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
            animator.SetTrigger("DieTrigger");
    }


    public void OnPlayerDeath()
    {
        isDead = true;

        animator.SetTrigger("DieTrigger");

        velocity = Vector3.zero;

        hasWeapon = false;
        if (swordObject != null)
            swordObject.SetActive(false);
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        float elapsed = 0f;
        Vector3 direction = transform.forward;
        float dashSpeed = dashTotalDistance / dashTime;

        while (elapsed < dashTime)
        {
            float t = elapsed / dashTime;
            float eased = 1 - Mathf.Pow(1 - t, 3);

            controller.Move(direction * (dashSpeed * eased) * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private IEnumerator AttackDash(float distance)
    {
        isDashing = true;

        float elapsed = 0f;
        Vector3 direction = transform.forward;
        float speed = distance / attackDashTime;

        while (elapsed < attackDashTime)
        {
            controller.Move(direction * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
