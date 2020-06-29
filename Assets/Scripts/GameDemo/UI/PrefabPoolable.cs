using UnityEngine;
using System.Collections;
using System.Collections.Generic;

interface PrefabPoolableRemote
{
    //GameObject CreatePrefab(GameObject prefab);//创建一个实例并加入pooling
    GameObject CreatePrefab(string prefabpath);//根据prefab路径完成创建
    void FreePooling();//主动释放引用，通常在parentGo被destory时调用
}
/// <summary>
/// 作用为解决使用GameObject.Instantiate<GameObject>(go);后留下的内存问题
/// 
/// 每次创建一个prefab实例，将其加入prefabinstances中，等到父节点被destory时，主动调用释放引用。从而GC
/// </summary>
public class PrefabPoolable:PrefabPoolableRemote {

    private GameObject parentGo;//定义这次所生成的prefab属于谁

    private Queue<GameObject> poolingInstance;//prefab生成后的引用
    private Dictionary<string,GameObject> prefabMap;

	public PrefabPoolable(GameObject parentGo)
    {
        this.parentGo = parentGo;
        poolingInstance = new Queue<GameObject>();
        prefabMap = new Dictionary<string,GameObject>();
    }

    #region 接口
    GameObject PrefabPoolableRemote.CreatePrefab(string prefabpath)
    {
        GameObject prefab = null;
        if(prefabMap.ContainsKey(prefabpath))
        {
            prefabMap.TryGetValue(prefabpath, out prefab);
            return CreatePrefab(prefab);
        }
        else
        {
            prefab = Resources.Load<GameObject>(prefabpath);
            prefabMap.Add(prefabpath, prefab);
            return CreatePrefab(prefab);
        }
    }
    private GameObject CreatePrefab(GameObject prefab)
    {
        GameObject aInstance = parentGo.InstantiateAndSetFalseActive(prefab);
        Transform aInstanceT = aInstance.transform;
        aInstanceT.SetParent(parentGo.transform);
        poolingInstance.Enqueue(aInstance);
        return aInstance;
    }

    void PrefabPoolableRemote.FreePooling()
    {
        GameObject pooling ;
        while (poolingInstance.Count > 0)
        {
            pooling = poolingInstance.Dequeue();
            GameObject.Destroy(pooling);
            pooling = null;
        }
        prefabMap.Clear();
       
        Resources.UnloadUnusedAssets();
    }
    #endregion

   
}
public static class ExpandGameObject
{

    //实例化并false掉
    public static GameObject InstantiateAndSetFalseActive(this GameObject go, GameObject original)
    {
        GameObject clone = GameObject.Instantiate<GameObject>(original);
        clone.SetActive(false);
        return clone;
    }
}