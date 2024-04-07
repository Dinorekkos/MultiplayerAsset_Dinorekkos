using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    /// <summary>
    ///  GameNetworkManager is the main class that manages the game network.
    ///  It is responsible for initializing the local player, local lobby, and lobby manager.
    /// </summary>
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
        private RelayManager _relayManager;
        
        

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
            _relayManager = new RelayManager();
            
            await InitializeServices();
            AuthenticationPlayer();
            
        }

        private async Task InitializeServices()
        {
            string serviceProfileName = "Player";
            
            //If you are using multiple unity editors with ParrelSync, make sure to initialize the local profile only once before using your Unity Services.
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{LocalProfileTool.LocalProfileSuffix}";
#endif

            await UnityServicesAuthenticator.TrySignInAsync(serviceProfileName);
        }

        private void AuthenticationPlayer()
        {
            var localId = AuthenticationService.Instance.PlayerId;
            var randomName = NameGenerator.GetRandomName(localId);
            
            _localUser.ID.Value = localId;
            _localUser.DisplayName.Value = randomName;
            
            Debug.Log($"Player ID: {_localUser.ID.Value} - Player Name: {_localUser.DisplayName.Value}");
        }

        private async void SendLocalLobbyData()
        {
            // await _lobbyManager.
        }

        #endregion


        #region public Methods

        public void HostSetRelayCode(string code)
        {
            _localLobby.RelayCode.Value = code;
            SendLocalLobbyData();
            
        }

        #endregion
    }
}