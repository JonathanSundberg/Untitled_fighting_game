using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class playercontroller : MonoBehaviour
{

    PlayerMovement movement;
    private PlayerInput _input;
    // Start is called before the first frame update
    void Start()
    {
        movement = gameObject.GetComponent<PlayerMovement>();
        _input = GetComponent<PlayerInput>();
        _input.actions["Up"].performed += OnUp;
        _input.actions["Up"].canceled += VerticalCancel;
        _input.actions["Down"].performed += OnDown;
        _input.actions["Down"].canceled += VerticalCancel;
        _input.actions["Left"].performed += OnLeft;
        _input.actions["Left"].canceled += HorizontalCancel;
        _input.actions["Right"].performed += OnRight;
        _input.actions["Right"].canceled += HorizontalCancel;

    }

    private void OnUp(InputAction.CallbackContext value)
    {
        Debug.Log("Up!");
        var input = value.ReadValue<float>();
        Debug.Log(input);
        movement.movement_dir.y = input;
    }
    private void VerticalCancel(InputAction.CallbackContext value)
    {
        Debug.Log("Vertical canceled");
        movement.movement_dir.y = 0;
    }
    private void HorizontalCancel(InputAction.CallbackContext value)
    {
        Debug.Log("Horizontal canceled");
        movement.movement_dir.x = 0;
    }

    private void OnDown(InputAction.CallbackContext value)
    {
        Debug.Log("Down!");
        var input = value.ReadValue<float>();
        Debug.Log(input);
        movement.movement_dir.y = input *-1.0f;
    }

    private void OnLeft(InputAction.CallbackContext value)
    {
        Debug.Log("Left!");
        var input = value.ReadValue<float>();
        Debug.Log(input);
        movement.movement_dir.x = input * -1.0f;
    }

    private void OnRight(InputAction.CallbackContext value)
    {
        Debug.Log("Right!");
        //var input = value.ReadValue<float>();
        //Debug.Log(input);
        //movement.movement_dir.x = input ;
    }


}
