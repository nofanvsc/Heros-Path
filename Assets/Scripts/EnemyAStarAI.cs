using System.Collections.Generic;
using UnityEngine;

public class EnemyAStarAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public bool canMove = true;

    [Header("Aggro Settings")]
    public float aggroRadius = 20f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float pathUpdateRate = 0.2f;

    [Header("FOV Settings")]
    public float viewAngle = 140f;
    public float viewDistance = 50f;
    public bool drawFOV = true;


    public float patrolRadius = 5f;     
    public float patrolWaitTime = 2f;    
    private float patrolTimer = 0f;
    private bool isPatrolling = false;
    private Vector3 patrolTarget;
   

    List<Node> path;
    int index;
    float timer;
    private float currentSpeed = 0f;


    void Start()
    {
     
        LocalGrid.Instance.GenerateGrid(transform.position);
        SetNewPatrolPoint();
    }


    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool isChasing = false;

   
        if (dist <= aggroRadius && IsPlayerInFOV())
        {
            isChasing = true;
            isPatrolling = false;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = pathUpdateRate;

                LocalGrid.Instance.GenerateGrid(transform.position);
                path = FindPath(transform.position, player.position);
                index = 0;
            }

            if (canMove)
                FollowPath();
        }
        else
        {

            PatrolBehaviour();
        }

    
        if (animator != null)
        {
            bool jogging = (isChasing || isPatrolling) && canMove && currentSpeed > 0.1f;

            animator.SetBool("isJogging", jogging);
            animator.SetFloat("speed", currentSpeed);
        }
    }


    void PatrolBehaviour()
    {
        patrolTimer -= Time.deltaTime;

       
        if (path == null || index >= path.Count)
        {
            if (patrolTimer <= 0f)
            {
                SetNewPatrolPoint();
            }

            currentSpeed = 0f;
            return;
        }

    
        isPatrolling = true;
        if (canMove)
            FollowPath();
    }

  
    // PATROL 
 
    void SetNewPatrolPoint()
    {
        patrolTimer = patrolWaitTime;

  
        Vector2 random = Random.insideUnitCircle.normalized * patrolRadius;
        patrolTarget = new Vector3(
            transform.position.x + random.x,
            transform.position.y,
            transform.position.z + random.y
        );

      
        LocalGrid.Instance.GenerateGrid(transform.position);

        path = FindPath(transform.position, patrolTarget);
        index = 0;

        isPatrolling = true;
    }


    bool IsPlayerInFOV()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > viewDistance) return false;

        Vector3 flatF = transform.forward; flatF.y = 0; flatF.Normalize();
        Vector3 flatP = toPlayer; flatP.y = 0; flatP.Normalize();

        float half = viewAngle / 2f;
        float angle = Vector3.Angle(flatF, flatP);

        return angle <= half;
    }

    void FollowPath()
    {
        if (path == null || index >= path.Count) return;

        Vector3 target = path[index].worldPos;
        Vector3 prev = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));

        currentSpeed = (transform.position - prev).magnitude / Mathf.Max(Time.deltaTime, 1e-6f);

        if (Vector3.Distance(transform.position, target) < 0.4f)
            index++;
    }

    List<Node> FindPath(Vector3 startPos, Vector3 endPos)
    {
        Node start = LocalGrid.Instance.NodeFromWorldPoint(startPos);
        Node end = LocalGrid.Instance.NodeFromWorldPoint(endPos);

        List<Node> open = new List<Node>();
        HashSet<Node> closed = new HashSet<Node>();
        open.Add(start);

        while (open.Count > 0)
        {
            Node current = open[0];

            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].fCost < current.fCost ||
                    open[i].fCost == current.fCost && open[i].hCost < current.hCost)
                {
                    current = open[i];
                }
            }

            open.Remove(current);
            closed.Add(current);

            if (current == end)
                return RetracePath(start, end);

            foreach (Node n in LocalGrid.Instance.GetNeighbours(current))
            {
                if (!n.walkable || closed.Contains(n))
                    continue;

                int newCost = current.gCost + GetDistance(current, n);

                if (newCost < n.gCost || !open.Contains(n))
                {
                    n.gCost = newCost;
                    n.hCost = GetDistance(n, end);
                    n.parent = current;

                    if (!open.Contains(n))
                        open.Add(n);
                }
            }
        }

        return null;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> p = new List<Node>();
        Node curr = end;

        while (curr != start)
        {
            p.Add(curr);
            curr = curr.parent;
        }

        p.Reverse();
        return p;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return 14 * Mathf.Min(dx, dy) + 10 * Mathf.Abs(dx - dy);
    }

    void OnDrawGizmos()
    {
        if (path != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < path.Count - 1; i++)
                Gizmos.DrawLine(path[i].worldPos, path[i + 1].worldPos);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);

        if (!drawFOV) return;

        Vector3 origin = transform.position;
        float half = viewAngle / 2f;

        Vector3 leftDir = Quaternion.Euler(0, -half, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, half, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + leftDir.normalized * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir.normalized * viewDistance);

        int steps = 20;
        float stepAngle = viewAngle / steps;

        for (int i = 0; i < steps; i++)
        {
            float a0 = -half + stepAngle * i;
            float a1 = a0 + stepAngle;

            Vector3 d0 = Quaternion.Euler(0, a0, 0) * transform.forward;
            Vector3 d1 = Quaternion.Euler(0, a1, 0) * transform.forward;

            Gizmos.DrawLine(origin + d0 * viewDistance, origin + d1 * viewDistance);
        }
    }
}
