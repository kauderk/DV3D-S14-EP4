using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PoolsManager", menuName = "Singleton/PoolsManager")]
public class PoolsManager : SingletonScriptableObject<PoolsManager>
{
    public string WhiteList = "EditorOnly";
    public static Action<string> OnReleaseAll;
    public void ReleaseAll()
    {
        OnReleaseAll?.Invoke(WhiteList);
    }
}
