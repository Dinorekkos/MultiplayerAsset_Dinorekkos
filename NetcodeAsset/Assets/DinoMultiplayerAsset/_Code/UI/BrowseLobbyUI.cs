using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using UnityEngine;

public class BrowseLobbyUI : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _onBrowseLobbies;
    
    [Header("UI")]
    [SerializeField] private GameObject _parent;
    [SerializeField] private GameObject _lobbiesContainer;
    [SerializeField] private GameObject _lobbyPrefab;

    private List<LobbyInQueryUI> _lobbies = new List<LobbyInQueryUI>();
    private LocalLobbyList _localLobbyList;
    void Start()
    {
        _parent.SetActive(false);
        _onBrowseLobbies.OnEventRaised += OnBrowseLobbies;
    }

    private void OnBrowseLobbies()
    {
        GameNetworkManager.Instance.BindLobbiesInQuery();
        _localLobbyList = GameNetworkManager.Instance.LobbyList;
        GetAvailableLobbies();
    }
    
    private void GetAvailableLobbies()
    {
        _parent.SetActive(true);
        foreach (var lobby in _lobbies)
        {
            Destroy(lobby.gameObject);
        }
        _lobbies.Clear();
        
        foreach (var lobby in _localLobbyList.CurrentLobbies)
        {
            LocalLobby localLobby = lobby.Value;
            var lobbyObj = Instantiate(_lobbyPrefab, _lobbiesContainer.transform);
            var lobbyInQuery = lobbyObj.GetComponent<LobbyInQueryUI>();
            lobbyInQuery.InitLobby(localLobby);
            _lobbies.Add(lobbyInQuery);
        }
    }
    
    
}
