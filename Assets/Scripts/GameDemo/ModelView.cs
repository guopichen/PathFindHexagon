using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ModelAssets", menuName = "PathFind/ModelAssets")]

public class ModelView : ScriptableObject
{
    [SerializeField]
    private GameObject prefab;

    public GameObject GetPrefab()
    {
        return prefab;
    }


    public string GetBodyTransformName()
    {
        if (prefab != null)
            return prefab.name.Substring(0, 9);
        return string.Empty;
    }

    public string GetWeaponTransformName()
    {
        string body = GetBodyTransformName();
        if (string.IsNullOrEmpty(body))
            return string.Empty;
        return body + "_Weapon";
    }
}
