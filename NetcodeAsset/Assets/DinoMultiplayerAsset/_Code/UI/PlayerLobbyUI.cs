using System;
using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using TMPro;
using UnityEngine;

public class PlayerLobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _NameTxt;
    [SerializeField] private TextMeshProUGUI _isReadyTxt;
    private LocalPlayer _localPlayer;
    
    public void InitPlayer(LocalPlayer localPlayer)
    {
        _localPlayer = localPlayer;
        SetName(localPlayer.DisplayName.Value);
        SetReady(localPlayer.UserStatus.Value == PlayerStatus.Ready);
        
        _localPlayer.UserStatus.onChanged += OnReadyChanged;
    }

    private void OnDestroy()
    {
        _localPlayer.UserStatus.onChanged -= OnReadyChanged;
    }

    private void OnReadyChanged(PlayerStatus status)
    {
        Debug.Log("Player Ready Changed");
        SetReady(status == PlayerStatus.Ready);
    }

    private void SetName(string name)
    {
        _NameTxt.text = name;
    }
    
    private void SetReady(bool isReady)
    {
        _isReadyTxt.text = isReady ? "Ready" : "Not Ready";
    }
    

    
}
