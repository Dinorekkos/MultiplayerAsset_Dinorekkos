using System.Collections;
using System.Collections.Generic;
using Dino;
using Dino.MultiplayerAsset;
using UnityEngine;
using UnityEngine.Serialization;


public class Player : NetcodeObejct
{
    [Header("Components")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Rigidbody rigidbody;
    
    
    [Header("Movement")]
    [SerializeField] private float speed;


    protected override void MyStart()
    {
        Debug.Log("MyStart() called!".SetColor("#34F8DA"));
    }

    protected override void MyUpdate()
    {
        Vector2 movement = InputManager.MovementVector2;
        rigidbody.velocity = new Vector3(movement.x, 0, movement.y) * speed;
    }
}
