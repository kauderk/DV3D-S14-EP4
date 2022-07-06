using System;
using System.Collections.Generic;
using ToolBox.Pools;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [SerializeField] private int _score = 3;

    private void OnEnable()
    {
        PoolsManager.OnReleaseAll += OnReleaseAll;
    }
    private void OnDisable()
    {
        PoolsManager.OnReleaseAll -= OnReleaseAll;
    }

    private void OnReleaseAll(string WhiteList)
    {
        if (!gameObject.tag.Equals(WhiteList))
            gameObject.Release();
    }

    public int Take()
    {
        gameObject.Release();
        return _score;
    }
}
