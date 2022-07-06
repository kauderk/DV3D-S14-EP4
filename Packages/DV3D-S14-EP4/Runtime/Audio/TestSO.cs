using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "TestSO", menuName = "Audio/TestSO")]
public class TestSO : ManagedObject
{
    protected override void OnBegin()
    {
        //Debug.Log("TestSO.OnEnable()");
    }
    public void Awake()
    {
        //Debug.Log("TestSO.Awake()");
    }
    protected override void OnEnd()
    {
        //Debug.Log("TestSO.OnDisable()");
    }
    public void OnDestroy()
    {
        //Debug.Log("TestSO.OnDestroy()");
    }
}


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public abstract class ManagedObject : ScriptableObject
{
    abstract protected void OnBegin();
    abstract protected void OnEnd();

#if UNITY_EDITOR
    protected void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayStateChange;
    }

    protected void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayStateChange;
    }

    void OnPlayStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            OnBegin();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            OnEnd();
        }
    }
#else
        protected void OnEnable()
        {
            OnBegin();
        }
 
        protected void OnDisable()
        {
            OnEnd();
        }
#endif
}