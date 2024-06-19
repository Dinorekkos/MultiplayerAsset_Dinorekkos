using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region public variables
    public static Vector2 MovementVector2;
    #endregion
    
    #region private variables

    private PlayerInput _playerInput;
    private InputAction _movementAction;

    #endregion
    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movementAction = _playerInput.actions["Movement"];
        
    }

    void Update()
    {
        MovementVector2 = _movementAction.ReadValue<Vector2>();
    }
}
