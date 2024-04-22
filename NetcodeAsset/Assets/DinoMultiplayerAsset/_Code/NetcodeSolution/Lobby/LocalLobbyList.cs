using System;
using System.Collections;
using System.Collections.Generic;

namespace Dino.MultiplayerAsset
{
    [Serializable]
    public class LocalLobbyList
    {
        public CallbackValue<LobbyQueryState> QueryState = new CallbackValue<LobbyQueryState>();
        public Action<Dictionary<string,LocalLobby>> OnLobbyListChange;
        
        private Dictionary<string,LocalLobby> _currentLobbies = new Dictionary<string, LocalLobby>();

        public Dictionary<string, LocalLobby> CurrentLobbies
        {
            get => _currentLobbies;
            set
            {
                _currentLobbies = value;
                OnLobbyListChange?.Invoke(_currentLobbies);
            }
        }
        
        public void Clear()
        {
            CurrentLobbies = new Dictionary<string, LocalLobby>();
            QueryState.Value = LobbyQueryState.Fetched;
        }
    }
}

public enum LobbyQueryState
{
    Empty,
    Fetching,
    Error,
    Fetched
}