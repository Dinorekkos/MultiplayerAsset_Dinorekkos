using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
        
        #endregion

        #region SerializedFields
        
        [Header("Network Settings")]
        [SerializeField] private NetworkSettingsSO _networkSettings;
        [SerializeField] private NetworkPrefabsList _networkPrefabsList;

        #endregion


        #region private properties
        
        private LocalPlayer _localUser;
        private LocalLobby _localLobby;
        private LobbyManager _lobbyManager;
        private RelayManager _relayManager;
        private LocalLobbyList _lobbyList;
        private NetworkManager _networkManager;
        #endregion

        #region public properties
        
        public Action<GameState> OnGameStateChanged;
        public GameState LocalGameState { get; private set; }
        public NetworkSettingsSO NetworkSettings => _networkSettings;
        public LocalLobby LocalLobby => _localLobby;
        public LocalLobbyList LobbyList => _lobbyList;
        public NetworkManager NetworkManager => _networkManager;
        public NetworkPrefabsList NetworkPrefabsList => _networkPrefabsList;
        public LocalLobbyList LocalLobbyList => _lobbyList;
        public LocalPlayer LocalPlayer => _localUser;
        
        public event Action<ulong> OnClientConnected;
        public event Action<ulong> OnClientDisconnected;
        
        public event Action OnLobbyListChanged; 
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
            DontDestroyOnLoad(this);

            //Initialize the local player, local lobby, lobby manager, and relay manager.
            _localUser = new LocalPlayer("", 0,false, "LocalPlayer");
            _localLobby = new LocalLobby {LocalLobbyState = {Value = LobbyState.Lobby}};
            _lobbyList = new LocalLobbyList();
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
            await _lobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
        }
        private async void SendLocalUserData()
        {
            await _lobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
        }

        private async Task CreateLobby()
        {
            _localUser.IsHost.Value = true;
            _localLobby.OnUserReadyChanged = OnPlayersReady;
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

        //Only Host needs to listen to this and change state.
        private void OnPlayersReady(int readyCount)
        {
            if (readyCount == _localLobby.PlayerCount && _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
            }
            else if (_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
            }
            
            SendLocalLobbyData();
        }

        private async Task BindLobby()
        {
            await _lobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
            _localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
            
            SetLobbyView();
        }

        private void SetLobbyView()
        {
            SetGameState(GameState.Lobby);
            SetLocalUserStatus(PlayerStatus.Lobby);
        }

        private void SetLocalUserStatus(PlayerStatus playerStatus)
        {
            _localUser.UserStatus.Value = playerStatus;
            SendLocalUserData();
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
            var isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) && LocalGameState == GameState.Lobby;
            LocalGameState = state;
            OnGameStateChanged?.Invoke(state);
            Debug.Log("State Game Changed: ".SetColor("#93FFE8") + state);

            if (isLeavingLobby)
            {
                Debug.Log("Leaving Lobby");
                LeaveLobby();
            }
        }

        private void ResetLocalLobby()
        {
            _localLobby.ResetLobby();
            _localLobby.RelayServer = null;
        }

        async Task CreateNetworkManager()
        {
            _networkManager = NetworkManager.Singleton;
            InitializeNetworkEvents();
            
            if (_localUser.IsHost.Value)
            {
                await _relayManager.SetRelayHostData(_localLobby);
                _networkManager.StartHost();

            }
            else
            {
                await _relayManager.AwaitRelayCode(_localLobby);
                await _relayManager.SetRelayClientData(_localLobby);
                _networkManager.StartClient();
            }
        }

        private void InitializeNetworkEvents()
        {
            _networkManager.OnClientConnectedCallback += (clientID) =>
            {
                OnClientConnected?.Invoke(clientID);
            };
            _networkManager.OnClientDisconnectCallback += (clientID) =>
            {
                OnClientDisconnected?.Invoke(clientID);
            };
        }
        
        void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            foreach (var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID.Value, lobby);

            LobbyList.CurrentLobbies = newLobbyDict;
            LobbyList.QueryState.Value = LobbyQueryState.Fetched;
        }
        #endregion


        #region public Methods

        public void SetMenuState()
        {
            SetGameState(GameState.Menu);
            SetLocalUserStatus(PlayerStatus.Menu);

        }
        public void StartNetworkedGame()
        {
#pragma warning disable 4014
            CreateNetworkManager();
#pragma warning restore 4014

        }
        public void LeaveLobby()
        {
            _localUser.ResetState();
#pragma warning disable 4014
            _lobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
            ResetLocalLobby();
            _lobbyList.Clear();
        }
        
        public void HostSetRelayCode(string code)
        {
            _localLobby.RelayCode.Value = code;
            SendLocalLobbyData();
        }
        
        public async void CreateLobby(string name, bool isPrivate, int maxPlayers = 4 , string password = null)
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
                Debug.Log("Find Quick joined lobby");
                LobbyConverters.RemoteToLocal(lobby, _localLobby);
                await JoinLobby();
            }
            else
            {
                Debug.Log("No lobby found");
                SetGameState(GameState.Menu);
            }
        }
        
        public async void BrowseLobbies()
        {
            SetGameState(GameState.JoinMenu);
            QueryResponse queryResponse = await _lobbyManager.GetQueryLobbies();
            var lobbies = queryResponse.Results;
            _lobbyList.Clear();
            foreach (var lobby in lobbies)
            {
                var localLobby = new LocalLobby();
                LobbyConverters.RemoteToLocal(lobby, localLobby);
                _lobbyList.CurrentLobbies.Add(lobby.Id, localLobby);
                Debug.Log("Lobby: " + localLobby.LobbyName.Value);
            }
            
            OnLobbyListChanged?.Invoke();
        }

        public async void QueryLobbies()
        {
            LobbyList.QueryState.Value = LobbyQueryState.Fetching;
            var queryResponse = await _lobbyManager.RetrieveLobbyListAsync();
            
            if (queryResponse == null)
            {
                LobbyList.QueryState.Value = LobbyQueryState.Error;
                return;
            }
            
            SetCurrentLobbies(LobbyConverters.QueryToLocalList(queryResponse));
            OnLobbyListChanged?.Invoke();
            
        }
        
        public void LoadScene(string sceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

      

        #endregion

        public void GoToGame()
        {   
            Debug.Log("Go to Game");
            StartNetworkedGame();
            LoadScene("GameScene");
            
            
        }
        
        
    }
}
public enum GameState
{
    Menu = 1,
    Lobby = 2,
    JoinMenu = 4,
}