using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowseLobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject _lobbiesContainer;
    [SerializeField] private GameObject _lobbyPrefab;
    
    private List<LobbyInQuery> _lobbies = new List<LobbyInQuery>();
    void Start()
    {
        
    }

    
}
