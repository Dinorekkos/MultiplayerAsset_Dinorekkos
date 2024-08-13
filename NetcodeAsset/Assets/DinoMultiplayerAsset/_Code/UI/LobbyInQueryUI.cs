using System;
using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInQueryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI _numPlayersTxt;
    [SerializeField] private TextMeshProUGUI _isPublicTxt;
    [SerializeField] private Button _joinLobbyButton;
    
    [Header("Events")]
    [SerializeField] private GameEvent _onInitLobby;

    
    LocalLobby _localLobby;
    
    private void Start()
    {
        _joinLobbyButton.onClick.AddListener(JoinLobbyButton);
    }

    public void InitLobby(LocalLobby localLobby)
    {
        _localLobby = localLobby;
        _lobbyNameTxt.text = _localLobby.LobbyName.Value;
        _numPlayersTxt.text = _localLobby.PlayerCount + "/" + _localLobby.MaxPlayerCount.Value;
        _isPublicTxt.text = _localLobby.Private.Value ? "Private" : "Public";
    }

    public void JoinLobbyButton()
    {
        GameNetworkManager.Instance.JoinLobby(_localLobby.LobbyID.Value, _localLobby.LobbyCode.Value);
        _onInitLobby.Raise();
        
    }
}
