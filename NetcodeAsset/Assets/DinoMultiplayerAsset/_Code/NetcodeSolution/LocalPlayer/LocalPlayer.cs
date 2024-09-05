using System;
using System.Collections.Generic;
using Dino;
using Dino.MultiplayerAsset;
using UnityEngine;

public class LocalPlayer 
{
    public CallbackValue<bool> IsHost = new CallbackValue<bool>(false);
    public CallbackValue<string> ID = new CallbackValue<string>(string.Empty);
    public CallbackValue<PlayerStatus> UserStatus = new CallbackValue<PlayerStatus>(PlayerStatus.None);
    public CallbackValue<string> DisplayName = new CallbackValue<string>(string.Empty);
    public CallbackValue<int> Index = new CallbackValue<int>(0);
    
    public DateTime LastUpdate;
    
    public LocalPlayer(string id,int index, bool isHost, string displayName = default, PlayerStatus status = default(PlayerStatus))
    {
        ID.Value = id;
        IsHost.Value = isHost;
        UserStatus.Value = status;
        Index.Value = index;
        DisplayName.Value = displayName;
    }

    public void ResetState()
    {
        IsHost.Value = false;
        UserStatus.Value = PlayerStatus.Menu;
    }
}
