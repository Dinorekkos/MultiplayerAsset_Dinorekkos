using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game Events/Game Event")]
public class GameEvent : ScriptableObject
{
    public event Action OnEventRaised;
    
    public void Raise()
    {
        OnEventRaised?.Invoke();
    }
    
}
