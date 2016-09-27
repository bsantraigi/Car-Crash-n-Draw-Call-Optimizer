using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {
    public bool DynamicGrowth;
    Dictionary<string, ObjectPool> Pools;
    Transform parent;
    static PoolManager _instance;
    const int DEFAULT_SIZE = 10;

    public static PoolManager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        parent = transform;
        Pools = new Dictionary<string, ObjectPool>();
        //StartCoroutine("PoolTester");
    }

    public void CreatePool(GameObject prefab)
    {
        if (!Pools.ContainsKey(prefab.name))
        {
            GameObject subsection = new GameObject(prefab.name + "_Pool");
            subsection.transform.parent = parent;
            Pools.Add(prefab.name, new ObjectPool(prefab, DEFAULT_SIZE, subsection.transform, DynamicGrowth));
        }
        else
        {
            Debug.LogWarning("Pool already exists for " + prefab.name);
        }
    }

    public PoolObject Reuse(string name, Vector3 position, Quaternion rotation)
    {
        if (Pools.ContainsKey(name))
        {
            return Pools[name].Reuse(position, rotation);
        }
        else
        {
            Debug.LogWarning("INVALID POOL INDEX");
            return null;
        }
    }

}
