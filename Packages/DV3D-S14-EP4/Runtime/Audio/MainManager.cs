using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MainManager", menuName = "Singleton/MainManager")]
public class MainManager : SingletonScriptableObject<MainManager>
{
    public static Action OnGameInitialized;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void FirstInitialize()
    {
        if (Application.isPlaying)
        {
            OnGameInitialized?.Invoke();
        }
    }
}