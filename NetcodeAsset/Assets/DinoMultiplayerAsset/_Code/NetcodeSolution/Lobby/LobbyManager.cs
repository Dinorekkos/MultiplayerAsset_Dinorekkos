using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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
        #endregion

        #region private methods

        private Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
        {
            Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();
            var displayNameObject = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
            data.Add("DisplayName", displayNameObject);
            return data;
        }

        #endregion

        #region IDisposable
        
        public void Dispose()
        {
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