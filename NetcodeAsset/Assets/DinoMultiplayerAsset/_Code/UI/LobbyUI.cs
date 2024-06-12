using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Lobby UI")] 
    [SerializeField] private GameObject _initalButttons;
    [SerializeField] private Button quickJoinButton;
    
    [Header("Create Lobby UI")]
    [SerializeField] private Button _GoCreateLobbyButton;
    [SerializeField] private Button _CreateLobbyButton;
    [SerializeField] private GameObject _CreateLobbyUI;
    [SerializeField] private TMP_InputField _LobbyNameInput;
    [SerializeField] private Slider _MaxPlayerSlider;
    [SerializeField] private TextMeshProUGUI _MaxPlayerText;
    [SerializeField] private Toggle _publicToggle;
    [SerializeField] private TextMeshProUGUI _PublicText;
    
    [Header("Browse Lobby UI")]
    [SerializeField] private Button _GoBrowseLobbyButton;
    [SerializeField] private GameObject _BrowseLobbyUI;
    [SerializeField] private Button _RefreshLobbiesButton;
    [SerializeField] private GameObject _LobbyList;
    
    [Header("InLobbyUI")]
    [SerializeField] private GameObject _InLobbyUI;
    
    private bool _isPublic = false;
    private int _maxPlayersCount;
    private string _lobbyName;
    void Start()
    {
        _GoCreateLobbyButton.onClick.AddListener(GoToCreateLobby);
        _GoBrowseLobbyButton.onClick.AddListener(BrowseLobbies);
        _MaxPlayerSlider.onValueChanged.AddListener(delegate
        {
            UpdateMaxPlayersCount();
        });
        
        _publicToggle.onValueChanged.AddListener(delegate
        {
            UpdatePublic();
        });
        
        quickJoinButton.onClick.AddListener(QuickJoin);
        _CreateLobbyButton.onClick.AddListener(CreateLobby);
        
        _maxPlayersCount = GameNetworkManager.Instance.NetworkSettings.MaxPlayerCount;
        _MaxPlayerSlider.value = _maxPlayersCount;
        UpdateMaxPlayersCount();
        
        UpdatePublic();
        
        EnableUIGameObjects(false, _InLobbyUI);
        EnableUIGameObjects(false, _CreateLobbyUI);
        
    }

    private void CreateLobby()
    {
        _lobbyName = _LobbyNameInput.text;
        
        Debug.Log("Creating lobby with name: " + _lobbyName + " max players: " + _maxPlayersCount + " private: " + !_isPublic);
        GameNetworkManager.Instance.CreateLobby(_lobbyName, !_isPublic, (int)_MaxPlayerSlider.value);
        EnableUIGameObjects(false, _CreateLobbyUI);
        EnableUIGameObjects(true, _InLobbyUI);
    }

    private void QuickJoin()
    {
        GameNetworkManager.Instance.QuickJoin();
        EnableUIGameObjects(false, _initalButttons);
        EnableUIGameObjects(true, _InLobbyUI);
    }
    
    private void ReturnToInitial()
    {
        EnableUIGameObjects(true, _initalButttons);
        EnableUIGameObjects(false, _CreateLobbyUI);
    }
    private void GoToCreateLobby()
    {
        EnableUIGameObjects(true, _CreateLobbyUI);
        EnableUIGameObjects(false, _initalButttons);
    }

    private void BrowseLobbies()
    {
    }


    private void EnableUIGameObjects(bool enable, GameObject obj)
    {
        obj.SetActive(enable);
    }
    
    private void UpdateMaxPlayersCount()
    {
        _MaxPlayerText.text = _MaxPlayerSlider.value.ToString();
    }
    private void UpdatePublic()
    {
        _isPublic = _publicToggle.isOn;
        _PublicText.text = _isPublic ? "Public" : "Private";
    }
    
    
}