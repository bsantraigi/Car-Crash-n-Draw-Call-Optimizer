using System.Collections.Generic;

public enum TileType
{
    GROUND,
    ROAD,
    BUILDING
}

[System.Serializable]
public enum RoadType
{
    None, 
    Horizontal,
    Vertical,
    CrissCross,
    Turn_LU,
    Turn_LD,
    Turn_RU,
    Turn_RD
}

[System.Serializable]
public class Index
{
    public int row;
    public int col;
    public Index(int _i, int _j)
    {
        row = _i;
        col = _j;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", row, col);
    }

    public override bool Equals(object obj)
    {
        Index i = obj as Index;
        if(i == null)
        {
            return false;
        }
        else
        {
            return (i.row == this.row) && (i.col == this.col);
        }
    }
    public override int GetHashCode()
    {
        // ASSUMING MAX SIZE OF GRID WILL ALWAYS BE LESS THAN 100
        return (row << 16) + col;
    }
}

[System.Serializable]
public class Tile : IHeapItem
{
    public uint groupID = 0;
    public bool isWalkable
    {
        get { return type == TileType.ROAD; }
    }
    public int gScore
    {
        get
        {
            if (!tileMap.isScored[index.row, index.col])
            {
                tileMap.isScored[index.row, index.col] = true;
                tileMap.gScores[index.row, index.col] = int.MaxValue;
                return int.MaxValue;
            }
            return tileMap.gScores[index.row, index.col];
        }
        set
        {
            tileMap.isScored[index.row, index.col] = true;
            tileMap.gScores[index.row, index.col] = value;
        }
    }
    public int hScore
    {
        get
        {
            return tileMap.hScores[index.row, index.col];
        }
        set
        {
            tileMap.hScores[index.row, index.col] = value;
        }
    }
    public int fScore
    {
        get { return gScore + hScore; }
    }
    public Index parent
    {
        // Helper field for A* algorithm
        get
        {
            return tileMap.parents[index.row, index.col];
        }
        set
        {
            tileMap.parents[index.row, index.col] = value;
        }
    }

    TileType tileType;
    int col;
    int row;
    Index _index;

    List<Index> neighbours;

    List<Index> _8_Neighbours;
    bool _visited = false;
    int _heapIndex;
    private TileMap tileMap;
    public RoadType roadType;

    public int HeapIndex
    {
        get { return _heapIndex; }
        set { _heapIndex = value; }
    }

    public int x
    {
        get { return col; }
    }
    public int z
    {
        get { return row; }
    }
    public List<Index> neighbourList
    {
        get { return neighbours; }
    }
    public List<Index> _8_NeighbourList
    {
        get { return _8_Neighbours; }
    }
    public TileType type
    {
        get { return tileType; }
        set { tileType = value; }
    }
    public Tile(int z, int x, TileMap _tileMap, TileType _tileType = TileType.GROUND, RoadType _roadType = RoadType.None)
    {
        this.row = z;
        this.col = x;
        neighbours = new List<Index>();
        _8_Neighbours = new List<Index>();
        _index = new Index(z, x);
        tileType = _tileType;
        tileMap = _tileMap;
        roadType = _roadType;
    }
    public bool visited
    {
        get { return _visited; }
    }
    public Index index
    {
        get { return _index; }
    }
    bool addValidToNeighbours(int row, int col, int rowCount, int colCount, bool EightNeighbours = false)
    {
        if (row < 0 || row > rowCount - 1 || col < 0 || col > colCount - 1)
        {
            return false;
        }
        if (!EightNeighbours)
        {
            neighbours.Add(new Index(row, col));
        }
        else
        {
            _8_Neighbours.Add(new Index(row, col));
        }
        return true;
    }

    public void SetNeighbours(int rowCount, int colCount)
    {
        int z = row;
        int x = col;
        addValidToNeighbours(z, x - 1, rowCount, colCount);
        addValidToNeighbours(z, x + 1, rowCount, colCount);
        addValidToNeighbours(z + 1, x, rowCount, colCount);
        addValidToNeighbours(z - 1, x, rowCount, colCount);
    }

    public void Set_8_Neighbours(int rowCount, int colCount)
    {
        int z = row;
        int x = col;
        addValidToNeighbours(z, x - 1, rowCount, colCount, true);
        addValidToNeighbours(z, x + 1, rowCount, colCount, true);
        addValidToNeighbours(z + 1, x, rowCount, colCount, true);
        addValidToNeighbours(z - 1, x, rowCount, colCount, true);

        addValidToNeighbours(z + 1, x - 1, rowCount, colCount, true);
        addValidToNeighbours(z + 1, x + 1, rowCount, colCount, true);
        addValidToNeighbours(z - 1, x - 1, rowCount, colCount, true);
        addValidToNeighbours(z - 1, x + 1, rowCount, colCount, true);
    }

    public void Visit()
    {
        _visited = true;
    }

    public void UnVisit()
    {
        _visited = false;
    }

    public override bool Equals(object other)
    {
        Tile n = other as Tile;
        return (n.x == this.x) && (n.z == this.z);
    }
    public int CompareTo(object obj)
    {
        Tile t = obj as Tile;
        return this.fScore.CompareTo(t.fScore);
    }

}