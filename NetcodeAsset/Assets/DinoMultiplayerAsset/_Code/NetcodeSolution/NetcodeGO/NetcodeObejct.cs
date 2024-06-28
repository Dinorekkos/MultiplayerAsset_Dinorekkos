using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetcodeObejct : NetworkBehaviour
    {
        protected NetworkTransform _networkTransform;
        protected NetworkRigidbody _networkRigidbody;
        protected NetworkAnimator _networkAnimator;

        public NetworkRigidbody NetworkRigidbody
        {
            get
            {
                if (_networkRigidbody == null)
                {
                    InitializeNetworkRigidbody();
                }
                return _networkRigidbody;
            }
        }
        public NetworkAnimator NetworkAnimator
        {
            get
            {
                if (_networkAnimator == null)
                {
                    InitializeNetworkAnimator();
                }
                return _networkAnimator;
            }
        }
        public NetworkTransform NetworkTransform
        {
            get
            {
                if (_networkTransform == null)
                {
                    InitializeNetworkTransform();
                }
                return _networkTransform;
            }
        }

        protected void Start()
        {
            if (!IsOwner)
                return;
           
            MyStart();
        }

        protected void Update()
        {
            // if (!IsOwner)
            //     return;
            
            MyUpdate();
        }



        protected virtual void MyUpdate()
        {
        }

        protected virtual void MyStart()
        {
        }

        private void InitializeNetworkRigidbody()
        {
            _networkRigidbody = GetComponent<NetworkRigidbody>();
            if (_networkRigidbody == null)
            {
                _networkRigidbody = gameObject.AddComponent<NetworkRigidbody>();
            }
            
        }
        private void InitializeNetworkAnimator()
        {
            _networkAnimator = GetComponent<NetworkAnimator>();
            if (_networkAnimator == null)
            {
                _networkAnimator = gameObject.AddComponent<NetworkAnimator>();
                Animator animator = GetComponent<Animator>();
                if (animator != null)
                {
                    _networkAnimator.Animator = animator;
                }
            }
        }
        private void InitializeNetworkTransform()
        {
            _networkTransform = GetComponent<NetworkTransform>();
            if (_networkTransform == null)
            {
                _networkTransform = gameObject.AddComponent<NetworkTransform>();
            }
        }

        
        #region Editor Methods
        
        public void InitializeButton()
        {
            MyStart();
        }
        
        public void AddRigidbody()
        {
            InitializeNetworkRigidbody();
        }
        public void AddAnimator()
        {
            InitializeNetworkAnimator();
        }

        public void AddTransform()
        {
            InitializeNetworkTransform();
        }
        #endregion
    }
}