using System;
using UnityEngine;
using Zenject;

public class MobileInput : IUserInput, ITickable
{
    public event Action OnPress = null;
    public event Action OnBeforePress = null;

    public void Tick()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.fingerId == 0 && touch.phase == TouchPhase.Began)
            {
                // BAD IMPLETATION, but I don't know any better...
                // I hardly ever use Mobile stuff
                // I do care, I just don't know any better
                OnBeforePress?.Invoke();
                OnPress?.Invoke();
            }

        }
    }
}