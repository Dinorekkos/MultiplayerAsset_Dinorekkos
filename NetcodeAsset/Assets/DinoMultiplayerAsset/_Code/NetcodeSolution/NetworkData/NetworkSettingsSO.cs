using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    [CreateAssetMenu(fileName = "NetworkSettingsSO", menuName = "DinoMultiplayerAsset/NetworkSettingsSO", order = 0)]
    public class NetworkSettingsSO : ScriptableObject
    {
        
        [Header("Lobby Settings")]
        [SerializeField] private int _maxPlayerCount = 4;

        [Header("Query Lobbies")] 
        [SerializeField] private int _maxLobbiesToShow = 10;
        public int MaxPlayerCount => _maxPlayerCount;
        public int MaxLobbiesToShow => _maxLobbiesToShow;
        
        
    }
}