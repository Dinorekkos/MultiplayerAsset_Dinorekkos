using System;
using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    
    GameNetworkManager _gameNetworkManager;
    private void Start()
    {
        InitMiniGame();
    }
    
    public void InitMiniGame()
    {
        _gameNetworkManager = GameNetworkManager.Instance;
        _gameNetworkManager.OnClientConnected += OnClientConnected;
       
    }

    private void OnClientConnected(ulong obj)
    {
        
    }
}
