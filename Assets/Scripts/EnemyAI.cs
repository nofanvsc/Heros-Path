using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Renderer rend; 

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1f;

    [Header("Detection Settings")]
    public float aggroRadius = 5f;  
    public int rayCount = 7;
    public float rayAngle = 45f;
    public LayerMask groundMask;

    [Header("Ground Settings")]
    public float groundOffset = 0.5f;
    private float smoothYVelocity;

    private bool isChasing = false;
    private bool canSeePlayer = false;

    void Start()
    {
      
        rend = GetComponent<Renderer>();

      
        if (rend != null)
            rend.material.color = Color.green;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        canSeePlayer = (distanceToPlayer <= aggroRadius && CanSeePlayer());

        if (canSeePlayer)
            isChasing = true;
        else if (distanceToPlayer > aggroRadius)
            isChasing = false;

       
        if (rend != null)
            rend.material.color = canSeePlayer ? Color.red : Color.green;

        if (isChasing)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);

            if (distanceToPlayer > stopDistance)
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        FollowTerrainHeight();
    }

    bool CanSeePlayer()
    {
        bool detected = false;
        float startAngle = -rayAngle / 2f;
        float angleStep = rayAngle / (rayCount - 1);

        // Vertical pitch angles: up, flat, down
        float[] pitchAngles = { -11f, 0f, 11f };

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;

            foreach (float pitch in pitchAngles)
            {
                Vector3 dir = Quaternion.Euler(pitch, currentAngle, 0) * transform.forward;
                Ray ray = new Ray(transform.position + Vector3.up * 0.5f, dir);

                if (Physics.Raycast(ray, out RaycastHit hit, aggroRadius))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        detected = true;
                        break; 
                    }
                }
            }

            if (detected) break; 
        }

        return detected;
    }


    void FollowTerrainHeight()
    {
        Ray groundRay = new Ray(transform.position + Vector3.up * 5f, Vector3.down);

        if (Physics.Raycast(groundRay, out RaycastHit hit, 20f, groundMask))
        {
            Vector3 newPosition = transform.position;
            float targetY = hit.point.y + groundOffset;

           
            newPosition.y = Mathf.SmoothDamp(transform.position.y, targetY, ref smoothYVelocity, 0.1f);
            transform.position = newPosition;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        Gizmos.color = Color.red;
        float[] pitchAngles = { -11f, 0f, 11f };
        float startAngle = -rayAngle / 2f;
        float angleStep = rayAngle / (rayCount - 1);

        foreach (float pitch in pitchAngles)
        {
            for (int i = 0; i < rayCount; i++)
            {
                float currentAngle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(pitch, currentAngle, 0) * transform.forward;
                Gizmos.color = pitch < 0 ? Color.cyan : pitch > 0 ? Color.magenta : Color.red;
                Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, dir * aggroRadius);
            }
        }
    }
}
