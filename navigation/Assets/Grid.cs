using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool useSmootheningFilter = true;
    public bool drawWorldWts = false;
    public Transform playerTransform;
    public Transform targetTransform;
    public GameObject debugObject;
    public LayerMask unWalkableMask;
    public int unWalkableinterest;
    public LayerMask walkableMask;
    public int walkableinterest;
    public LayerMask footPathMask;
    public int footPathWalkinterest;
    public Vector3 gridWorldSize;
    public float nodeRadius;
    public float kernelSideLength;
    Node[,,] grid;

    float nodeDiametre;
    int gridSizeX, gridSizeY, gridSizeZ;
    int maxWtToMove = 0;

    Vector3 worldBottomLeft;
    Node playerNode;
    Node targetNode;
    void Awake()
    {
        nodeDiametre = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiametre);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiametre);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiametre);

        worldBottomLeft = transform.position - (new Vector3(1, 0, 0) * gridWorldSize.x / 2 + new Vector3(0, 1, 0) * gridWorldSize.y / 2 + new Vector3(0, 0, 1) * gridWorldSize.z / 2);
        CreateGrid();
        playerNode = NodeFromWorldPoint(playerTransform.position);
    }

    void CreateGrid()
    {
        int noofnodes = 0;
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];
        

        for (int x = 0; x<gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft + new Vector3(x * nodeDiametre, y * nodeDiametre, z * nodeDiametre) + new Vector3(nodeRadius, nodeRadius, nodeRadius);
                    //bool walkable = (Physics.CheckSphere(worldPoint, nodeRadius, walkableMask));
                    RaycastHit hit;
                    //ground checking
                    bool walkable = (Physics.Raycast(worldPoint, Vector3.down, out hit, nodeDiametre, walkableMask));
                    int walkInterest = walkableinterest;

                    //footpath checking
                    if (Physics.CheckSphere(worldPoint, nodeRadius, footPathMask))
                    {
                        walkInterest = footPathWalkinterest;
                    }
                    //obstacle cheking
                    if (Physics.CheckSphere(worldPoint, nodeRadius, unWalkableMask))
                    {
                        walkable = false;
                        walkInterest = unWalkableinterest;
                    }
                    grid[x, y, z] = new Node(walkable, worldPoint, x, y, z, walkInterest);
                    if(walkable) { noofnodes++; }
                    if (grid[x, y, z].walkInterest > maxWtToMove)
                    {
                        maxWtToMove = grid[x, y, z].walkInterest;
                    }
                }
            }
        }
        if (useSmootheningFilter)
        {
            GaussianGrid();
        }
    }

    void GaussianGrid()
    {
        maxWtToMove = 0;
        int kernelSideIndexLength = (int)(kernelSideLength / nodeDiametre);
        float kernelSideHalfLength = kernelSideLength / 2;
        //kernelSideIndexLength = 3;
        if (kernelSideIndexLength % 2 == 0) { kernelSideIndexLength++; }
        int kernalSideIndexHalfLength = (kernelSideIndexLength - 1) / 2;
        int[,,] kernel;


        kernel = new int[kernelSideIndexLength, kernelSideIndexLength, kernelSideIndexLength];
        float maxDist = Vector3.Distance(Vector3.zero, new Vector3(1,1,1) * kernelSideHalfLength);
        for (int i = -kernalSideIndexHalfLength; i <= kernalSideIndexHalfLength; i++)
        {
            for (int j = -kernalSideIndexHalfLength; j <= kernalSideIndexHalfLength; j++)
            {
                for (int k = -kernalSideIndexHalfLength; k <= kernalSideIndexHalfLength; k++)
                {
                    kernel[kernalSideIndexHalfLength + i, kernalSideIndexHalfLength + j, kernalSideIndexHalfLength + k] = (int)(((maxDist - (Vector3.Distance(Vector3.zero, new Vector3(i, j, k)) * kernelSideHalfLength/ kernalSideIndexHalfLength))/ maxDist) * 10);
                }
            }
        }
        // string temp = "";
        // for (int i = -kernalSideIndexHalfLength; i <= kernalSideIndexHalfLength; i++)
        // {
        //     string temp1 = "";
        //     for (int j = -kernalSideIndexHalfLength; j <= kernalSideIndexHalfLength; j++)
        //     {
        //         string temp2 = "";
        //         for (int k = -kernalSideIndexHalfLength; k <= kernalSideIndexHalfLength; k++)
        //         {
        //             temp2 += (kernel[kernalSideIndexHalfLength + i, kernalSideIndexHalfLength + j, kernalSideIndexHalfLength + k]).ToString() + " ";
        //         }
        //         temp1 += temp2+" / ";
        //     }
        //     temp += temp1 + " // ";
        // }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    if (!grid[x, y, z].walkable)
                    {
                        continue;
                    }
                    int total = 0;
                    int denominator = 0;
                    for (int i = -kernalSideIndexHalfLength; i <= kernalSideIndexHalfLength; i++)
                    {
                        for (int j = -kernalSideIndexHalfLength; j <= kernalSideIndexHalfLength; j++)
                        {
                            for (int k = -kernalSideIndexHalfLength; k <= kernalSideIndexHalfLength; k++)
                            {
                                if (x + i < 0 || y + j < 0 || z + k < 0 || x + i >= gridSizeX || y + j >= gridSizeY || z + k >= gridSizeZ)
                                {
                                    continue;
                                }
                                else
                                {
                                    total += grid[x + i, y + j, z + k].walkInterest * kernel[kernalSideIndexHalfLength + i, kernalSideIndexHalfLength + j, kernalSideIndexHalfLength + k];
                                    denominator += kernel[kernalSideIndexHalfLength + i, kernalSideIndexHalfLength + j, kernalSideIndexHalfLength + k];
                                }
                            }
                        }
                    }
                    if(denominator > 0)
                    {
                        grid[x, y, z].walkInterest = (int)(total / denominator);
                        if(grid[x, y, z].walkInterest > maxWtToMove)
                        {
                            maxWtToMove = grid[x, y, z].walkInterest;
                        }
                    }
                }
            }
        }
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX* gridSizeY* gridSizeZ;
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        Vector3 nodePosition = new Vector3(node.gridX, node.gridY, node.gridZ);
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x<=1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if(x==0 && y==0 && z == 0) { continue; }

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;
                    Vector3 direction = new Vector3(checkX, checkY, checkZ) - nodePosition;

                    if(checkX > -1 && checkX < gridSizeX && checkY > -1 && checkY < gridSizeY && checkZ > -1 && checkZ < gridSizeZ)
                    {
                        neighbours.Add(grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition - worldBottomLeft).x / gridWorldSize.x;
        float percentY = (worldPosition - worldBottomLeft).y / gridWorldSize.y;
        float percentZ = (worldPosition - worldBottomLeft).z / gridWorldSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY); 
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);

        return grid[x, y, z];
    }


    public List<Node> path;
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));

        if (grid != null && drawWorldWts)
        {
            playerNode = NodeFromWorldPoint(playerTransform.position);
            targetNode = NodeFromWorldPoint(targetTransform.position);
            foreach (Node n in grid)
            {
                float colorRatio = (float)n.walkInterest / ((float)maxWtToMove);
                Gizmos.color = (n.walkable) ? Color.Lerp(Color.red, Color.white, colorRatio) : Color.red;
                if (playerNode == n)
                {
                    Gizmos.color = Color.cyan;
                }
                if (targetNode == n)
                {
                    Gizmos.color = Color.blue;
                }
                if (n.walkable)
                {
                    if (path != null)
                    {
                        if (path.Contains(n))
                        {
                            //Gizmos.color = Color.black;
                        }
                    }
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiametre - 0.1f));
                }
            }
        }
        if(path!= null)
        {
            Node prevNode = path[0];
            for (int i = 1; i < path.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(prevNode.worldPosition, path[i].worldPosition);
                prevNode = path[i];
            }
        }
        
    }
}
