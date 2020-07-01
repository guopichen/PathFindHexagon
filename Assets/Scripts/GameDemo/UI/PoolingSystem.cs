using System;
using System.Collections.Generic;
using UnityEngine;

public interface PoolingGameObjectRemote
{
    void OnEnterPool();
    void OnExitPool();
}
public class PoolingSystem
{
    public static PoolingSystem Instance = new PoolingSystem();
    private PoolingSystem()
    {

    }
    #region I need a simple pooling system to finish it nice and clean
    private class Pool
    {
        private GameObject prefab;
        private Queue<GameObject> set;
        private List<GameObject> myGoSet;
        public Pool(GameObject prefab)
        {
            this.prefab = prefab;
            set = new Queue<GameObject>();
            myGoSet = new List<GameObject>();
        }
        public void InitSize(int size)
        {
            for (int i = 0; i < size; i++)
            {
                GameObject clone = GameObject.Instantiate<GameObject>(prefab);
                clone.SetActive(false);
                set.Enqueue(clone);
                myGoSet.Add(clone);
            }
        }
        public void ReleasePool()
        {
            for (int i = myGoSet.Count - 1; i >= 0; i--)
            {
                GameObject go = myGoSet[i];
                if (go)
                    GameObject.Destroy(go);
            }
            myGoSet.Clear();
            set.Clear();
        }
        public GameObject GetOne()
        {
            if (set.Count >= 1)
            {
                GameObject result = set.Dequeue();
                if (result != null)
                {
                    PoolingGameObjectRemote remote = result.GetComponent<PoolingGameObjectRemote>();
                    remote?.OnExitPool();
                    return result;
                }
                return GetOne();
            }
            else
            {
                InitSize(2);
                return GetOne();
            }
        }

        public void ReturnObject2Pool(GameObject myclone)
        {
            if (myclone == null) return;
            //myclone.transform.SetParent(null);
            PoolingGameObjectRemote remote = myclone.GetComponent<PoolingGameObjectRemote>();
            myclone.SetActive(false);
            remote?.OnEnterPool();
            set.Enqueue(myclone);
        }
    }

    private Dictionary<int, Pool> localPoolingSet = new Dictionary<int, Pool>();
    private Dictionary<int, Pool> activeSet = new Dictionary<int, Pool>();
    private void RegistPooling(GameObject prefab, int size)
    {
        if (prefab != null)
        {
            int key = prefab.GetInstanceID();
            Pool pool = null;
            if (localPoolingSet.ContainsKey(key))
            {
                return;
            }
            pool = new Pool(prefab);
            pool.InitSize(size);
            localPoolingSet.Add(key, pool);
        }
    }
    public void FreePrefabPooling(GameObject prefab)
    {
        if (prefab == null)
            return;
        int key = prefab.GetInstanceID();
        if (localPoolingSet.ContainsKey(key))
        {
            Pool pool = localPoolingSet[key];
            pool.ReleasePool();
        }
    }

    public GameObject GetOneClone_NotActive(GameObject prefab)//yes, I want it to be inactive. It's simple
    {
        if (prefab == null)
            return null;
        Pool pool = null;
        int key = prefab.GetInstanceID();
        if (localPoolingSet.ContainsKey(key))
        {
            pool = localPoolingSet[key];
        }
        else
        {
            RegistPooling(prefab, 3);
            return GetOneClone_NotActive(prefab);
        }
        GameObject go = pool.GetOne();
        int key2 = go.GetInstanceID();
        if (activeSet.ContainsKey(key2))
        {
            activeSet[key2] = pool;
        }
        else
            activeSet.Add(key2, pool);

        return go;
    }

    public void ReturnClone(GameObject clone)
    {
        if (clone == null)
            return;
        int key = clone.GetInstanceID();
        if (activeSet.ContainsKey(key))
        {
            Pool pool = activeSet[key];
            pool.ReturnObject2Pool(clone);
        }
    }


    #endregion
}
