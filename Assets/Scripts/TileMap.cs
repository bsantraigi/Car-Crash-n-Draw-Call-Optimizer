using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileMap : IEnumerable
{

    // Variables
    int rowSize, colSize;
    int _tileCount;
    [SerializeField]
    public Tile[,] tiles;
    int _visitCount;

    public int[,] gScores;
    public bool[,] isScored; // If the gScore has already been set once after reset
    public int[,] hScores;
    public Index[,] parents; // Helper field for A* algorithm

    public void ResetScores()
    {
        gScores = new int[rowSize, colSize];
        hScores = new int[rowSize, colSize];
        parents = new Index[rowSize, colSize];
        isScored = new bool[rowSize, colSize];
    }

    public int tileCount
    {
        get { return _tileCount; }
    }
    // Properties
    public Tile this[int i, int j]
    {
        get
        {
            if (i < 0 || i > rowSize - 1 || j < 0 || j > colSize - 1)
            {
                return null;
            }
            else
            {
                return tiles[i, j];
            }
        }
        set
        {
            if (i >= 0 && i < rowSize && j >= 0 && j < colSize)
            {
                tiles[i, j] = value;
            }
            else
            {
                Debug.Log("NodeMatrix index out of limits.");
                // Throw Some Error
            }
        }
    }
    public Tile this[Index index]
    {
        get
        {
            int i = index.row;
            int j = index.col;
            if (i < 0 || i > rowSize - 1 || j < 0 || j > colSize - 1)
            {
                return null;
            }
            else
            {
                return tiles[i, j];
            }
        }
        set
        {
            int i = index.row;
            int j = index.col;
            if (i >= 0 && i < rowSize && j >= 0 && j < colSize)
            {
                tiles[i, j] = value;
            }
            else
            {
                Debug.Log("NodeMatrix index out of limits.");
                // Throw Some Error
            }
        }
    }
    public int GetLength(int i)
    {
        // 0 - numrows
        // 1 - numcols
        return i == 0 ? colSize : rowSize;
    }    
    public TileMap(int _rowSize, int _colSize)
    {
        rowSize = _rowSize;
        colSize = _colSize;
        _tileCount = rowSize * colSize;
        tiles = new Tile[rowSize, colSize];

        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                tiles[z, x] = new Tile(z, x, this);
                tiles[z, x].SetNeighbours(rowSize, colSize);
                tiles[z, x].Set_8_Neighbours(rowSize, colSize);
            }
        }
        _visitCount = 0;
    }
    public void Visit(Index index)
    {
        tiles[index.row, index.col].Visit();
        _visitCount++;
    }
    public float PercentVisit
    {
        get { return (float)_visitCount * 100f / _tileCount; }
    }    
    public bool AnyVisitedNeighbour(Index index)
    {
        Tile n = this[index];

        foreach (Index nIndex in n._8_NeighbourList)
        {
            if (this[nIndex].visited)
            {
                return true;
            }
        }
        return false;
    }
    public Tile RandomVisitNeighbour(int row, int col)
    {
        List<Index> neighbours = tiles[row, col].neighbourList;
        // Check if any unvisited node
        int visitCount = 0;
        foreach (Index index in neighbours)
        {
            Tile n = tiles[index.row, index.col];
            if (n.visited)
            {
                visitCount++;
            }
        }
        if (neighbours.Count == visitCount)
        {
            return null; // All neighbours explored
        }

        // Randomly choose an unvisited neighbour
        Index indx = neighbours[Random.Range(0, neighbours.Count)];
        Tile rNode = tiles[indx.row, indx.col];
        while (rNode.visited)
        {
            indx = neighbours[Random.Range(0, neighbours.Count)];
            rNode = tiles[indx.row, indx.col];
        }
        Visit(indx);
        return rNode;
    }
    public Index Expand(Index rIndex)
    {        
        List<Index> neighbours = this[rIndex].neighbourList;
        int chances = 0;
        foreach (Index i in neighbours)
        {
            if (CanExpand(rIndex, i))
            {
                chances++;
                break;
            }
        }
        if (chances == 0)
        {
            return null;
        }

        // Randomly choose an unvisited neighbour
        Index newIndex = neighbours[Random.Range(0, neighbours.Count)];
        while (!CanExpand(rIndex, newIndex))
        {
            newIndex = neighbours[Random.Range(0, neighbours.Count)];
        }
        Visit(newIndex);
        return newIndex;
    }
    bool CanExpand(Index baseIndex, Index targetIndex)
    {
        List<Index> tNeighbours = this[targetIndex]._8_NeighbourList;
        if (baseIndex.row == targetIndex.row)
        {
            int xdiff = targetIndex.col - baseIndex.col;
            if (xdiff > 0)
            {
                // Check nodes with higher x only
                foreach (Index ni in tNeighbours)
                {
                    if (ni.Equals(baseIndex))
                    {
                        // Skip
                        continue;
                    }
                    if (ni.col >= targetIndex.col)
                    {
                        if (this[ni].visited)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                // Check nodes with lower x only
                foreach (Index ni in tNeighbours)
                {
                    if (ni.Equals(baseIndex))
                    {
                        // Skip
                        continue;
                    }
                    if (ni.col <= targetIndex.col)
                    {
                        if (this[ni].visited)
                        {
                            return false;
                        }
                    }
                }
            }

        }
        else
        {
            int zdiff = targetIndex.row - baseIndex.row;
            if (zdiff > 0)
            {
                // Check nodes with higher z only
                foreach (Index ni in tNeighbours)
                {
                    if (ni.Equals(baseIndex))
                    {
                        // Skip
                        continue;
                    }
                    if (ni.row >= targetIndex.row)
                    {
                        if (this[ni].visited)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                // Check nodes with lower z only
                foreach (Index ni in tNeighbours)
                {
                    if (ni.Equals(baseIndex))
                    {
                        // Skip
                        continue;
                    }
                    if (ni.row <= targetIndex.row)
                    {
                        if (this[ni].visited)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }    
    public void GenerateNewTileMap(int row1, int col1, int row2, int col2)
    {
        // CustomGraph.GetDFSMap(ref tiles, row1, col1, row2, col2);
        CustomGraph.GetRankBasedMap(ref tiles, row1, col1, row2, col2);
        SetRoadTypes();
    }
    public uint FindNearestGroupID(Index baseIndex)
    {
        TileType type = this[baseIndex].type;
        uint gID = this[baseIndex].groupID;
        Index[] indices =
        {
            new Index(baseIndex.row - 1, baseIndex.col),
            new Index(baseIndex.row, baseIndex.col - 1)
        };
        foreach(Index i in indices)
        {
            if (IsValidIndex(i))
            {
                if(this[i].type == type)
                {
                    return this[i].groupID;
                }
            }
        }

        return 0;
    }    
    public bool IsValidIndex(Index i)
    {
        return ((i.col >= 0) && (i.col < colSize) && (i.row >= 0) && (i.row < rowSize));
    }
    public int[,] TileMapToIntArray()
    {
        int[,] map = new int[rowSize, colSize];
        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                map[z, x] = (int)tiles[z, x].type;
            }
        }
        return map;
    }
    public IEnumerator GetEnumerator()
    {
        return tiles.GetEnumerator();
    }
    public void MakePath(Index index1, Index index2)
    {
        int x, z;
        x = index1.col;
        z = index1.row;
        while (x != index2.col)
        {
            if(x < index2.col)
            {
                tiles[z, ++x].type = TileType.ROAD;
            }
            else
            {
                tiles[z, --x].type = TileType.ROAD;
            }
        }

        while (z != index2.row)
        {
            if (z < index2.row)
            {
                tiles[++z, x].type = TileType.ROAD;
            }
            else
            {
                tiles[--z, x].type = TileType.ROAD;
            }
        }
    }
    public Tile GetRoadTileNear(Index _index)
    {
        bool[,] visited = new bool[rowSize, colSize];
        Tile tile = new Tile(_index.row, _index.col, this);

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(this[_index]);
        while(queue.Count > 0)
        {
            tile = queue.Dequeue();
            visited[tile.index.row, tile.index.col] = true;
            if(tile.type == TileType.ROAD)
            {
                break;
            }
            foreach(Index ni in tile._8_NeighbourList)
            {
                if (!visited[ni.row, ni.col])
                {
                    queue.Enqueue(this[ni]);
                }
            }
        }
        return tile;
    }
    public void SetRoadTypes()
    {
        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile tile0 = this[z, x];
                if (tile0.type != TileType.ROAD)
                {
                    continue;
                }
                List<Index> roadNeighbours = new List<Index>();
                foreach (Index i in tile0.neighbourList)
                {
                    if (this[i].type == TileType.ROAD)
                    {
                        roadNeighbours.Add(i);
                    }
                }
                int len = roadNeighbours.Count;
                Tile tile1, tile2;
                switch (len)
                {
                    case 4:
                        tile0.roadType = RoadType.CrissCross;
                        break;
                    case 3:
                        tile0.roadType = RoadType.CrissCross;
                        break;
                    case 2:
                        // 2 neighbours
                        tile1 = this[roadNeighbours[0].row, roadNeighbours[0].col];
                        tile2 = this[roadNeighbours[1].row, roadNeighbours[1].col];
                        if (tile1.x == tile2.x)
                        {
                            tile0.roadType = RoadType.Vertical;
                        }
                        else if (tile1.z == tile2.z)
                        {
                            tile0.roadType = RoadType.Horizontal;
                        }
                        else
                        {
                            int xDiff = tile1.x - tile2.x;
                            int zDiff = tile1.z - tile2.z;
                            int k = tile1.x - tile0.x;
                            if (xDiff == 1)
                            {
                                if (zDiff == 1)
                                    tile0.roadType = (k == 0) ? RoadType.Turn_LU : RoadType.Turn_RD;
                                else
                                    tile0.roadType = (k == 0) ? RoadType.Turn_LD : RoadType.Turn_RU;
                            }
                            else if (xDiff == -1)
                            {
                                if (zDiff == 1)
                                    tile0.roadType = (k == 0) ? RoadType.Turn_RU : RoadType.Turn_LD;
                                else
                                    tile0.roadType = (k == 0) ? RoadType.Turn_RD : RoadType.Turn_LU;
                            }
                        }
                        break;
                    case 1:
                        tile1 = this[roadNeighbours[0].row, roadNeighbours[0].col];
                        if (tile0.z == tile1.z)
                        {
                            tile0.roadType = RoadType.Horizontal;
                        }
                        else
                        {
                            tile0.roadType = RoadType.Vertical;
                        }
                        break;
                }
            }
        }
    }
}

class CustomGraph {
    static TileMap grid;
    static Tile start;
    static Tile finish;
    static int colSize;
    static int rowSize;

    public static void GetDFSMap(ref Tile[,] tiles, int row1, int col1, int row2, int col2)
    {

        rowSize = tiles.GetLength(0);
        colSize = tiles.GetLength(1);

        grid = new TileMap(rowSize, colSize);
        
        start = grid[row1, col1];
        finish = grid[row2, col2];

        // Start DFS on the node (custom)graph
        Tile current = start;
        grid.Visit(start.index);
        Stack<Tile> nodeBuff = new Stack<Tile>();
        while (!(current.Equals(finish) || current == null))
        {
            // Debug.Log(new Index(current.z, current.x));
            Tile next = grid.RandomVisitNeighbour(current.z, current.x);
            if (next == null)
            {
                if (nodeBuff.Count == 0)
                {
                    return;
                }
                current = nodeBuff.Pop();
            }
            else
            {
                nodeBuff.Push(current);
                current = next;
            }
        }
        while(nodeBuff.Count != 0)
        {
            Tile n = nodeBuff.Pop();
            tiles[n.z, n.x].type = TileType.ROAD;
        }
        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile node = grid[z, x];
                if (node.visited)
                {
                    tiles[node.z, node.x].type = TileType.ROAD;
                }
            }
        }
    }

    public static void GetRankBasedMap(ref Tile[,] tiles, int row1, int col1, int row2, int col2)
    {
        ExpandRegions(ref tiles, row1, col1, row2, col2);
        ConnectAllRoads(ref tiles);
        CalculatePathRanks();
        
    }    

    static void ExpandRegions(ref Tile[,] tiles, int row1, int col1, int row2, int col2)
    {
        rowSize = tiles.GetLength(0);
        colSize = tiles.GetLength(1);
        
        Index startIndex = new Index(row1, col1);
        Index finishIndex = new Index(row2, col2);

        grid = new TileMap(rowSize, colSize);

        start = grid[startIndex];
        finish = grid[finishIndex];

        int roomCount = 7;
        int maxRoomSize = 6; // Max no. of tiles a room can have
        bool canContinue = true;
        //for (int r = 0; r < roomCount; r++)
        while(grid.PercentVisit <= 60)
        {
            // Debug.Log(grid.PercentVisit);
            // Get a tile which has no visited neighbour yet
            Index rIndex = new Index(Random.Range(0, rowSize), Random.Range(0, colSize));
            int trialCount = 0;
            while (grid.AnyVisitedNeighbour(rIndex))
            {
                if(trialCount > 100)
                {
                    canContinue = false;
                    break;
                }
                trialCount++;
                rIndex.row = Random.Range(0, rowSize);
                rIndex.col = Random.Range(0, colSize);
            }            
            if (!canContinue)
            {
                break;
            }
            grid.Visit(rIndex);
            grid.Visit(rIndex);
            // Start expanding around rIndex
            for (int size = 1; size<maxRoomSize; size++)
            {
                // Debug.Log("Expand Base:" + rIndex);
                rIndex = grid.Expand(rIndex);     
                if(rIndex == null)
                {
                    break;
                }           
            }           
        }

        // Mark the road tiles
        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile node = grid[z, x];
                if (node.visited)
                {
                    grid[z, x].type = tiles[z, x].type = TileType.ROAD;
                    node.UnVisit();
                }
            }
        }

    }
    static void CalculatePathRanks()
    {

    }
    /// <summary>
    /// Find the connected components of all unvisited
    /// nodes and connnect them using shortest length paths
    /// </summary>
    static void ConnectAllRoads(ref Tile[,] tiles)
    {
        uint maxTileGroupID = 0;

        // HORIZONTAL pass
        grid[0, 0].groupID = 1; // Min possible group id is 1
        for(int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile current = grid[z, x];
                if(current.type == TileType.ROAD)
                {
                    uint gid = grid.FindNearestGroupID(current.index);
                    if(gid == 0)
                    {
                        maxTileGroupID++;                        
                        current.groupID = maxTileGroupID;
                    }
                    else
                    {
                        current.groupID = gid;
                    }
                }
            }
        }
        
        int[,] equivalenceClass = new int[maxTileGroupID + 1, maxTileGroupID + 1];

        // VERTICAL Pass
        for (int x = 0; x < colSize; x++)
        {
            for (int z = 0; z < rowSize; z++)
            {
                Tile current = grid[z, x];
                if (current.type == TileType.ROAD)
                {
                    // Find all neighbouring tile gids(of same tiletypes) and mark them
                    // Find the min of all marked
                    Index lIndex = new Index(z, x - 1);
                    if (grid.IsValidIndex(lIndex)) {
                        Tile left = grid[lIndex];
                        if (current.type == left.type)
                        {
                            if (current.groupID != left.groupID)
                            {
                                equivalenceClass[current.groupID, left.groupID] = 1;
                                equivalenceClass[left.groupID, current.groupID] = 1;
                            }
                        }
                    }
                }

            }
        }

        // Recursion using stack for finding unique ids and their equivalents
        bool[] isChecked = new bool[maxTileGroupID + 1];
        Dictionary<uint, List<uint>> uniqueIDs = new Dictionary<uint, List<uint>>();
        uint[] matchedIDs = new uint[maxTileGroupID + 1];

        for (uint id1 = 1; id1 < maxTileGroupID + 1; id1++)
        {
            if (!isChecked[id1])
            {
                uniqueIDs.Add(id1, new List<uint>());
                isChecked[id1] = true;
                uint rootID = id1;
                Stack<uint> children = new Stack<uint>();
                for (uint id2 = 1; id2 < maxTileGroupID + 1; id2++)
                {
                    if (rootID != id2 && equivalenceClass[rootID, id2] == 1)
                    {
                        uniqueIDs[rootID].Add(id2);
                        children.Push(id2);
                    }
                }

                while (children.Count > 0)
                {
                    uint id2 = children.Pop();
                    if (!isChecked[id2])
                    {
                        isChecked[id2] = true;
                        for (uint id3 = 1; id3 < maxTileGroupID + 1; id3++)
                        {
                            if (id2 != id3 && rootID != id3 && equivalenceClass[id2, id3] == 1)
                            {
                                uniqueIDs[rootID].Add(id3);
                                children.Push(id3);
                            }
                        }
                    }
                }

            }
        }


        foreach (uint id in uniqueIDs.Keys)
        {
            foreach(uint id2 in uniqueIDs[id])
            {
                matchedIDs[id2] = id;
            }
        }

        for (int i = 0; i < matchedIDs.Length; i++)
        {
            if (matchedIDs[i] == 0)
            {
                matchedIDs[i] = (uint)i;
            }
        }

        // CHANGE the groupIDs of tiles to UNIQUE group ids only
        for (int x = 0; x < colSize; x++)
        {
            for (int z = 0; z < rowSize; z++)
            {
                Tile current = grid[z, x];
                if (current.groupID != 0)
                {
                    current.groupID = matchedIDs[current.groupID];
                }
            }
        }

        // Drop some tiles to road to CONNECT ALL THE ROADS TOGETHER
        RoomSet roomset = new RoomSet();
        foreach(Tile t in grid)
        {
            if (t.groupID != 0)
            {
                roomset.AddTile(t, t.groupID);
            }
        }

        // Create connections among the rooms until there is only one room left
        int countCache = roomset.rooms.Count;
        while (roomset.rooms.Count > 1)
        {
            countCache = roomset.rooms.Count;

            // Dictionary to store the pair of rooms to be connected
            Dictionary<uint, List<uint>> connectSequence = new Dictionary<uint, List<uint>>();

            // For each room find out the nearest room and add to the above list
            foreach (uint gid in roomset.rooms.Keys)
            {
                Room room = roomset.rooms[gid];
                Room bestRoom = null;
                float bestDist = float.MaxValue;
                Vector3 center1 = room.GetCenter();
                foreach (uint nextId in roomset.rooms.Keys)
                {
                    // Find out distances from each room and store the minimum
                    if (gid != nextId)
                    {
                        Room otherRoom = roomset.rooms[nextId];
                        Vector3 center2 = otherRoom.GetCenter();
                        float dist = Vector3.Distance(center1, center2);
                        if (dist < bestDist)
                        {
                            bestRoom = otherRoom;
                            bestDist = dist;
                        }
                    }
                }

                bool room_exists = connectSequence.ContainsKey(room.tileGroupID);
                bool bestRoom_exists = connectSequence.ContainsKey(bestRoom.tileGroupID);

                // Next piece of code makes sure that there is no repeated connection
                // request e.g. (1,3) and (3,1)

                if (!room_exists)
                {
                    if (!bestRoom_exists)
                    {
                        // Add index for room
                        connectSequence.Add(room.tileGroupID, new List<uint>());
                        connectSequence[room.tileGroupID].Add(bestRoom.tileGroupID);
                    }
                    else
                    {
                        // check for existing connections o/w add
                        if (!connectSequence[bestRoom.tileGroupID].Contains(room.tileGroupID))
                        {
                            connectSequence[bestRoom.tileGroupID].Add(room.tileGroupID);
                        }
                    }
                }
                else
                {
                    // Add to rooms index in the list if already not there
                    connectSequence[room.tileGroupID].Add(bestRoom.tileGroupID);
                }
            }
            
            
            roomset.ApplyConnections(connectSequence, ref grid);            
            if(countCache <= roomset.rooms.Count)
            {
                // FURTHER MERGE FAIL
                break;
            }
        }
        // Mark the road tiles
        for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile node = grid[z, x];
                if (node.type == TileType.ROAD)
                {
                    tiles[z, x].type = TileType.ROAD;
                }
            }
        }

        //TODO: Set direction of the tiles
        //TODO: Remove unnecessary tiles
        #region ROADTYPE_CODE
        /*for (int z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < colSize; x++)
            {
                Tile tile0 = grid[z, x];
                if(tile0.type != TileType.ROAD)
                {
                    continue;
                }
                List<Index> roadNeighbours = new List<Index>();
                foreach(Index i in tile0.neighbourList)
                {
                    if(grid[i].type == TileType.ROAD)
                    {
                        roadNeighbours.Add(i);
                    }
                }
                int len = roadNeighbours.Count;
                Tile tile1, tile2;
                switch (len)
                {
                    case 4:
                        tile0.roadType = RoadType.CrissCross;
                        break;
                    case 3:
                        tile0.roadType = RoadType.CrissCross;
                        break;
                    case 2:
                        // 2 neighbours
                        tile1 = grid[roadNeighbours[0].row, roadNeighbours[0].col];
                        tile2 = grid[roadNeighbours[1].row, roadNeighbours[1].col];
                        if (tile1.x == tile2.x)
                        {
                            tile0.roadType = RoadType.Vertical;
                        }
                        else if (tile1.z == tile2.z)
                        {
                            tile0.roadType = RoadType.Horizontal;
                        }
                        else
                        {
                            int xDiff = tile1.x - tile2.x;
                            int zDiff = tile1.z - tile2.z;
                            int k = tile1.x - tile0.x;
                            if (xDiff == 1)
                            {
                                if (zDiff == 1)
                                    tile0.roadType = (k == 0) ? RoadType.Turn_LU : RoadType.Turn_RD;
                                else
                                    tile0.roadType = (k == 0) ? RoadType.Turn_LD : RoadType.Turn_RU;
                            }
                            else if (xDiff == -1)
                            {
                                if (zDiff == 1)
                                    tile0.roadType = (k == 0) ? RoadType.Turn_RU : RoadType.Turn_LD;
                                else
                                    tile0.roadType = (k == 0) ? RoadType.Turn_RD : RoadType.Turn_LU;
                            }
                        }
                        break;
                    case 1:
                        tile1 = grid[roadNeighbours[0].row, roadNeighbours[0].col];
                        if (tile0.z == tile1.z)
                        {
                            tile0.roadType = RoadType.Horizontal;
                        }
                        else
                        {
                            tile0.roadType = RoadType.Vertical;
                        }
                        break;
                }
                
                tiles[tile0.z, tile0.x].roadType = tile0.roadType;
            }
        }*/
        #endregion
    }
}

public class Room
{    
    public enum BOUND
    {
        LEFT_TOP,
        RIGHT_TOP,
        LEFT_BOTTOM,
        RIGHT_BOTTOM
    }
    public uint tileGroupID;
    public List<Tile> tiles;
    public List<uint> connectedRooms;
    public int tileCount;
    Dictionary<BOUND, Index> bounds;
    Index LeftTop, LeftBottom, RightTop, RightBottom;
    Vector3 center;
    bool boundsUpdated = false;
    bool centerUpdated = false;

    public Room(uint gID)
    {
        tiles = new List<Tile>();
        tileGroupID = gID;
        tileCount = 0;

        LeftTop = new Index(int.MinValue, int.MaxValue);
        RightBottom = new Index(int.MaxValue, int.MinValue);
        RightTop = new Index(int.MinValue, int.MinValue);
        LeftBottom = new Index(int.MaxValue, int.MaxValue);

        bounds = new Dictionary<BOUND, Index>();

        bounds.Add(BOUND.LEFT_BOTTOM, LeftBottom);
        bounds.Add(BOUND.RIGHT_BOTTOM, RightBottom);
        bounds.Add(BOUND.LEFT_TOP, LeftTop);
        bounds.Add(BOUND.RIGHT_TOP, RightTop);

        connectedRooms = new List<uint>();
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile);
        tileCount++;
        boundsUpdated = false;
        centerUpdated = false;
    }
    public Vector3 GetCenter()
    {
        if (!centerUpdated)
        {
            centerUpdated = true;
            Vector3 total = new Vector3();
            foreach (Tile t in tiles)
            {
                total = total + new Vector3(t.x, 0, t.z);
            }

            center = total / tileCount;
        }
        return center;
    }

    public Dictionary<BOUND, Index> GetBounds()
    {
        if (!boundsUpdated)
        {
            boundsUpdated = true;
            // Calculate all the bounds
            Vector3 center = GetCenter();
            int top, bottom, left, right;

            top = bottom = LeftBottom.row = RightBottom.row = LeftTop.row = RightTop.row = (int)center.z;
            left = right = LeftBottom.col = RightBottom.col = LeftTop.col = RightTop.col = (int)center.x;
            
            foreach(Tile t in tiles)
            {
                // if one of the bounds is more than the last one
                // the other side of the bound may be sacrificed by 2 units
                int sacrifice = 2; 
                
                if(t.z > top)
                {
                    top = t.z;
                    // Check left top or right top  
                    if(t.x - left <= sacrifice)
                    {
                        LeftTop = t.index;
                    }else if (right - t.x <= sacrifice)
                    {
                        RightTop = t.index;
                    }
                }else if(t.z < bottom)
                {
                    bottom = t.z;
                    // check left bottom or right bottom
                    if (t.x - left <= sacrifice)
                    {
                        LeftBottom = t.index;
                    }
                    else if (right - t.x <= sacrifice)
                    {
                        RightBottom = t.index;
                    }
                }

                if(t.x > right)
                {
                    right = t.x;
                    // check right top or right bottom
                    if (t.z - bottom <= sacrifice)
                    {
                        RightBottom = t.index;
                    }
                    else if (top - t.z <= sacrifice)
                    {
                        RightTop = t.index;
                    }
                }
                else if(t.x < left)
                {
                    left = t.x;
                    // check left top or left bottom
                    if (t.z - bottom <= sacrifice)
                    {
                        LeftBottom = t.index;
                    }
                    else if (top - t.z <= sacrifice)
                    {
                        LeftTop = t.index;
                    }
                }
            }

            bounds[BOUND.LEFT_BOTTOM] = LeftBottom;
            bounds[BOUND.RIGHT_BOTTOM] = RightBottom;
            bounds[BOUND.LEFT_TOP] = LeftTop;
            bounds[BOUND.RIGHT_TOP] = RightTop;
        }
        
        return bounds;
        
    }
    
}
class RoomSet
{
    public Dictionary<uint, Room> rooms;

    public RoomSet()
    {
        rooms = new Dictionary<uint, Room>();
    }
    public void AddTile(Tile tile, uint roomID)
    {
        if (!rooms.ContainsKey(roomID))
        {            
            rooms.Add(roomID, new Room(roomID));
        }
        rooms[roomID].AddTile(tile);
    }
    void ConnectRooms(uint finalRoomID, uint otherRoomID)
    {
        // Always finalRoomID < otherRoomID
        Room finalRoom;
        Room otherRoom;

        finalRoom = rooms[finalRoomID];
        otherRoom = rooms[otherRoomID];

        finalRoom.connectedRooms.Add(otherRoom.tileGroupID);
        foreach(Tile t in otherRoom.tiles)
        {
            finalRoom.AddTile(t);
        }

        rooms[otherRoomID] = rooms[finalRoomID];

    }
    /// <summary>
    /// Apply the connections mentioned in connectSequence and reflect changes
    /// in the referenced Tilemap
    /// </summary>
    /// <param name="connectSequence"></param>
    /// <param name="tilemap"></param>
    public void ApplyConnections(Dictionary<uint, List<uint>> connectSequence, ref TileMap tilemap)
    {
        List<uint> toRemove = new List<uint>(); // The higher ids
        foreach (uint roomID1 in connectSequence.Keys)
        {
            foreach (uint roomID2 in connectSequence[roomID1])
            {
                // Create the new road tiles and then merge the roads
                Room room1 = rooms[roomID1];
                Room room2 = rooms[roomID2];
                Vector3 center1 = room1.GetCenter();
                Vector3 center2 = room2.GetCenter();

                /// As per the orientation of room1 and room2 
                /// decide the corners to be connected
                if(center1.z >= center2.z)
                {
                    if(center1.x >= center2.x)
                    {
                        tilemap.MakePath(room1.GetBounds()[Room.BOUND.LEFT_BOTTOM], room2.GetBounds()[Room.BOUND.RIGHT_TOP]);
                    }
                    else
                    {
                        tilemap.MakePath(room1.GetBounds()[Room.BOUND.RIGHT_BOTTOM], room2.GetBounds()[Room.BOUND.LEFT_TOP]);
                    }
                }
                else
                {
                    if (center1.x >= center2.x)
                    {
                        tilemap.MakePath(room1.GetBounds()[Room.BOUND.LEFT_TOP], room2.GetBounds()[Room.BOUND.RIGHT_BOTTOM]);
                    }
                    else
                    {
                        tilemap.MakePath(room1.GetBounds()[Room.BOUND.RIGHT_TOP], room2.GetBounds()[Room.BOUND.LEFT_BOTTOM]);
                    }
                }

                /// The minimum of the two room ids becomes the final roomID
                if (roomID1 < roomID2)
                {
                    ConnectRooms(roomID1, roomID2);
                    toRemove.Add(roomID2);
                }
                else if(roomID1 > roomID2)
                {
                    ConnectRooms(roomID2, roomID1);
                    toRemove.Add(roomID1);
                }
            }
        }

        // Remove the merged rooms from rooms dictionary
        foreach(uint id in toRemove)
        {
            if (rooms.Remove(id))
            {
                // Debug.Log("Remove: " + id);
            }
        }
    }
}
