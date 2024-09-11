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
    [SerializeField] PlayerStatus _playerStatus;
    
    public void InitPlayer(LocalPlayer localPlayer)
    {
        _localPlayer = localPlayer;
        SetName(localPlayer.DisplayName.Value);
        SetReady(localPlayer.UserStatus.Value == PlayerStatus.Ready);
        
        localPlayer.UserStatus.onChanged += OnUserStatusChanged;
        
    }

    private void OnUserStatusChanged(PlayerStatus status)
    {
        SetReady(status == PlayerStatus.Ready);
    }

    private void SetName(string name)
    {
        _NameTxt.text = name;
    }
    
    private void SetReady(bool isReady)
    {
        if (!_localPlayer.IsHost.Value)
        {
            _isReadyTxt.text = isReady ? "Ready" : "Not Ready";
        }
        else
        {
            _isReadyTxt.text = "Ready";
        }
        _playerStatus = _localPlayer.UserStatus.Value;
        
    }
    

    
}
