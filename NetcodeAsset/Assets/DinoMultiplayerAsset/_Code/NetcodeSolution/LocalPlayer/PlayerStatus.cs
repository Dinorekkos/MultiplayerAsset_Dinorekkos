namespace Dino.MultiplayerAsset
{
    public enum PlayerStatus
    {
        None = 0,
        Connecting = 1, // User has joined a lobby but has not yet connected to Relay.
        Lobby = 2, // User is in a lobby and connected to Relay.
        Ready = 4, // User has selected the ready button, to ready for the "game" to start.
        InGame = 8, // User is part of a "game" that has started.
        Menu = 16 // User is not in a lobby, in one of the main menus.
    }

    public enum RequestType
    {
        Query = 0,
        Join,
        QuickJoin,
        Host
    }
    
    public enum LobbyColor
    {
        None = 0,
        Orange = 1,
        Green = 2,
        Blue = 3
    }
    public enum LobbyState
    {
        Lobby = 1,
        CountDown = 2,
        InGame = 4
    }
}