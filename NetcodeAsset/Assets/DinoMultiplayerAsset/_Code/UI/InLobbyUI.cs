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
    
    [Header("InLobbyUI")]
    
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI _lobbyCodeTxt;
    [SerializeField] private TextMeshProUGUI _playerCount;
    [SerializeField] private TextMeshProUGUI _maxPlayerCount;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private GameObject _playerContainer;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _container;

    LocalLobby _localLobby;
    private int _currentPlayers;
    
    void Start()
    {
        GameNetworkManager.Instance.LocalLobby.LobbyName.onChanged += UpdateLobbyName;
        GameNetworkManager.Instance.LocalLobby.OnUserJoined += UpdatePlayerCount;
        _startGameButton.onClick.AddListener(StartGame);
     
        _container.SetActive(false);
    }

    private void UpdatePlayerCount(LocalPlayer localPlayer)
    {
        _localLobby = GameNetworkManager.Instance.LocalLobby;
        int playerCount = _localLobby.PlayerCount;
        int maxPlayerCount = _localLobby.MaxPlayerCount.Value;
        _playerCount.text = playerCount + "/" + maxPlayerCount;
        
        bool isPrivate = _localLobby.Private.Value;
        UpdateLobbyCode(isPrivate);
        UpdatePlayers();
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
        
        // Debug.Log("Lobby Code: ".SetColor("#F77820") + GameNetworkManager.Instance.LocalLobby.LobbyCode.Value);
    }


    private void UpdatePlayers()
    {
        foreach (Transform child in _playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var player in _localLobby.LocalPlayers)
        {
            GameObject playerUI = Instantiate(_playerPrefab, _playerContainer.transform);
            PlayerLobbyUI playerLobbyUI = playerUI.GetComponent<PlayerLobbyUI>();
            playerLobbyUI.SetName(player.DisplayName.Value);
            playerLobbyUI.SetReady(false);
            Debug.Log("PLayer Status ".SetColor("#20D0F7") + player.UserStatus.Value);
        }
        
    }
    
    private void StartGame()
    {
        // GameNetworkManager.Instance.StartGame();
    }
   
}
