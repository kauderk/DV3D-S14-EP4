using System;
using UnityEngine;
using Zenject;

public class EditorInput : IUserInput, ITickable
{
    public event Action OnPress = null;
    public event Action OnBeforePress = null;

    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnPress?.Invoke();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            OnBeforePress?.Invoke();
    }
}