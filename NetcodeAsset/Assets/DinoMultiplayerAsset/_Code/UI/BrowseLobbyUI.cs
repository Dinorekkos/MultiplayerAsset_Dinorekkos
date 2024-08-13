using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dino.MultiplayerAsset;
using UnityEngine;
using UnityEngine.UI;

public class BrowseLobbyUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _onBrowseLobbies;
    [SerializeField] private GameEvent _onReturnMenu;

    
    [Header("UI")]
    [SerializeField] private GameObject _parent;
    [SerializeField] private GameObject _lobbiesContainer;
    [SerializeField] private GameObject _lobbyPrefab;
    [SerializeField] private Button _returnButton;
    
    private List<LobbyInQueryUI> _lobbiesUI = new List<LobbyInQueryUI>();
    
    Dictionary<string, LocalLobby> _currentLobbies = new Dictionary<string, LocalLobby>();
    void Start()
    {
        _parent.SetActive(false);
        _returnButton.onClick.AddListener(ReturnToInitial);
        _onBrowseLobbies.OnEventRaised += BrowseLobbies;    
        GameNetworkManager.Instance.OnLobbyListChanged += UpdateLobbies;
    }

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            BrowseLobbies();
        }
    }
    
    private void BrowseLobbies()
    {
        GameNetworkManager.Instance.BrowseLobbies();
    }
   
    private void ReturnToInitial()
    {
        GameNetworkManager.Instance.SetMenuState();
        _parent.SetActive(false);
        _onReturnMenu.Raise();
    }
    private void UpdateLobbies()
    {
        var lobbies = GameNetworkManager.Instance.LocalLobbyList.CurrentLobbies;
        _currentLobbies = lobbies;
        SetLobbiesInUI();
    }
    
    private void SetLobbiesInUI()
    {
        _parent.SetActive(true);
        foreach (var lobby in _lobbiesUI)
        {
            Destroy(lobby.gameObject);
        }
        _lobbiesUI.Clear();
        
        foreach (var lobby in _currentLobbies)
        {
            LocalLobby localLobby = lobby.Value;
            var lobbyObj = Instantiate(_lobbyPrefab, _lobbiesContainer.transform);
            var lobbyInQuery = lobbyObj.GetComponent<LobbyInQueryUI>();
            lobbyInQuery.InitLobby(localLobby);
            _lobbiesUI.Add(lobbyInQuery);
        }
    }
    
    
}
