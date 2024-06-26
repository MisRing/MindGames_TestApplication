using UnityEngine;
using Unity.Netcode;
using Cinemachine;

//Movement logic class
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{

    private GamePlayerInput playerInput;

    //Some settings
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotatinonSpeed = 20f;

    private float gravity = 9.81f;
    private float velocity;

    [SerializeField] private CinemachineVirtualCamera vCamera;

    private CharacterController characterController;


    //Initializing Input and CharacterController classes
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = new();
        playerInput.Enable();
    }

    //Setts camera priority for player
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
            vCamera.Priority = 1;
        else
            vCamera.Priority = 0;
    }

    //Some input logic
    private void Update()
    {
        Vector2 moveInput = playerInput.Player.Movement.ReadValue<Vector2>();
        Vector2 rotateInput = playerInput.Player.MouseMove.ReadValue<Vector2>();

        velocity += gravity * Time.deltaTime;

        if(IsServer && IsLocalPlayer)
        {
            Move(moveInput, velocity);
            Rotate(rotateInput.x);
            RotateCamera(rotateInput.y);

        }
        else if(IsClient && IsLocalPlayer)
        {
            MoveServerRpc(moveInput, velocity);
            RotateServerRpc(rotateInput.x);
            RotateCameraServerRpc(rotateInput.y);

        }
    }

    //Movement logic
    private void Move(Vector2 _input, float _velocity)
    {
        Vector3 direction = _input.x * transform.right + _input.y * transform.forward;
        direction = direction * moveSpeed - new Vector3(0, _velocity, 0);

        characterController.Move(direction * Time.deltaTime);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector2 _input, float _velocity)
    {
        Move(_input, _velocity);
    }

    //Rotation on Y-axys
    private void Rotate(float _input)
    {
        Vector3 rotation = Vector3.up * _input * rotatinonSpeed;

        transform.Rotate(rotation * Time.deltaTime);
    }

    [ServerRpc]
    private void RotateServerRpc(float _input)
    {
        Rotate(_input);
    }
    
    //Rotetion on X-axys (only camera rotates)
    private void RotateCamera(float _input)
    {
        float rotation = - _input * rotatinonSpeed * Time.deltaTime;
        rotation += vCamera.transform.eulerAngles.x;

        vCamera.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
    }

    [ServerRpc]
    private void RotateCameraServerRpc(float _input)
    {
        RotateCamera(_input);
    }
}
