using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
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

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                CreateLobby("TestLobby", false);
            }
            
            if(Input.GetKeyDown(KeyCode.J))
            {
                QuickJoin();
            }
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
            
        }
        private async void SendLocalUserData()
        {
            await _lobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
        }

        private async Task CreateLobby()
        {
            _localUser.IsHost.Value = true;
            _localLobby.OnUserReadyChanged += OnPlayersReady;
            try
            {
                await BindLobby();
            }
            catch (LobbyServiceException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void OnPlayersReady(int readyCount)
        {
            if (readyCount == _localLobby.PlayerCount && _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
                SendLocalLobbyData();
            }
            else if (_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                SendLocalLobbyData();
            }
        }

        private async Task BindLobby()
        {
            await _lobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
            _localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
            // SetLobbyView();
        }

        private async Task JoinLobby()
        {
            _localUser.IsHost.ForceSet(false);
            await BindLobby();
        }

        private void OnLobbyStateChanged(LobbyState state)
        {
        }

        private void SetGameState(GameState state)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region public Methods

        public void HostSetRelayCode(string code)
        {
            _localLobby.RelayCode.Value = code;
            SendLocalLobbyData();
            
        }
        
        public async void CreateLobby(string name, bool isPrivate, string password = null, int maxPlayers = 4)
        {
            try
            {
                var lobby = await _lobbyManager.CreateLobbyAsync(
                    name,
                    maxPlayers,
                    isPrivate,
                    _localUser,
                    password);
                
                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await CreateLobby();
            }
            catch (LobbyServiceException exception)
            {
                Debug.LogError(exception);
                throw;
            }
        }

        public async void JoinLobby(string lobbyID, string lobbyCode, string password = null)
        {
            try
            {
                var lobby = await _lobbyManager.JoinLobbyAsync(lobbyID, lobbyCode,_localUser, password:password);
                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await JoinLobby();
            }
            catch (LobbyServiceException serviceException)
            {
                SetGameState(GameState.JoinMenu);
                Debug.LogError(serviceException);
                throw;
            }
        }

        public async void QuickJoin()
        {
            var lobby = await _lobbyManager.QuickJoinLobbyAsync(_localUser);
            if(lobby != null)
            {
                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await JoinLobby();
            }
            else
            {
                SetGameState(GameState.JoinMenu);
            }
        }

      

        #endregion
    }
}
public enum GameState
{
    Menu = 1,
    Lobby = 2,
    JoinMenu = 4,
}