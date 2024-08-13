using System;
using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInQueryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI _numPlayersTxt;
    [SerializeField] private TextMeshProUGUI _isPublicTxt;
    [SerializeField] private Button _joinLobbyButton;
    
    LocalLobby _localLobby;
    
    private void Start()
    {
        
    }

    public void InitLobby(LocalLobby localLobby)
    {
        _localLobby = localLobby;
        _lobbyNameTxt.text = _localLobby.LobbyName.Value;
        _numPlayersTxt.text = _localLobby.PlayerCount + "/" + _localLobby.MaxPlayerCount.Value;
        _isPublicTxt.text = _localLobby.Private.Value ? "Private" : "Public";
    }

    public void OnJoinLobby()
    {
        
    }
}
