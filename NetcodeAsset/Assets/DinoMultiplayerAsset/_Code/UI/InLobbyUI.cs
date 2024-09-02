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
    [Header("Game Events")]
    [SerializeField] private GameEvent _onInitLobby;
    
    [Header("InLobbyUI")]
    
    [SerializeField] private TextMeshProUGUI _lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI _lobbyCodeTxt;
    [SerializeField] private TextMeshProUGUI _playerCount;
    [SerializeField] private TextMeshProUGUI _maxPlayerCount;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private GameObject _playerContainer;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _container;
    [SerializeField] private Button _quitLobbyButton;
    [SerializeField] private GameEvent _onReturnMenu;

    LocalPlayer _localPlayer;
    LocalLobby _localLobby;
    private int _currentPlayers;
    
    
    void Start()
    {
        _container.SetActive(false);
        Initialize();
    }

    private void Initialize()
    {
        _onInitLobby.OnEventRaised += InitLobby;
        
        GameNetworkManager.Instance.LocalLobby.LobbyName.onChanged += UpdateLobbyName;
        GameNetworkManager.Instance.LocalLobby.OnUserJoined += UpdatePlayers;
        GameNetworkManager.Instance.LocalLobby.OnUserLeft += OnUserLeft;
        GameNetworkManager.Instance.LocalLobby.OnUserReadyChanged += OnPlayersReadyChanged;
        
        
        _localPlayer = GameNetworkManager.Instance.LocalPlayer;
        _startGameButton.onClick.AddListener(GoToGameButton);
        _readyButton.onClick.AddListener(HandleReady);
        _quitLobbyButton.onClick.AddListener(LeaveLobby);
        
        
    }

    private void InitLobby()
    {
        _container.SetActive(true);
        
        if (_localPlayer.IsHost.Value)
        {
            _startGameButton.gameObject.SetActive(true);
        }
        else
        {
            _startGameButton.gameObject.SetActive(false);
        }
        
    }
    private void UpdatePlayers(LocalPlayer localPlayer = null)
    {
        _localLobby = GameNetworkManager.Instance.LocalLobby;
        int playerCount = _localLobby.PlayerCount;
        int maxPlayerCount = _localLobby.MaxPlayerCount.Value;
        _playerCount.text = playerCount + "/" + maxPlayerCount;
        
        bool isPrivate = _localLobby.Private.Value;
        UpdateLobbyCode(isPrivate);
        HandlePlayerBanners();
    }
    
    private void OnPlayersReadyChanged(int index)
    {
        HandlePlayerBanners(index);
    }
    private void OnUserLeft(int index)
    {
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


    private void HandlePlayerBanners(int index = 0)
    {
        DestroyBanners();

        foreach (var player in _localLobby.LocalPlayers)
        {
            GameObject playerUI = Instantiate(_playerPrefab, _playerContainer.transform);
            PlayerLobbyUI playerLobbyUI = playerUI.GetComponent<PlayerLobbyUI>();
            playerLobbyUI.SetName(player.DisplayName.Value);
            playerLobbyUI.SetReady(player.UserStatus.Value == PlayerStatus.Ready);
            // Debug.Log("PLayer Status ".SetColor("#20D0F7") + player.UserStatus.Value);
        }
    }

    private void DestroyBanners()
    {
        foreach (Transform child in _playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void GoToGameButton()
    {
        
        GameNetworkManager.Instance.GoToGame();
    }
    private void HandleReady()
    {
        _localPlayer.UserStatus.Value = PlayerStatus.Ready;
    }
    
    private void LeaveLobby()
    {
        DestroyBanners();
        GameNetworkManager.Instance.SetMenuState();
        _container.SetActive(false);
        _onReturnMenu.Raise();
        
    }
   
}
