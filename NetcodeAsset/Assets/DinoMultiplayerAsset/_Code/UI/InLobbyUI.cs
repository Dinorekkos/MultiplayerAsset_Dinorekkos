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
        SubscribeLobbyEvents();
        _localPlayer = GameNetworkManager.Instance.LocalPlayer;
        _startGameButton.onClick.AddListener(GoToGameButton);
        _readyButton.onClick.AddListener(HandleReady);
        _quitLobbyButton.onClick.AddListener(LeaveLobby);
    }

    private void SubscribeLobbyEvents()
    {
        GameNetworkManager.Instance.LocalLobby.LobbyName.onChanged += UpdateLobbyName;
        GameNetworkManager.Instance.LocalLobby.OnUserJoined += UpdatePlayers;
        GameNetworkManager.Instance.LocalLobby.OnUserLeft += OnUserLeft;
        GameNetworkManager.Instance.LocalLobby.HostID.onChanged += CheckHost;
    }
    
    private void UnsubscribedEvents()
    {
        GameNetworkManager.Instance.LocalLobby.LobbyName.onChanged -= UpdateLobbyName;
        GameNetworkManager.Instance.LocalLobby.OnUserJoined -= UpdatePlayers;
        GameNetworkManager.Instance.LocalLobby.OnUserLeft -= OnUserLeft;
        GameNetworkManager.Instance.LocalLobby.HostID.onChanged -= CheckHost;
    }
    


    private void InitLobby()
    {
        SubscribeLobbyEvents();
        UpdateLobby();
        _container.SetActive(true);
        _readyButton.interactable = true;
    }

    private void UpdateLobby()
    {
        if(_localLobby == null) return;
        
        CheckHost(_localLobby.HostID.Value);
        UpdatePlayers();
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
        CheckHost();
    }

    private void CheckHost(string hostID = "")
    {
        if(hostID == _localPlayer.ID.Value)
        {
            _startGameButton.gameObject.SetActive(true);
            _readyButton.gameObject.SetActive(false);
        }
        else
            _startGameButton.gameObject.SetActive(false);  
        
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
            playerLobbyUI.InitPlayer(player);
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
        int playerIndex = _localLobby.GetPlayerIndex(_localPlayer.ID.Value);
        bool isReady = _localLobby.LocalPlayers[playerIndex].UserStatus.Value == PlayerStatus.Ready;
        
        if (isReady)
        {
            _localLobby.ChangePlayerStatus(playerIndex, PlayerStatus.Lobby);
            return;
        }
        
        _localLobby.ChangePlayerStatus(playerIndex, PlayerStatus.Ready);
    }
    
    private void LeaveLobby()
    {
        DestroyBanners();
        GameNetworkManager.Instance.SetMenuState();
        _container.SetActive(false);
        UnsubscribedEvents();
        _onReturnMenu.Raise();
        
    }
    
   
   
}
