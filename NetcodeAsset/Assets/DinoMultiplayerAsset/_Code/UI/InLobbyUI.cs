using System.Collections;
using System.Collections.Generic;
using Dino;
using Dino.MultiplayerAsset;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InLobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI _lobbyCodeTxt;
    [SerializeField] private TextMeshProUGUI _playerCount;
    [SerializeField] private TextMeshProUGUI _maxPlayerCount;
    [SerializeField] private TextMeshProUGUI _readyPlayerText;
    [SerializeField] private Button _startGameButton;
    
    LocalLobby _localLobby;
    
    void Start()
    {
        GameNetworkManager.Instance.LocalLobby.LobbyName.onChanged += UpdateLobbyName;
        GameNetworkManager.Instance.LocalLobby.OnUserJoined += UpdatePlayerCount;
    }

    private void UpdatePlayerCount(LocalPlayer lobby)
    {
        int playerCount = GameNetworkManager.Instance.LocalLobby.PlayerCount;
        int maxPlayerCount = GameNetworkManager.Instance.LocalLobby.MaxPlayerCount.Value;
        _playerCount.text = playerCount + "/" + maxPlayerCount;
        
        bool isPrivate = GameNetworkManager.Instance.LocalLobby.Private.Value;
        UpdateLobbyCode(isPrivate);
    }

    private void UpdateLobbyName(string lobbyName)
    {
        _lobbyNameTxt.text = lobbyName;
    }
    
    private void UpdateLobbyCode(bool isPrivate)
    {
        if (isPrivate)
        {
            _lobbyCodeTxt.gameObject.SetActive(false);
        }
        else
        {
            _lobbyCodeTxt.gameObject.SetActive(true);
            _lobbyCodeTxt.text = GameNetworkManager.Instance.LocalLobby.LobbyCode.Value;
        }
        
        Debug.Log("Lobby Code: ".SetColor("#F77820") + GameNetworkManager.Instance.LocalLobby.LobbyCode.Value);
    }


    private void UpdatePlayers()
    {
        
        
    }
   
}
