using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class PathFinding : MonoBehaviour{
    // public bool UseHeap;
    private static List<Vector3> path;
    private static TileMap tileMap;
    public Transform source;
    public Transform target;
    public void TestRun(MapGenerator mapGenerator)
    {
        path = new List<Vector3>();
        Index start = mapGenerator.WorldPointToMapIndex(source.position);
        Index goal = mapGenerator.WorldPointToMapIndex(target.position);
        TileMap tileMap = mapGenerator.tileMap;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        List<Index> li = FindPath_AStar(start, goal, mapGenerator);
        if (li != null)
        {
            foreach (Index i in li)
            {
                path.Add(mapGenerator.IndexToWorldLocation(i));
            }
        }
        sw.Stop();
        UnityEngine.Debug.Log("Time Elapsed: " + sw.ElapsedMilliseconds + " ms ");
        UnityEngine.Debug.Log("Path Len: " + path.Count);
    }

    void OnDrawGizmos()
    {
        if(path != null)
        {
            foreach(Vector3 i in path)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(i, Vector3.one);
            }
        }
    }

    static int HCostEstimate(Index start, Index goal)
    {
        return DistanceBetween(start, goal);
    }
    static List<Index> ReconstructPath(Index goal)
    {
        List<Index> path = new List<Index>();
        Index current = goal;
        while (true) {
            if (tileMap[current].parent == null || tileMap[current].parent == current)
            {
                break;
            }
            path.Add(current);
            current = tileMap[current].parent;
        }
        path.Reverse();
        return path;
    }
    static int DistanceBetween(Index a, Index b)
    {
        return System.Math.Abs(a.row - b.row) + System.Math.Abs(a.col - b.col);
    }

    public static List<Index> FindPath_AStar(Index _startIndex, Index _goalIndex, MapGenerator mapGenerator) {
        tileMap = mapGenerator.tileMap;
        tileMap.ResetScores();
        Tile start = tileMap[_startIndex];
        Tile goal = tileMap[_goalIndex];
        Heap<Tile> openSet = new Heap<Tile>(tileMap.tileCount);   // The set of nodes already evaluated.
        Heap<Tile> closedSet = new Heap<Tile>(tileMap.tileCount);   // The set of tentative nodes to be evaluated, initially containing the start node
        openSet.Add(start);
        start.gScore = 0; // Cost from start along best known path.

        // Estimated total cost from start to goal through y
        start.hScore = HCostEstimate(_startIndex, _goalIndex);

        while (openSet.Count > 0) {

            Tile current = openSet.RemoveTop();
            if (current.Equals(goal))
            {
                return ReconstructPath(_goalIndex);
            }
            closedSet.Add(current);
            foreach (Index neighbourIndex in current.neighbourList)
            {
                Tile neighbour = tileMap[neighbourIndex];
                if (neighbour.isWalkable)
                {
                    if (closedSet.Contains(neighbour))
                    {
                        // Ignore the neighbor which is already evaluated.
                        continue;
                    }
                    int gscoreNew = current.gScore + DistanceBetween(current.index, neighbourIndex); // length of this path.
                    if (!openSet.Contains(neighbour))  // Discover a new node
                        openSet.Add(neighbour);
                    else if (gscoreNew >= neighbour.gScore)
                        continue;        // This is not a better path.

                    // This path is the best until now. Record it!
                    neighbour.parent = current.index;
                    neighbour.gScore = gscoreNew;
                    neighbour.hScore = HCostEstimate(neighbourIndex, _goalIndex);
                }
            }            
        }
        
        return null;
    }
    public static List<Index> FindPath_Slow(Index start, Index goal)
    {
        List<Index> closedSet = new List<Index>();   // The set of nodes already evaluated.
        List<Index> openSet = new List<Index>();   // The set of tentative nodes to be evaluated, initially containing the start node
        openSet.Add(start);
        tileMap[start].gScore = 0; // Cost from start along best known path.

        // Estimated total cost from start to goal through y
        tileMap[start].hScore = HCostEstimate(start, goal);

        while (openSet.Count > 0)
        {
            Index current = openSet[0];
            foreach (Index i in openSet)
            {
                if (tileMap[i].fScore < tileMap[current].fScore)
                {
                    current = i;
                }
            }
            if (current.Equals(goal))
                return ReconstructPath(goal);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Index neighbour in tileMap[current].neighbourList)
            {
                if (tileMap[neighbour].isWalkable)
                {
                    if (closedSet.Contains(neighbour))
                    {
                        // Ignore the neighbor which is already evaluated.
                        continue;
                    }
                    int gscoreNew = tileMap[current].gScore + DistanceBetween(current, neighbour); // length of this path.
                    if (!openSet.Contains(neighbour))  // Discover a new node
                        openSet.Add(neighbour);
                    else if (gscoreNew >= tileMap[neighbour].gScore)
                        continue;        // This is not a better path.

                    // This path is the best until now. Record it!
                    tileMap[neighbour].parent = current;
                    tileMap[neighbour].gScore = gscoreNew;
                    tileMap[neighbour].hScore = HCostEstimate(neighbour, goal);
                }
            }
        }
        UnityEngine.Debug.Log("Failed Path");
        return null;
    }
}
        
    


