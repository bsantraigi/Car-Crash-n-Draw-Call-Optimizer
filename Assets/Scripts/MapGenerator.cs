using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class MapGenerator : MonoBehaviour
{
    public int tileSizeX;
    public int tileSizeZ;
    public float tileHeight;
    public float tileWidth;

    public int startRow;
    public int startCol;
    public int endRow;
    public int endCol;

    public bool GenerateNewMap;
    public Image MiniMap;
    public Image PlayerMarker;
    [HideInInspector]public string selectedFile;
    public static MapGenerator Instance;
    bool _isTileMapReady;
    
    Dictionary<RoadType, Vector2> uvMap_X_Dictionary; // Ranges for x coordinate
    Dictionary<RoadType, Vector2> uvMap_Y_Dictionary; // Ranges for x coordinate
    Dictionary<Index, int> IndexToTriangle;
    public bool isTileMapReady
    {
        get { return _isTileMapReady; }
    }

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;
    Color[] miniMapColors;

    /// <summary>
    /// MESH_COMPONENTS_LOCAL
    /// Collider mesh and visible mesh are different because of uv
    /// </summary>
    private Mesh colliderMesh;
    private Mesh mesh;
    private Vector3[] colliderVertices;
    private Vector3[] vertices; // Extra vertex entries.
    private Vector2[] uv;
    private int[] colliderTriangles;
    private int[] triangles;

    TileMap _tileMap;

    public TileMap tileMap
    {
        get { return _tileMap; }
    }
    void Awake()
    {
        Instance = this;
    }
    public void SetInstance()
    {
        Instance = this;
    }
    void Start()
    {
        Init();
        CreateMap();
        _isTileMapReady = true;
    }

    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void CreateMap()
    {
        // HARD CODE
        uvMap_X_Dictionary = new Dictionary<RoadType, Vector2>();
        uvMap_Y_Dictionary = new Dictionary<RoadType, Vector2>();
        uvMap_X_Dictionary.Add(RoadType.CrissCross, new Vector2(0.2f, 0.4f));
        uvMap_X_Dictionary.Add(RoadType.Horizontal, new Vector2(0.4f, 0.6f));
        uvMap_X_Dictionary.Add(RoadType.Vertical, new Vector2(0, 0.2f));
        uvMap_X_Dictionary.Add(RoadType.Turn_LD, new Vector2(0.6f, 0.8f));
        uvMap_X_Dictionary.Add(RoadType.Turn_LU, new Vector2(0.6f, 0.8f));
        uvMap_X_Dictionary.Add(RoadType.Turn_RD, new Vector2(0.8f, 0.6f));
        uvMap_X_Dictionary.Add(RoadType.Turn_RU, new Vector2(0.8f, 0.6f));

        uvMap_Y_Dictionary.Add(RoadType.Turn_RU, new Vector2(1f, 0));
        uvMap_Y_Dictionary.Add(RoadType.Turn_LU, new Vector2(1f, 0));
        if (GenerateNewMap)
        {
            GenerateMap();
        }
        else
        {
            string path = "Assets/Saved Maps/" + selectedFile;
            _tileMap = MyFileIO.ByteArrayToObject(File.ReadAllBytes(path)) as TileMap;
            tileSizeZ = _tileMap.GetLength(0);
            tileSizeX = _tileMap.GetLength(1);

        }
        BuildMesh();
        AssignUVs();
        BuildMiniMapTexture(); // Map for minimap viewer
        ApplyMesh();
        // Temp code for testing A*
        //PathFinding pathFinder;

        //pathFinder = GetComponent<PathFinding>();
        //pathFinder.TestRun();
    }

    public void CreateMapFrom(TileMap tileMapSource)
    {
        // HARD CODE
        uvMap_X_Dictionary = new Dictionary<RoadType, Vector2>();
        uvMap_Y_Dictionary = new Dictionary<RoadType, Vector2>();
        uvMap_X_Dictionary.Add(RoadType.CrissCross, new Vector2(0.2f, 0.4f));
        uvMap_X_Dictionary.Add(RoadType.Horizontal, new Vector2(0.4f, 0.6f));
        uvMap_X_Dictionary.Add(RoadType.Vertical, new Vector2(0, 0.2f));
        uvMap_X_Dictionary.Add(RoadType.Turn_LD, new Vector2(0.6f, 0.8f));
        uvMap_X_Dictionary.Add(RoadType.Turn_LU, new Vector2(0.6f, 0.8f));
        uvMap_X_Dictionary.Add(RoadType.Turn_RD, new Vector2(0.8f, 0.6f));
        uvMap_X_Dictionary.Add(RoadType.Turn_RU, new Vector2(0.8f, 0.6f));

        uvMap_Y_Dictionary.Add(RoadType.Turn_RU, new Vector2(1f, 0));
        uvMap_Y_Dictionary.Add(RoadType.Turn_LU, new Vector2(1f, 0));


        _tileMap = tileMapSource;
        tileSizeZ = _tileMap.GetLength(0);
        tileSizeX = _tileMap.GetLength(1);
        BuildMesh();
        AssignUVs();
        BuildMiniMapTexture(); // Map for minimap viewer
        ApplyMesh();

    }

    public Vector3 IndexToWorldLocation(Index index)
    {
        Vector3 loc = transform.position;
        loc = loc + new Vector3(index.col * tileWidth, 0, index.row * tileHeight);
        return loc;
    }
    public Vector3 IndexToWorldLocation(int row, int col)
    {
        Vector3 loc = transform.position;
        loc = loc + new Vector3(col * tileWidth, 0, row * tileHeight);
        return loc;
    }
    public Index WorldPointToMapIndex(Vector3 position)
    {
        Vector3 localPos = transform.InverseTransformPoint(position);
        int col = Mathf.FloorToInt(localPos.x / tileWidth);
        int row = Mathf.FloorToInt(localPos.z / tileHeight);
        return new Index(row, col);
    }
    void GenerateMap()
    {
        _tileMap = new TileMap(tileSizeZ, tileSizeX);
        _tileMap.GenerateNewTileMap(startRow, startCol, endRow, endCol);
    }

    void BuildMesh()
    {
        int numVertX = tileSizeX + 1;
        int numVertZ = tileSizeZ + 1;

        ///***************************************************************************************
        ///***********************COLLIDER MESH SETUP*********************************************
        ///***************************************************************************************

        /// COLLIDER MESH ONLY NEEDS A RECTANGLE WITH 4 VERTICES
        colliderVertices = new Vector3[4];
        colliderTriangles = new int[6];
        colliderMesh = new Mesh();

        int k = 0;
        for (int z = 0; z < numVertZ;)
        {
            for (int x = 0; x < numVertX;)
            {
                colliderVertices[k++] = new Vector3(x * tileWidth, 0, z * tileHeight);
                x += numVertX - 1;
            }
            z += numVertZ - 1;
        }
        colliderTriangles[0] = 2;
        colliderTriangles[1] = 1;
        colliderTriangles[2] = 0;

        colliderTriangles[3] = 2;
        colliderTriangles[4] = 3;
        colliderTriangles[5] = 1;

        colliderMesh.vertices = colliderVertices;
        colliderMesh.triangles = colliderTriangles;
        colliderMesh.RecalculateNormals();

        ///***************************************************************************************
        ///***********************VISIBLE MESH SETUP**********************************************
        ///***************************************************************************************
        int visibleSize = 4 * (numVertX - 2) * (numVertZ - 2) + 2 * 2 * (numVertX + numVertZ - 4) + 4;
        vertices = new Vector3[visibleSize];
        mesh = new Mesh();
        int step1 = (numVertX - 2) * (numVertZ - 2);
        int step2 = 2 * step1;
        int step3 = 3 * step1;
        // SET THE INNER POINTS OF THE VISIBLE MESH
        k = 0;
        for (int z = 1; z < numVertZ - 1; z++)
        {
            for (int x = 1; x < numVertX - 1; x++)
            {
                Vector3 v = new Vector3(x * tileWidth, 0, z * tileHeight);
                vertices[k] = v;
                vertices[k + step1] = v;
                vertices[k + step2] = v;
                vertices[k + step3] = v;
                k++;
            }
        }
        k = 4 * step1; // Set the current index to last added point

        // BOUNDARY POINTS
        step1 = 2 * (numVertZ - 2);
        for (int z = 1; z < numVertZ - 1; z++)
        {
            foreach (int x in new int[] { 0, numVertX - 1 })
            {
                vertices[k] = new Vector3(x * tileWidth, 0, z * tileHeight);
                vertices[k + step1] = vertices[k];
                k++;
            }

        }

        k += step1; // Set the current index to last added point


        step1 = 2 * (numVertX - 2);
        for (int x = 1; x < numVertX - 1; x++)
        {
            foreach (int z in new int[] { 0, numVertZ - 1 })
            {
                vertices[k] = new Vector3(x * tileWidth, 0, z * tileHeight);
                vertices[k + step1] = vertices[k];
                k++;
            }
        }
        k += step1; // Set the current index to last added point

        // CORNER POINTS
        vertices[k++] = colliderVertices[0];
        vertices[k++] = colliderVertices[1];
        vertices[k++] = colliderVertices[2];
        vertices[k++] = colliderVertices[3];

        // Create the triangles
        triangles = new int[3 * 2 * tileSizeX * tileSizeZ];
        IndexToTriangle = new Dictionary<Index, int>();

        // NON-BOUNDARY TRIANGLES
        step1 = (numVertX - 2) * (numVertZ - 2);
        step2 = 2 * step1;
        step3 = 3 * step1;
        k = 0;
        int _mod_numVertX = numVertX - 2;
        for (int z = 1; z < tileSizeZ - 1; z++)
        {
            int modZ = z - 1;
            for (int x = 1; x < tileSizeX - 1; x++)
            {
                int modX = x - 1;
                IndexToTriangle.Add(new Index(z, x), k);
                triangles[k++] = (modZ + 1) * _mod_numVertX + modX + step1;
                triangles[k++] = modZ * _mod_numVertX + modX + 1 + step3;
                triangles[k++] = modZ * _mod_numVertX + modX + step2;

                triangles[k++] = (modZ + 1) * _mod_numVertX + modX + step1;
                triangles[k++] = (modZ + 1) * _mod_numVertX + modX + 1;
                triangles[k++] = modZ * _mod_numVertX + modX + 1 + step3;
            }
        }

        int offset = 4 * step1;
        int step = 2 * (numVertZ - 2);
        // Non corner boundaries
        // vertical boundaries
        for (int z = 1; z < tileSizeZ - 1; z++)
        {
            int modZ = z - 1;
            int x = 0;
            IndexToTriangle.Add(new Index(z, x), k);

            triangles[k++] = (modZ + 1) * 2 + offset;
            triangles[k++] = modZ * _mod_numVertX + step3;
            triangles[k++] = modZ * 2 + offset + step;

            triangles[k++] = (modZ + 1) * 2 + offset;
            triangles[k++] = (modZ + 1) * _mod_numVertX;
            triangles[k++] = modZ * _mod_numVertX + step3;

            x = tileSizeX - 2;
            IndexToTriangle.Add(new Index(z, x + 1), k);

            triangles[k++] = (modZ + 1) * (_mod_numVertX) + x + step1;
            triangles[k++] = offset + modZ * 2 + 1 + step;
            triangles[k++] = (modZ) * (_mod_numVertX) + x + step2;

            triangles[k++] = (modZ + 1) * (_mod_numVertX) + x + step1;
            triangles[k++] = offset + (modZ + 1) * 2 + 1;
            triangles[k++] = offset + modZ * 2 + 1 + step;
        }

        // Horizontal boundaries
        offset = 4 * step1 + 4 * (numVertZ - 2);
        step = 2 * (numVertZ - 2);
        for (int x = 1; x < tileSizeX - 1; x++)
        {
            int modX = x - 1;
            int z = 0;
            IndexToTriangle.Add(new Index(z, x), k);

            triangles[k++] = modX + step1;
            triangles[k++] = offset + (modX + 1) * 2;
            triangles[k++] = offset + (modX) * 2 + step;

            triangles[k++] = modX + step1;
            triangles[k++] = (modX + 1);
            triangles[k++] = offset + (modX + 1) * 2;

            z = tileSizeZ - 2;
            IndexToTriangle.Add(new Index(z + 1, x), k);

            triangles[k++] = offset + (modX) * 2 + step + 1;
            triangles[k++] = z * _mod_numVertX + modX + 1 + step3;
            triangles[k++] = z * _mod_numVertX + modX + step2;

            triangles[k++] = offset + (modX) * 2 + step + 1;
            triangles[k++] = offset + (modX + 1) * 2 + 1;
            triangles[k++] = z * _mod_numVertX + modX + 1 + step3;
        }

        // CORNER BOXES
        #region BOTTOM_LEFT
        IndexToTriangle.Add(new Index(0, 0), k);

        triangles[k++] = 4 * step1;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2);
        triangles[k++] = visibleSize - 4;

        triangles[k++] = 4 * step1;
        triangles[k++] = 0;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2);
        #endregion

        #region BOTTOM_RIGHT
        IndexToTriangle.Add(new Index(0, tileSizeX - 1), k);

        triangles[k++] = numVertX - 3 + step1;
        triangles[k++] = visibleSize - 3;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) + (_mod_numVertX - 1) * 2 + 2 * (numVertX - 2);

        triangles[k++] = numVertX - 3 + step1;
        triangles[k++] = 4 * step1 + 1;
        triangles[k++] = visibleSize - 3;
        #endregion

        #region TOP_LEFT
        IndexToTriangle.Add(new Index(tileSizeZ - 1, 0), k);

        triangles[k++] = visibleSize - 2;
        triangles[k++] = _mod_numVertX * (numVertZ - 3) + step3;
        triangles[k++] = 4 * step1 + (numVertZ - 3) * 2 + 2 * (numVertX - 2);

        triangles[k++] = visibleSize - 2;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) + 1;
        triangles[k++] = _mod_numVertX * (numVertZ - 3) + step3;
        #endregion

        #region TOP_RIGHT
        IndexToTriangle.Add(new Index(tileSizeZ - 1, tileSizeX - 1), k);

        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) + 4 * (numVertX - 2) - 1;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) - 1;
        triangles[k++] = step3 - 1;

        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) + 4 * (numVertX - 2) - 1;
        triangles[k++] = visibleSize - 1;
        triangles[k++] = 4 * step1 + 4 * (numVertZ - 2) - 1;
        #endregion

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    void AssignUVs()
    {
        // NEW UV ARRAY
        uv = new Vector2[vertices.Length];

        // SET THE UV COORDS FOR ALL THE VERTICES IN RIGHT ORDER
        for (int i = 0; i < triangles.Length; i += 6)
        {

            SetUVToTri(i);
        }

        // Update the mesh
        mesh.uv = uv;
    }
    public void ReCalculateRoadTypes()
    {
        _tileMap.SetRoadTypes();
        AssignUVs();
        meshFilter.sharedMesh.uv = uv;
    }
    void SetUVToTri(int triFirstVert)
    {
        int[] a = new int[]
            {
                triangles[triFirstVert],
                triangles[triFirstVert+1],
                triangles[triFirstVert+2]
            };
        int[] b = new int[]
        {
                triangles[triFirstVert+3],
                triangles[triFirstVert+4],
                triangles[triFirstVert+5]
        };

        //****************************************************************************//
        //****************************************************************************//
        //****ALERT:*******SLOW CODE *************SLOW CODE***************************//
        //**************************SLOW CODE*****************************************//
        Index tileIndex = WorldPointToMapIndex((vertices[a[2]] + vertices[b[1]]) / 2);
        //****************************************************************************//

        float x_start = 0.8f;
        float x_end = 1;
        float y_start = 0;
        float y_end = 1;

        RoadType roadType = _tileMap[tileIndex].roadType;
        if (uvMap_X_Dictionary.ContainsKey(roadType))
        {
            Vector2 v = uvMap_X_Dictionary[roadType];
            x_start = v.x;
            x_end = v.y;
        }
        if (uvMap_Y_Dictionary.ContainsKey(roadType))
        {
            Vector2 v = uvMap_Y_Dictionary[roadType];
            y_start = v.x;
            y_end = v.y;
        }
        uv[a[0]] = new Vector2(x_start, y_end);
        uv[a[1]] = new Vector2(x_end, y_start);
        uv[a[2]] = new Vector2(x_start, y_start);

        uv[b[0]] = new Vector2(x_start, y_end);
        uv[b[1]] = new Vector2(x_end, y_end);
        uv[b[2]] = new Vector2(x_end, y_start);
    }
    public void ChangeTile(Index index, TileType newType)
    {
        if (_tileMap[index].type == newType)
        {
            // Do nothing
            return;
        }
        else
        {
            _tileMap[index].type = newType;
            if(newType == TileType.ROAD)
            {
                _tileMap[index].roadType = RoadType.CrissCross;
            }
            else
            {
                _tileMap[index].roadType = RoadType.None;
            }
            // Index to triangles
            int triIndex = IndexToTriangle[index];
            // Update uv
            SetUVToTri(triIndex);
            meshFilter.sharedMesh.uv = uv;
        }
    }
    void ApplyMesh()
    {
        // Setup Collider Mesh
        colliderMesh.name = "ColliderMesh";
        meshCollider.sharedMesh = colliderMesh;

        // Setup Visible Mesh
        mesh.name = "VisibleMesh";
        meshFilter.mesh = mesh;
    }

    void BuildMiniMapTexture()
    {
        miniMapColors = new Color[tileSizeZ*tileSizeX];
        Color road = new Color(63/255f, 73/255f, 83/255f);
        Color ground = new Color(60/255f, 153/255f, 64/255f);
        for (int z = 0; z < tileSizeZ; z++)
        {
            for (int x = 0; x < tileSizeX; x++)
            {
                if (tileMap[z, x].type == TileType.ROAD)
                {
                    miniMapColors[z * tileSizeX + x] = road;
                }
                else
                {
                    miniMapColors[z * tileSizeX + x] = ground;
                }
            }
        }
        
        Texture2D texture2d = new Texture2D(tileSizeZ, tileSizeX);
        texture2d.SetPixels(miniMapColors);
        texture2d.Apply();
        texture2d.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(texture2d, new Rect(0, 0, tileSizeZ, tileSizeX), Vector2.zero);
        
        MiniMap.sprite = sprite;
        // Encode texture into PNG
        //byte[] bytes = texture2d.EncodeToPNG();
        //Object.Destroy(texture2d);

        // For testing purposes, also write to a file in the project folder
        //System.IO.File.WriteAllBytes(Application.dataPath + "/Temp/SavedScreen.png", bytes);
        //Debug.Log(Application.dataPath + "/Temp/SavedScreen.png");
    }

    public void UpdateMinimap(Vector3 worldPos, Vector3 EulerAngles)
    {
        Index i = WorldPointToMapIndex(worldPos);
        Vector3 position = MiniMap.rectTransform.anchoredPosition;
        position.x = 100 - worldPos.x * 1.667f; // 1.4286 is 10/6
        position.y = 100 - worldPos.z * 1.667f;
        MiniMap.rectTransform.anchoredPosition = position;

        Vector3 markerAngles = PlayerMarker.rectTransform.eulerAngles;
        markerAngles.z = -EulerAngles.y;
        PlayerMarker.rectTransform.eulerAngles = markerAngles;
    }

    
}
