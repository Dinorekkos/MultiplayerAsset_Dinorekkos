using System.Collections;
using System.Collections.Generic;
using Dino.MultiplayerAsset;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetcodeObejct) , true)]
public class NetcodeObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        NetcodeObejct netcodeObejct = (NetcodeObejct)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Editor Options");
        
        
        if (GUILayout.Button("Initialize Object"))
        {
            netcodeObejct.InitializeButton();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Network Transform");
        GUILayout.Box("Send GameObject Position through the clients. ", GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Initialize Network Transform"))
        {
            netcodeObejct.AddTransform();
        }

        GUILayout.Space(10);
        GUILayout.Label("Network Rigidbody");
        GUILayout.Box("Handles physics by the Netcode system for this gameObject. ", GUILayout.ExpandWidth(true));
        
        if (GUILayout.Button("Initialize Network Rigidbody"))
        {
            netcodeObejct.AddRigidbody();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Network Animator");
        GUILayout.Box("Handles gameObject animations through the clients", GUILayout.ExpandWidth(true));
        
        if (GUILayout.Button("Initialize Network Animator"))
        {
            netcodeObejct.AddAnimator();
        }
    }
    
}
