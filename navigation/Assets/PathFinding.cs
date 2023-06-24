using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class PathFinding : MonoBehaviour
{
    public bool useHeap = false;
    public Transform seeker, target;
    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    void Update()
    {
        //if (!Input.GetKeyDown("space"))
        //{
        //    return;
        //}
        if (!useHeap)
        {
            FindPathWithList(seeker.position, target.position);
        }
        else
        {
            FindPathWithHeap(seeker.position, target.position);
        }
    }

    void FindPathWithList(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        int TempNodesSearched = 0;
        while (openSet.Count > 0)
        {
            TempNodesSearched++;
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost / openSet[i].walkInterest < currentNode.hCost / currentNode.walkInterest)
                {
                    currentNode = openSet[i];
                }
            }
            

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if(currentNode == targetNode)
            {
                sw.Stop();
                UnityEngine.Debug.Log("Path Found :" + sw.ElapsedMilliseconds + "ms");
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void FindPathWithHeap(Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        int TempNodesSearched = 0;
        while (openSet.Count > 0)
        {
            TempNodesSearched++;
            Node currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                sw.Stop();
                UnityEngine.Debug.Log("Path Found :" + sw.ElapsedMilliseconds + "ms");
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        Vector3 curDirection = Vector3.zero;
        while(currentNode!= startNode)
        {
            Vector3 newDirection = currentNode.worldPosition - currentNode.parent.worldPosition;
            if (curDirection != newDirection)
            {
                path.Add(currentNode);
                curDirection = newDirection;
            }
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        int distZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        int[] order = new int[3] { distX, distY, distZ };
        for (int i=0;i<order.Length; i++)
        {
            for (int j = i + 1; j < order.Length; j++)
            {
                if (order[i] < order[j])
                {
                    int temp = order[i];
                    order[i] = order[j];
                    order[j] = temp;
                }
            }
        }

        int dist = 17 * (order[2]) + 14 * (order[1] - order[2]) + 10 * (order[0] - order[1]);
        return dist;
    }
}
