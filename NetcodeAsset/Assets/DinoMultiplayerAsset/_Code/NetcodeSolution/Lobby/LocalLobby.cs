using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    public class LocalLobby
    {
        #region public properties

        public Action<LocalPlayer> OnUserJoined;
        public Action<int> OnUserLeft;
        public Action<int> OnUserReadyChanged;
        public Action OnPlayerKicked;
        // public Action<string> OnHostIDChanged;
        
        public CallbackValue<string> LobbyID = new CallbackValue<string>(string.Empty);
        public CallbackValue<string> LobbyCode = new CallbackValue<string>(string.Empty);
        public CallbackValue<string> RelayCode = new CallbackValue<string>(string.Empty);
        public CallbackValue<ServerAddress> RelayServer = new CallbackValue<ServerAddress>();
        public CallbackValue<string> LobbyName = new CallbackValue<string>(string.Empty);
        public CallbackValue<string> HostID = new CallbackValue<string>(string.Empty);
        public CallbackValue<LobbyState> LocalLobbyState = new CallbackValue<LobbyState>();
        public CallbackValue<int> MaxPlayerCount = new CallbackValue<int>();
        public CallbackValue<long> LastUpdated = new CallbackValue<long>();
        public CallbackValue<bool> Private = new CallbackValue<bool>();
        public CallbackValue<int> AvailableSlots = new CallbackValue<int>();
        public CallbackValue<bool> Locked = new CallbackValue<bool>();

        
        public int PlayerCount => _localPlayers.Count;
        public List<LocalPlayer> LocalPlayers => _localPlayers;
        #endregion

        #region private properties

        List<LocalPlayer> _localPlayers = new List<LocalPlayer>();
        
        #endregion

        #region public methods

        public void ResetLobby()
        {
            _localPlayers.Clear();
            LobbyName.Value = "";
            LobbyID.Value = "";
            LobbyCode.Value = "";
            MaxPlayerCount.Value = GameNetworkManager.Instance.NetworkSettings.MaxPlayerCount;
            
            OnUserJoined = null;
            OnUserLeft = null;
            
        }

        public void OnKickedFromLobby()
        {
            OnPlayerKicked?.Invoke();
        }
        #endregion

        #region public constructors

        public LocalLobby()
        {
            LastUpdated.Value = DateTime.Now.ToFileTimeUtc();
            HostID.onChanged += OnHostChanged;
        }

        ~LocalLobby()
        {
            HostID.onChanged -= OnHostChanged;
        }

        #endregion

        
        #region public methods

        public LocalPlayer GetLocalPlayer(int index)
        {
            return PlayerCount > index ? _localPlayers[index] : null;
        }

        public void AddPlayer(int index, LocalPlayer user)
        {
            _localPlayers.Insert(index, user);
            user.UserStatus.onChanged += OnUserChangedStatus;
            OnUserJoined?.Invoke(user);
            Debug.Log($"Added User: {user.DisplayName.Value} - {user.ID.Value} to slot {index + 1}/{PlayerCount}");

        }

        public void RemovePlayer(int playerIndex)
        {
            Debug.Log($"Removing User: {_localPlayers[playerIndex].DisplayName.Value} - {_localPlayers[playerIndex].ID.Value} from slot {playerIndex + 1}/{PlayerCount}");
            _localPlayers[playerIndex].UserStatus.onChanged -= OnUserChangedStatus;
            _localPlayers.RemoveAt(playerIndex);
            OnUserLeft?.Invoke(playerIndex);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Lobby : ");
            sb.AppendLine(LobbyName.Value);
            sb.Append("ID: ");
            sb.AppendLine(LobbyID.Value);
            sb.Append("Code: ");
            sb.AppendLine(LobbyCode.Value);
            sb.Append("Lobby LocalLobbyState Last Edit: ");
            sb.AppendLine(new DateTime(LastUpdated.Value).ToString());
            sb.Append("RelayCode: ");
            sb.AppendLine(RelayCode.Value);
            sb.Append("Max Player Count: ");
            sb.AppendLine(MaxPlayerCount.Value.ToString());
            
            return base.ToString();
        }

        #endregion
        
        #region private methods

        private void OnHostChanged(string newHostID)
        {
            foreach (var player in _localPlayers)
            {
                player.IsHost.Value = player.ID.Value == newHostID;
                // OnHostIDChanged?.Invoke(newHostID);
            }
        }
        
        private void OnUserChangedStatus(PlayerStatus status)
        {
            int readyCount = 0;
            
            foreach (var player in _localPlayers)
            {
                if (player.UserStatus.Value == PlayerStatus.Ready)
                {
                    readyCount++;
                }
            }
            OnUserReadyChanged?.Invoke(readyCount);
        }
        

        #endregion
        
    }
}