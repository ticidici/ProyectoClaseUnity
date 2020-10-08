using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    CharacterController controller;
    CharacterInputActions inputActions;
    AnimationCurve curve;

    public float runAcceleration = 6;


    //PHYSICS PROPERTIES
    Vector3 Velocity { get; set; }
    Vector3 Acceleration { get; set; }
    float _maxHorizontalSpeed;
    float MaxHorizontalSpeed { get { return _maxHorizontalSpeed; } set { _maxHorizontalSpeed = Mathf.Abs(value); } }

    //INPUT
    Vector2 lastMovementInput = new Vector2();

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new CharacterInputActions();
        SubscribeToInputActions();
    }

    void Update()
    {
        //Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Vector3 movement = new Vector3(lastMovementInput.x, 0, lastMovementInput.y);
        Vector3 movement = Utils.InputRelativeToCamera(Camera.main.transform, lastMovementInput);
        movement.Normalize();

        if(movement != Vector3.zero)
        {
            Acceleration = movement * runAcceleration;
        }
        else
        {
            Acceleration = Vector3.zero;
            Velocity = Vector3.zero;
        }
        ApplyMovementSimple();
    }

    private void OnEnable()
    {
        inputActions.PlayerActionMap.Enable();
    }

    private void OnDisable()
    {
        inputActions.PlayerActionMap.Disable();
    }

    void SubscribeToInputActions()
    {
        inputActions.PlayerActionMap.Movement.performed += OnMovementChanged;
        inputActions.PlayerActionMap.Movement.canceled += OnMovementChanged;

        //inputActions.PlayerActionMap.Jump.performed += OnJumpInputChanged;
        //inputActions.PlayerActionMap.Jump.canceled += OnJumpInputChanged;

    }

    void OnMovementChanged(InputAction.CallbackContext context)
    {
        lastMovementInput = context.ReadValue<Vector2>();
    }

    void ApplyMovementSimple()
    {
        Acceleration = new Vector3(Acceleration.x, 0, Acceleration.z);//el parámetro "y" se lo va a petar de todos modos

        //apply accelerations
        Vector3 newVelocity = Velocity + (Acceleration * Time.deltaTime);//la que llevamos más la ganada en el frame


        //Vector2 horizontalVelocity = new Vector2(newVelocity.x, newVelocity.z);
        //if (horizontalVelocity.magnitude > MaxHorizontalSpeed)
        //{
        //    horizontalVelocity.Normalize();
        //    horizontalVelocity *= MaxHorizontalSpeed;
        //    newVelocity.x = horizontalVelocity.x;
        //    newVelocity.z = horizontalVelocity.y;
        //}


        controller.SimpleMove(newVelocity);

        Velocity = newVelocity;
    }
}
