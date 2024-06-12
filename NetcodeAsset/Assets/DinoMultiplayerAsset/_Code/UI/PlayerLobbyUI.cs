using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _NameTxt;
    [SerializeField] private TextMeshProUGUI _isReadyTxt;
    
    public void SetName(string name)
    {
        _NameTxt.text = name;
    }
    
    public void SetReady(bool isReady)
    {
        _isReadyTxt.text = isReady ? "Ready" : "Not Ready";
    }
    

    
}
