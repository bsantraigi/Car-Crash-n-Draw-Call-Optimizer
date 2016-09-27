using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TownBuilder : MonoBehaviour {
    struct Cache
    {
        public static int groupZ, groupX;
    }
    public GameObject[] TownProps;
    public int defaultPoolGridSize = 7;
    public bool DCO = false;
    PoolObject[,] ActiveTownProps;
    Dictionary<int, int> Col2IndexMap;
    Dictionary<int, int> Row2IndexMap;

    TileMap tileMap;
    MapGenerator mapGenerator;
    Index playerIndex;
    bool loaded = false;
    PoolManager poolManager;
    Vector3 tileSpawnOffset;
    GameObject[,] townBlocks;
    bool[,] townBlockStatus;
    const int groupSize = 5;
    int groupCount;

    void Start()
    {
        StartCoroutine(WaitForGameLoad());
    }

    void FixedUpdate()
    {
        if (loaded)
        {
            
        }
        if (GamePlayController.started)
        {
            Index i = CarUserController.instance.index;
            if (!playerIndex.Equals(i))
            {
                int groupZ = i.row / 5;
                int groupX = i.col / 5;
                if (Cache.groupZ != groupZ || Cache.groupX != groupX)
                {
                    #region HIDING_BLOCKS
                    townBlocks[Cache.groupZ, Cache.groupX].SetActive(false);
                    if (Cache.groupZ + 1 < groupCount)
                    {
                        townBlocks[Cache.groupZ + 1, Cache.groupX].SetActive(false);
                        if (Cache.groupX + 1 < groupCount)
                        {
                            townBlocks[Cache.groupZ + 1, Cache.groupX + 1].SetActive(false);
                        }
                        if (Cache.groupX - 1 > 0)
                        {
                            townBlocks[Cache.groupZ + 1, Cache.groupX - 1].SetActive(false);
                        }
                    }
                    if (Cache.groupZ - 1 > 0)
                    {
                        townBlocks[Cache.groupZ - 1, Cache.groupX].SetActive(false);
                        if (Cache.groupX - 1 > 0)
                        {
                            townBlocks[Cache.groupZ - 1, Cache.groupX - 1].SetActive(false);
                        }
                        if (Cache.groupX + 1 < groupCount)
                        {
                            townBlocks[Cache.groupZ - 1, Cache.groupX + 1].SetActive(false);
                        }
                    }
                    if (Cache.groupX + 1 < groupCount)
                    {
                        townBlocks[Cache.groupZ, Cache.groupX + 1].SetActive(false);
                    }
                    if (Cache.groupX - 1 > 0)
                    {
                        townBlocks[Cache.groupZ, Cache.groupX - 1].SetActive(false);
                    }
                    #endregion

                    #region ACTIVATING_BLOCKS
                    townBlocks[groupZ, groupX].SetActive(true);
                    if (groupZ + 1 < groupCount)
                    {
                        townBlocks[groupZ + 1, groupX].SetActive(true);
                        if (groupX + 1 < groupCount)
                        {
                            townBlocks[groupZ + 1, groupX + 1].SetActive(true);
                        }
                        if (groupX - 1 > 0)
                        {
                            townBlocks[groupZ + 1, groupX - 1].SetActive(true);
                        }
                    }
                    if (groupZ - 1 > 0)
                    {
                        townBlocks[groupZ - 1, groupX].SetActive(true);
                        if (groupX - 1 > 0)
                        {
                            townBlocks[groupZ - 1, groupX - 1].SetActive(true);
                        }
                        if (groupX + 1 < groupCount)
                        {
                            townBlocks[groupZ - 1, groupX + 1].SetActive(true);
                        }
                    }
                    if (groupX + 1 < groupCount)
                    {
                        townBlocks[groupZ, groupX + 1].SetActive(true);
                    }
                    if (groupX - 1 > 0)
                    {
                        townBlocks[groupZ, groupX - 1].SetActive(true);
                    }
                    #endregion

                    Cache.groupX = groupX;
                    Cache.groupZ = groupZ;
                }
            }
            //Vector3 start = CarUserController.instance.position;
            //Debug.DrawLine(start, start + new Vector3(DrawLength, 0, 0), Color.red);
            //Debug.DrawLine(start, start + new Vector3(-DrawLength, 0, 0), Color.red);
            //Debug.DrawLine(start, start + new Vector3(0, 0, DrawLength), Color.red);
            //Debug.DrawLine(start, start + new Vector3(0, 0, -DrawLength), Color.red);
            //if (Input.GetKey(KeyCode.KeypadPlus))
            //{
            //    DrawLength += Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.KeypadMinus))
            //{
            //    DrawLength -= Time.deltaTime;
            //}
        }
    }

    IEnumerator WaitForGameLoad()
    {
        while (!GamePlayController.Instance.Loaded)
        {
            yield return new WaitForSeconds(0.2f);
        }
        loaded = true;
        mapGenerator = GetComponent<MapGenerator>();
        tileMap = mapGenerator.tileMap;
        tileSpawnOffset = new Vector3(mapGenerator.tileWidth / 7, 0, mapGenerator.tileHeight / 7);
        StartCoroutine(AddTownProps());
    }
    IEnumerator AddTownProps()
    {
        GameObject mainParent = new GameObject("Parent");
        
        
        groupCount = mapGenerator.tileSizeX / groupSize;

        townBlocks = new GameObject[groupCount, groupCount];
        townBlockStatus = new bool[groupCount, groupCount];

        for (int g = 0; g < groupCount; g++)
        {
            for (int g2 = 0; g2 < groupCount; g2++)
            {
                GameObject group = new GameObject(string.Format("Group{0}.{1}", g, g2));
                group.transform.parent = mainParent.transform;
                townBlocks[g, g2] = group;
                townBlockStatus[g, g2] = false;
                int startRow = g * groupSize;
                int endRow = (g + 1) * groupSize;
                int startCol = g2 * groupSize;
                int endCol = (g2 + 1) * groupSize;
                for (int i = startRow; i < endRow; i++)
                {
                    for (int j = startCol; j < endCol; j++)
                    {
                        Tile tile = tileMap[i, j];
                        if (tile.type == TileType.GROUND)
                        {
                            int k = Random.Range(0, TownProps.Length);
                            GameObject go = Instantiate(TownProps[k], mapGenerator.IndexToWorldLocation(i, j), Quaternion.identity) as GameObject;
                            go.transform.parent = group.transform;
                        }
                    }
                }
                if(DCO)
                    townBlocks[g, g2].AddComponent<DrawCallOptimizer>();
                yield return null;
                townBlocks[g, g2].SetActive(false);
            }
        }
        Debug.Log("Hoo!! we are done here.");
        playerIndex = CarUserController.instance.index;
        GamePlayController.started = true;


        // Initial Setup
        
        Index index = CarUserController.instance.index;
        int groupZ = index.row / groupSize;
        int groupX = index.col / groupSize;

        Cache.groupZ = groupZ;
        Cache.groupX = groupX;
        townBlocks[groupZ, groupX].SetActive(true);

        if (groupZ + 1 < groupCount)
        {
            townBlocks[groupZ + 1, groupX].SetActive(true);
            if (groupX + 1 < groupCount)
            {
                townBlocks[groupZ + 1, groupX + 1].SetActive(true);
            }
            if (groupX - 1 > 0)
            {
                townBlocks[groupZ + 1, groupX - 1].SetActive(true);
            }
        }
        if (groupZ - 1 > 0)
        {
            townBlocks[groupZ - 1, groupX].SetActive(true);
            if (groupX - 1 > 0)
            {
                townBlocks[groupZ - 1, groupX - 1].SetActive(true);
            }
            if (groupX + 1 < groupCount)
            {
                townBlocks[groupZ - 1, groupX + 1].SetActive(true);
            }
        }
        if (groupX + 1 < groupCount)
        {
            townBlocks[groupZ, groupX + 1].SetActive(true);
        }
        if (groupX - 1 > 0)
        {
            townBlocks[groupZ, groupX - 1].SetActive(true);
        }
    }
}
