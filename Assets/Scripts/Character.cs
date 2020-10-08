using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    CharacterController controller;
    CharacterInputActions inputActions;
    AnimationCurve curve;

    public float runAcceleration = 16;
    public float runMaxSpeed = 8;
    public float maxDownwardsSpeedFalling = 15;
    public float maxUpwardsSpeedNormal = 40;
    public float normalGravity = 20;
    public float turnSpeed = 500;
    public float runDeceleration = 3;
    public Vector2 jumpSpeed = new Vector2(0, 3.5f);


    //PHYSICS PROPERTIES
    Vector3 Velocity { get; set; }
    Vector3 Acceleration { get; set; }
    float _maxHorizontalSpeed;
    float MaxHorizontalSpeed { get { return _maxHorizontalSpeed; } set { _maxHorizontalSpeed = Mathf.Abs(value); } }
    float _maxDownwardsSpeed;
    float MaxDownwardsSpeed { get { return _maxDownwardsSpeed; } set { _maxDownwardsSpeed = -Mathf.Abs(value); } }
    float _maxUpwardsSpeed;
    float MaxUpwardsSpeed { get { return _maxUpwardsSpeed; } set { _maxUpwardsSpeed = Mathf.Abs(value); } }
    float Gravity { get; set; } = -15;//permitimos gravedad positiva, ir con cuidado
    bool Decelerate { get; set; }
    bool IsGrounded { get; set; }

    //INPUT
    Vector2 lastMovementInput = new Vector2();
    ButtonPhase jumpPress = ButtonPhase.NotPressed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new CharacterInputActions();
        SubscribeToInputActions();
    }

    void Update()
    {
        MaxHorizontalSpeed = runMaxSpeed;
        MaxDownwardsSpeed = maxDownwardsSpeedFalling;
        MaxUpwardsSpeed = maxUpwardsSpeedNormal;
        Gravity = normalGravity;

        ProcessMovementInput(lastMovementInput);
        ProcessJumpInput();

        ApplyMovementSimple();

        if (jumpPress == ButtonPhase.JustPressed || jumpPress == ButtonPhase.JustReleased)
            jumpPress++;
    }

    void ProcessMovementInput(Vector2 input)
    {
        Vector3 inputWorldDirection = Utils.InputRelativeToCamera(Camera.main.transform, input);


        if (input != Vector2.zero)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(inputWorldDirection, Vector3.up), turnSpeed * Time.deltaTime);
            if (IsGrounded)
            {
                //si estamos en el suelo reconvertimos la inercia hacia la nueva dirección
                //TODO falta quitar velocidad si el cambio de yaw es muy brusco
                Vector2 horizontalVelocity = new Vector2(Velocity.x, Velocity.z);
                Velocity = new Vector3(inputWorldDirection.x * horizontalVelocity.magnitude, Velocity.y, inputWorldDirection.z * horizontalVelocity.magnitude);
            }
            Acceleration = inputWorldDirection * runAcceleration;
            Decelerate = false;
        }
        else
        {
            Vector2 horizontalVelocityOppositeDirection = new Vector2(-Velocity.x, -Velocity.z);
            if (horizontalVelocityOppositeDirection != Vector2.zero)
            {
                horizontalVelocityOppositeDirection.Normalize();//no queremos normalizar el vector (0, 0)
                Acceleration = new Vector3(horizontalVelocityOppositeDirection.x, 0, horizontalVelocityOppositeDirection.y);
                Acceleration *= Mathf.Abs(runDeceleration);
                Decelerate = true;
            }
            else
                Acceleration = Vector3.zero;
        }

    }
    void ProcessJumpInput()
    {
        if (IsGrounded)
        {
            if (jumpPress == ButtonPhase.JustPressed)
            {
                //TODO Si la velocidad que llevamos es menor a la velocidad horizontal de salto, ponérsela en x/z
                Velocity = new Vector3(Velocity.x, jumpSpeed.y, Velocity.z);
            }
        }
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

        inputActions.PlayerActionMap.Jump.performed += OnJumpInputChanged;
        inputActions.PlayerActionMap.Jump.canceled += OnJumpInputChanged;

    }

    void OnMovementChanged(InputAction.CallbackContext context)
    {
        lastMovementInput = context.ReadValue<Vector2>();
    }

    void OnJumpInputChanged(InputAction.CallbackContext context)
    {
        jumpPress = context.phase == InputActionPhase.Canceled ? ButtonPhase.JustReleased : ButtonPhase.JustPressed;
    }

    void ApplyMovementSimple()
    {
        Acceleration = new Vector3(Acceleration.x, 0, Acceleration.z);//el parámetro "y" se lo va a petar de todos modos

        //apply accelerations
        Vector3 newVelocity = Velocity + (Acceleration * Time.deltaTime);//la que llevamos más la ganada en el frame


        Vector2 horizontalVelocity = new Vector2(newVelocity.x, newVelocity.z);
        if (horizontalVelocity.magnitude > MaxHorizontalSpeed)
        {
            horizontalVelocity.Normalize();
            horizontalVelocity *= MaxHorizontalSpeed;
            newVelocity.x = horizontalVelocity.x;
            newVelocity.z = horizontalVelocity.y;
        }


        //paramos del todo si cambiamos de signo por la desaceleración
        if (Decelerate &&
            (newVelocity.x * Velocity.x < 0 || newVelocity.z * Velocity.z < 0))//negativo por negativo = positivo, solo negativo por positivo da negativo
        {
            newVelocity.x = 0;
            newVelocity.z = 0;
            Acceleration = new Vector3(0, Acceleration.y, 0);
            Decelerate = false;
        }

        Vector3 oldPosition = transform.position;

        IsGrounded = controller.SimpleMove(newVelocity);

        //Velocity = (transform.position - oldPosition) / Time.deltaTime;

        Velocity = newVelocity;

        if (Velocity.x == 0 && Velocity.z == 0)
        {
            //mirar si hay que poner desaceleración a 0 también
            Decelerate = false;
        }
    }
}
