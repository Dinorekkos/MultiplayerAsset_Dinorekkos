using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    public class GameNetworkManager : MonoBehaviour
    {

        #region instance

        

        private static GameNetworkManager _instance;
        public static GameNetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameNetworkManager>();
                }
                return _instance;
            }
        }
        
        #endregion

        #region private properties
        
        private LocalPlayer _localUser;
        private LocalLobby _localLobby;
        private LobbyManager _lobbyManager;
        

        #endregion
        
        
        #region Unity Methods

        private void Awake()
        {
            Initialize();
        }
        
        #endregion

        #region private methods

        private async void Initialize()
        {

            _localUser = new LocalPlayer("", 0,false, "LocalPlayer");
            _localLobby = new LocalLobby {LocalLobbyState = {Value = LobbyState.Lobby}};
            _lobbyManager = new LobbyManager();
            
            

        }

        private async Task InitializeServices()
        {
            string serviceProfileName = "Player";

#if UNITY_EDITOR
            // serviceProfileName = $"{serviceProfileName}{LocalProfileTool.LocalProfileSuffix}";
#endif
        }
        
        

        #endregion
        
        
        
    }
}