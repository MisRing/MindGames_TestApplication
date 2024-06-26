using Cinemachine;
using Unity.Netcode;
using UnityEngine;

//Script for all player interactions (except movement)
public class PlayerInteraction : NetworkBehaviour
{
    private GamePlayerInput playerInput;

    [SerializeField] private CinemachineVirtualCamera vCamera;
    [SerializeField] private Transform cameraTransform;

    [HideInInspector] public NetworkVariable<bool> inventoryFull = new NetworkVariable<bool>(false);

    private void Awake()
    {
        playerInput = new();
        playerInput.Enable();

        //Additing controll actions to keys
        playerInput.Player.ResetPuzzle.started += context => ResetPuzzle();
        playerInput.Player.LMB.started += context => LeftMB();
        playerInput.Player.RMB.started += context => RightMB();
        playerInput.Player.TAB.started += context => CursorState();

        playerInput.Player.ESC.started += context => Application.Quit();
    }

    public override void OnNetworkSpawn()
    {
        return;
    }

    //Action for reset puzzle
    private void ResetPuzzle()
    {
        if (IsServer && IsLocalPlayer)
            SendPzUpdateRequest();
        else if (IsClient && IsLocalPlayer)
            SendPzUpdateRequestServerRpc();
    }

    //Action for hiding/showing and locking/unlocking cursor
    private void CursorState()
    {
        Cursor.visible = !Cursor.visible;
        Cursor.lockState = Cursor.lockState != CursorLockMode.Locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    //Action for left mouse button (functions of Grabing and puting cube)
    private void LeftMB()
    {
        if (IsServer && IsLocalPlayer)
            Grab(cameraTransform.position, cameraTransform.forward);
        else if (IsClient && IsLocalPlayer)
            GrabServerRpc(cameraTransform.position, cameraTransform.forward);
    }

    //Action for throwing cube
    private void RightMB()
    {
        if (IsServer && IsLocalPlayer)
            ThrowItem();
        else if (IsClient && IsLocalPlayer)
            ThrowItemServerRpc();
    }

    //Request to update puzzle
    private void SendPzUpdateRequest()
    {
        Puzzle pz = FindObjectOfType<Puzzle>();
        pz.UpdatePuzzle();
    }

    //Server Rpc request to update puzzle
    [ServerRpc]
    private void SendPzUpdateRequestServerRpc()
    {
        SendPzUpdateRequest();
    }

    //Void of throwing logic
    private void ThrowItem()
    {
        GameObject cube = transform.GetChild(transform.childCount - 1).gameObject;
        cube.GetComponent<Rigidbody>().isKinematic = false;
        cube.GetComponent<BoxCollider>().enabled = true;
        cube.transform.parent = null;

        inventoryFull.Value = false;
    }

    [ServerRpc]
    private void ThrowItemServerRpc()
    {
        ThrowItem();
    }

    //Void of grabing and puting logic
    private void Grab(Vector3 pos, Vector3 dir)
    {
        RaycastHit HitInfo;

        //Trying to fing nessesary objects by casting ray to view direction
        if (Physics.Raycast(pos, dir, out HitInfo, 3.0f))
        {
            Puzzle pz = GameObject.FindObjectOfType<Puzzle>();

            //Logic if we found cube
            if (HitInfo.transform.gameObject.tag == "PuzzleCube" && !inventoryFull.Value)
            {
                GameObject cube = HitInfo.transform.gameObject;

                if (cube.transform.parent != null && cube.transform.parent.tag == "PuzzleTemp")
                        pz.RemoveCube(int.Parse(cube.transform.parent.gameObject.name.Replace("Temp ", "")));

                cube.GetComponent<Rigidbody>().isKinematic = true;
                cube.GetComponent<BoxCollider>().enabled = false;

                cube.transform.parent = transform;
                cube.transform.localPosition = Vector3.forward;

                inventoryFull.Value = true;
            }
            //Logic if we found spot for cube
            else if(HitInfo.transform.gameObject.tag == "PuzzleTemp" && inventoryFull.Value)
            {
                GameObject cube = transform.GetChild(transform.childCount - 1).gameObject;

                cube.GetComponent<BoxCollider>().enabled = true;
                cube.transform.parent = HitInfo.transform;

                cube.transform.localEulerAngles = Vector3.zero;
                cube.transform.localPosition = Vector3.zero;

                int tempID = int.Parse(HitInfo.transform.gameObject.name.Replace("Temp ", ""));
                pz.PutCube(cube.GetComponent<Cube_cntrl>(), tempID);

                inventoryFull.Value = false;
            }
        }
    }
    
    [ServerRpc]
    private void GrabServerRpc(Vector3 pos, Vector3 dir)
    {
        Grab(pos, dir);
    }
}

