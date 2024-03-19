using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    public class LobbyManager : MonoBehaviour
    {
        void Start()
        {
            InitializeAuthenticator();
        }

        void Update()
        {

        }
        
        
        private async void InitializeAuthenticator()
        {
            await UnityServicesAuthenticator.TryInitServicesAsync();
        }
    }
}