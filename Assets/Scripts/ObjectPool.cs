using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TARGET
// To reuse object
// To make the hierarchy clean
[System.Serializable]
public class ObjectPool {

    GameObject Prefab;
    Transform parentTransform;
    Queue<PoolObject> Pool;
    bool DynamicGrowth = false;

    public ObjectPool(GameObject _prefab, int StartSize, Transform _parentTransform, bool _dynamicGrowth = false)
    {
        Prefab = _prefab;
        Pool = new Queue<PoolObject>();
        parentTransform = _parentTransform;
        while(Pool.Count < StartSize)
        {
            GameObject go = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity) as GameObject;
            Pool.Enqueue(new PoolObject(go, this));
            go.transform.parent = parentTransform;
        }
        DynamicGrowth = _dynamicGrowth;

    }

    public PoolObject Reuse(Vector3 position, Quaternion rotation)
    {
        if (Pool.Count > 0)
        {
            PoolObject instance = Pool.Dequeue();
            instance.Reuse(position, rotation);
            return instance;
        }
        else
        {
            GameObject go = Object.Instantiate(Prefab, position, rotation) as GameObject;
            go.transform.parent = parentTransform;
            PoolObject instance = new PoolObject(go, this);
            instance.Reuse(position, rotation);
            return instance;
        }
    }

    public void ReturnObject(PoolObject po)
    {
        Pool.Enqueue(po);
    }
}

public class PoolObject
{
    GameObject _gameObject;
    Transform transform;
    ObjectPool pool;

    public GameObject gameObject
    {
        get
        {
            return _gameObject;
        }
    }

    public PoolObject(GameObject _gameObject, ObjectPool _pool)
    {
        this._gameObject = _gameObject;
        transform = this._gameObject.transform;
        this._gameObject.SetActive(false);
        pool = _pool;
    }

    public void Restore()
    {
        _gameObject.SetActive(false);
        pool.ReturnObject(this);
    }

    public void Reuse(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        _gameObject.SetActive(true);
    }
}
