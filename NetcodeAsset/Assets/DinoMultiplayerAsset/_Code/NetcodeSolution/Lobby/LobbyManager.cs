using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Enum = System.Enum;

namespace Dino.MultiplayerAsset
{
    public class LobbyManager : IDisposable
    {
        #region public variables

        public int MaxLobbiesToShow
        {
            get => _maxLobbiesToShow;
            set => _maxLobbiesToShow = value;
        }

        public Lobby CurrentLobby => _currentLobby;

        #endregion

        #region private variables

        private Lobby _currentLobby;
        private LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();
        private Task _heartbeatTask;

        private int _maxLobbiesToShow = 10;

        private const string KEY_RELAYCODE = nameof(LocalLobby.RelayCode);
        private const string KEY_DISPLAYNAME = nameof(LocalPlayer.DisplayName);
        private const string KEY_USERSTATUS = nameof(LocalPlayer.UserStatus);

        private ServiceRateLimiter _joinCoolDown = new ServiceRateLimiter(2, 6f);
        private ServiceRateLimiter _queryCooldown = new ServiceRateLimiter(1, 1f);
        private ServiceRateLimiter _quickJoinCooldown = new ServiceRateLimiter(1, 10f);
        private ServiceRateLimiter _createCooldown = new ServiceRateLimiter(2, 6f);
        private ServiceRateLimiter _leaveLobbyOrRemovePlayer = new ServiceRateLimiter(5, 1);
        private ServiceRateLimiter _heartBeatCooldown = new ServiceRateLimiter(5, 30);
        private ServiceRateLimiter _updatePlayerCooldown = new ServiceRateLimiter(5, 5f);
        private ServiceRateLimiter _updateLobbyCooldown = new ServiceRateLimiter(5, 5f);

        private Task _heartBeatTask;

        #endregion


        #region public methods

        public bool InLobby()
        {
            if (_currentLobby == null)
            {
                Debug.LogWarning("LobbyManager not currently in a lobby. Did you CreateLobbyAsync or JoinLobbyAsync?");
                return false;
            }

            return true;
        }

        public ServiceRateLimiter GetRateLimit(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.Join:
                    return _joinCoolDown;
                case RequestType.QuickJoin:
                    return _quickJoinCooldown;
                case RequestType.Host:
                    return _createCooldown;

            }

            return _queryCooldown;
        }

        public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate,
            LocalPlayer localUser, string password)
        {
            if (_createCooldown.IsCooingDown)
            {
                Debug.LogWarning("CreateLobbyAsync is on cooldown.");
                return null;
            }

            await _createCooldown.QueueUntilCooldown();
            string uasId = AuthenticationService.Instance.PlayerId;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser)),
                Password = password
            };
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            StartHeartBeat();

            return _currentLobby;
        }

        public async Task<Lobby> JoinLobbyAsync(string lobbyId, string lobbyCode, LocalPlayer localUser,
            string password = null)
        {
            if (_joinCoolDown.IsCooingDown || (lobbyId == null && lobbyCode == null))
            {
                Debug.LogWarning("JoinLobbyAsync is on cooldown or lobbyId and lobbyCode are null.");
                return null;
            }

            await _joinCoolDown.QueueUntilCooldown();

            string uasId = AuthenticationService.Instance.PlayerId;
            var playerData = CreateInitialPlayerData(localUser);

            if (!string.IsNullOrEmpty(lobbyId))
            {
                JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player(id: uasId, data: playerData), Password = password
                };
                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
            }
            else
            {
                JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions
                {
                    Player = new(id: uasId, data: playerData), Password = password
                };
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
            }

            return _currentLobby;
        }

        public async Task<Lobby> QuickJoinLobbyAsync(LocalPlayer localUser, LobbyColor lobbyColor = LobbyColor.None)
        {
            //not queue on quickjoin 
            if (_quickJoinCooldown.IsCooingDown)
            {
                Debug.LogWarning("Quick Join Lobby hit the rate limit.");
                return null;
            }

            await _quickJoinCooldown.QueueUntilCooldown();
            var filters = LobbyColorToFilter(lobbyColor);
            string uasId = AuthenticationService.Instance.PlayerId;

            var joinRequest = new QuickJoinLobbyOptions
            {
                Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser)),
                Filter = filters
            };

            return _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);

        }

        public async Task BindLocalLobbyToRemote(string lobbyID, LocalLobby localLobby)
        {
            _lobbyEventCallbacks.LobbyDeleted += async () =>
            {
                await LeaveLobbyAsync();
            };

            _lobbyEventCallbacks.DataChanged += changes =>
            {
                foreach (var change in changes)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;
                    
                    if (changedKey == KEY_RELAYCODE)
                        localLobby.RelayCode.Value = changedValue.Value.Value;

                }
            };
            
            _lobbyEventCallbacks.DataAdded += changes =>
            {
                foreach (var change in changes)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;
                    
                    if (changedKey == KEY_RELAYCODE)
                        localLobby.RelayCode.Value = changedValue.Value.Value;

                }
            };
            
            _lobbyEventCallbacks.DataRemoved += changes =>
            {
                foreach (var change in changes)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;
                    
                    if (changedKey == KEY_RELAYCODE)
                        localLobby.RelayCode.Value = "";
                }
            };

            _lobbyEventCallbacks.PlayerLeft += players =>
            {
                foreach (var leftPlayerIndex in players)
                {
                    localLobby.RemovePlayer(leftPlayerIndex);
                }

            };

            _lobbyEventCallbacks.PlayerJoined += players =>
            {
                foreach (var playerChanges in players)
                {
                    Player joinedPlayer = playerChanges.Player;
                    var id = joinedPlayer.Id;
                    var index = playerChanges.PlayerIndex;
                    var isHost = localLobby.HostID.Value == id;

                    var newPlayer = new LocalPlayer(id, index, isHost);

                    foreach (var dataEntry in joinedPlayer.Data)
                    {
                        var dataObject = dataEntry.Value;
                        ParseCustomPlayerData(newPlayer, dataEntry.Key, dataObject.Value);
                    }

                    localLobby.AddPlayer(index, newPlayer);
                }
            };

        }

        public async Task LeaveLobbyAsync()
        {
            await _leaveLobbyOrRemovePlayer.QueueUntilCooldown();
            if(!InLobby()) return;
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
            Dispose();
        }

        public async Task UpdateLobbyDataAsync(Dictionary<string, string> data)
        {
            if(!InLobby()) return;

            Dictionary<string, DataObject> dataCurrent = _currentLobby.Data ?? new Dictionary<string, DataObject>();
            var shouldLock = false;

            foreach (var dataNew in data)
            {
                // DataObject.IndexOptions index = dataNew.Key == ""
                DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, dataNew.Value);
                
                if (dataCurrent.ContainsKey(dataNew.Key))
                    dataCurrent[dataNew.Key] = dataObject;
                else
                    dataCurrent.Add(dataNew.Key, dataObject);

                if (dataNew.Key == "LocalLobbyState")
                {
                    Enum.TryParse(dataNew.Value, out LobbyState state);
                    shouldLock = state != LobbyState.Lobby;
                }

                if (_updateLobbyCooldown.TaskQueued) return;
                
                await _updateLobbyCooldown.QueueUntilCooldown();
                UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
                {
                    Data = dataCurrent,
                    IsLocked = shouldLock
                };
                _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, updateLobbyOptions);
            }
            
        }
        public async Task UpdatePlayerDataAsync(Dictionary<string, string> data)
        {
            if(!InLobby()) return;
            
            string playerId = AuthenticationService.Instance.PlayerId;
            Dictionary<string, PlayerDataObject> dataCurrent = new Dictionary<string, PlayerDataObject>();
            
            foreach (var dataEntry in data)
            {
                PlayerDataObject dataObject = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
                    value: dataEntry.Value);
                
                if(dataCurrent.ContainsKey(dataEntry.Key))
                    dataCurrent[dataEntry.Key] = dataObject;
                else
                    dataCurrent.Add(dataEntry.Key, dataObject);
            }
            
            if(_updatePlayerCooldown.TaskQueued) return;

            await _updatePlayerCooldown.QueueUntilCooldown();
            
            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = dataCurrent,
                AllocationId = null,
                ConnectionInfo = null
                
            };
            _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, playerId, updateOptions);
            
        }

    #endregion

        #region private methods

        private Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
        {
            Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();
            var displayNameObject = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
            data.Add("DisplayName", displayNameObject);
            return data;
        }
        
        private List<QueryFilter> LobbyColorToFilter(LobbyColor limitColor)
        {
            List<QueryFilter> filters = new List<QueryFilter>();

            switch (limitColor)
            {
                case LobbyColor.Orange:
                    filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Orange).ToString(), QueryFilter.OpOptions.EQ));
                    break;
                case LobbyColor.Green:
                    filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Green).ToString(), QueryFilter.OpOptions.EQ));
                    break;
                case LobbyColor.Blue:
                    filters.Add(new QueryFilter(QueryFilter.FieldOptions.N1, ((int)LobbyColor.Blue).ToString(), QueryFilter.OpOptions.EQ));
                    break;
                    
            }

            return filters;
            
        }

        private void ParseCustomPlayerData(LocalPlayer player, string dataKey, string playerDataValue)
        {
            if (dataKey == KEY_USERSTATUS)
            {
                player.UserStatus.Value = (PlayerStatus)int.Parse(playerDataValue);
            }
            else if (dataKey == KEY_DISPLAYNAME)
            {
                player.DisplayName.Value = playerDataValue;
            }
        }

        
        
        private void StartHeartBeat()
        {
#pragma warning disable 4014
            _heartBeatTask = HeartBeatLoop();
#pragma warning restore 4014
        }
        
        private async Task HeartBeatLoop()
        {
            while (_currentLobby != null)
            {
                await SendHeartbeatPingAsync();
                await Task.Delay(8000);
            }
        }
        
        private async Task SendHeartbeatPingAsync()
        {
            if(!InLobby()) return;

            if (_heartBeatCooldown.IsCooingDown) return;

            await _heartBeatCooldown.QueueUntilCooldown();
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }
        
        #endregion

        #region IDisposable
        
        public void Dispose()
        {
            _currentLobby = null;
            _lobbyEventCallbacks = new LobbyEventCallbacks();
        }
        #endregion

    }

    public class ServiceRateLimiter
    {
        public Action<bool> OnCooldownChange;
        public readonly int coolDownMS;
        
        public bool TaskQueued { get; private set; } = false;

        private readonly int _serviceCallTimes;
        private bool _coolingDown = false;
        private int _taskCounter;

        public bool IsCooingDown
        {
            get => _coolingDown;
            private set
            {
                if (_coolingDown != value)
                {
                    _coolingDown = value;
                    OnCooldownChange?.Invoke(_coolingDown);
                }
            }
        }
        public ServiceRateLimiter(int callTimes, float coolDown, int pingBuffer = 100)
        {
            _serviceCallTimes = callTimes;
            _taskCounter = _serviceCallTimes;
            coolDownMS = Mathf.CeilToInt(coolDown * 1000) + pingBuffer;
        }

        public async Task QueueUntilCooldown()
        {
            if (_coolingDown)
            {
#pragma warning disable 4014
                ParallelCooldownAsync();
#pragma warning restore 4014
            }
            
            _taskCounter--;
            
            if (_taskCounter > 0) return;
         
            if(!TaskQueued) 
                TaskQueued = true;
            else
                return;

            while (_coolingDown)
            {
                await Task.Delay(10);
            }
        }

        private async Task ParallelCooldownAsync()
        {
            IsCooingDown = true;
            await Task.Delay(coolDownMS);
            IsCooingDown = false;
            TaskQueued = false;
            _taskCounter = _serviceCallTimes;
        }
    }
}