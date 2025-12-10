using System.Collections.Generic;
using UnityEngine;

public class LocalGrid : MonoBehaviour
{
    public static LocalGrid Instance;

    public float nodeRadius = 1f;
    public int gridSize = 40;
    public float maxSlopeAngle = 40f;
    public LayerMask groundMask;
    public LayerMask obstacleMask;

    Node[,] grid;
    float nodeDiameter;
    Vector3 center;

    Terrain terrain;

    void Awake()
    {
        Instance = this;
        nodeDiameter = nodeRadius * 2f;
        terrain = Terrain.activeTerrain;
    }


    public void GenerateGrid(Vector3 worldCenter)
    {
        center = worldCenter;
        grid = new Node[gridSize, gridSize];

        float half = (gridSize / 2f) * nodeDiameter;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 pos = new Vector3(
                    worldCenter.x - half + x * nodeDiameter,
                    worldCenter.y + 30f,
                    worldCenter.z - half + y * nodeDiameter
                );

                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f, groundMask))
                {
                    pos = hit.point;

                    float slope = Vector3.Angle(hit.normal, Vector3.up);
                    bool tooSteep = slope > maxSlopeAngle;

                    bool blocked = Physics.CheckSphere(pos, nodeRadius * 0.6f, obstacleMask);

                    bool walkable = !tooSteep && !blocked;

                    grid[x, y] = new Node(walkable, pos, x, y);
                }
                else
                {
                    grid[x, y] = new Node(false, pos, x, y);
                }
            }
        }
    }


    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        float half = (gridSize / 2f) * nodeDiameter;

        float percentX = (worldPos.x - (center.x - half)) / (gridSize * nodeDiameter);
        float percentY = (worldPos.z - (center.z - half)) / (gridSize * nodeDiameter);

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSize - 1) * percentX);
        int y = Mathf.RoundToInt((gridSize - 1) * percentY);

        return grid[x, y];
    }

   
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> list = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSize &&
                    checkY >= 0 && checkY < gridSize)
                {
                    list.Add(grid[checkX, checkY]);
                }
            }
        }
        return list;
    }

 
    void OnDrawGizmos()
    {
        if (grid == null) return;

        foreach (Node n in grid)
        {
            Gizmos.color = n.walkable ? new Color(0, 1, 0, .2f) : new Color(1, 0, 0, .4f);
            Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeDiameter - .1f));
        }
    }
}

public class Node
{
    public bool walkable;
    public Vector3 worldPos;
    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector3 pos, int x, int y)
    {
        this.walkable = walkable;
        this.worldPos = pos;
        this.gridX = x;
        this.gridY = y;
    }
}
