using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    public class RelayManager : IDisposable
    {
        public bool IsRelayInitialized { get; private set; }
        public async Task AwaitRelayCode(LocalLobby lobby)
        {
            string relayCode = lobby.RelayCode.Value;
            lobby.RelayCode.onChanged += (code) => relayCode = code;
            while (string.IsNullOrEmpty(relayCode))
            {
                await Task.Delay(100);
            }
        }
        public async Task SetRelayHostData(LocalLobby localLobby)
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found in NetworkManager");
                return;
            }

            var allocation = await Relay.Instance.CreateAllocationAsync(localLobby.MaxPlayerCount.Value);
            var joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameNetworkManager.Instance.HostSetRelayCode(joinCode);

            bool isSecure = false;
            
            // get a SECURE endpoint for the Relay Server, either allocation or join allocation
            var endpoint = GetEndpointForAllocation(allocation.ServerEndpoints,
                allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);
            
            transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);
            
            IsRelayInitialized = true;
        }
        
        
        public async Task SetRelayClientData(LocalLobby localLobby)
        {
            UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found in NetworkManager");
                return;
            }

            var joinAllocation = await Relay.Instance.JoinAllocationAsync(localLobby.RelayCode.Value);
            bool isSecure = false;
            
            // get a SECURE endpoint for the Relay Server, either allocation or join allocation
            var endpoint = GetEndpointForAllocation(joinAllocation.ServerEndpoints,
                joinAllocation.RelayServer.IpV4, joinAllocation.RelayServer.Port, out isSecure);
            
            transport.SetClientRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure);
            
            IsRelayInitialized = true;
        }

        /// <summary>
        /// get a endpoint for the Relay Server, either allocation or join allocation
        /// If DTLS encryption is available, and there's a secure server endpoint available, use that as a secure connection. Otherwise, just connect to the Relay IP unsecured.
        /// </summary>
        NetworkEndPoint GetEndpointForAllocation(List<RelayServerEndpoint> endpoints, string ip, int port, out bool isSecure)
        {
#if ENABLE_MANAGED_UNITYTLS
            foreach (RelayServerEndpoint endpoint in endpoints)
            {
                if (endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
                {
                    isSecure = true;
                    return NetworkEndPoint.Parse(endpoint.Host, (ushort)endpoint.Port);
                }
            }
#endif
            isSecure = false;
            return NetworkEndPoint.Parse(ip, (ushort)port);
        }
        
        string AddressFromEndpoint(NetworkEndPoint endpoint)
        {
            return endpoint.Address.Split(":")[0];
        }


        public void Dispose()
        {
            
        }
    }
}
