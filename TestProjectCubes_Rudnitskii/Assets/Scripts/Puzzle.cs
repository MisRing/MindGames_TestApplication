using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//Logic of puzzle (the biggest part is server code)
public class Puzzle : NetworkBehaviour
{
    [SerializeField] private GameObject cubePref;

    private List<GameObject> _cubes = new List<GameObject>();

    //Start point for new cubes
    private Vector3 cubeSpawnPoint = Vector3.up;

    //Nessesary colors in right order
    [SerializeField] private CubeColor[] _pattern = new CubeColor[9];
    [SerializeField] private Renderer[] _patternCubes = new Renderer[9];

    private Cube_cntrl[] cubes = new Cube_cntrl[9];
    private int cubesOnPuzzle = 0;

    //Resets all puzzle, clearing cubes and sets new pattern
    public void UpdatePuzzle()
    {
        cubesOnPuzzle = 0;

        //A little crutch to remove little bug
        foreach (PlayerInteraction pi in FindObjectsOfType<PlayerInteraction>())
            pi.inventoryFull.Value = false;

        //Creating new pattern
        if (IsServer)
        {
            CubeColor[] cubeCol = new CubeColor[9];

            for (int i = 0; i < cubeCol.Length; i++)
            {
                cubeCol[i] = (CubeColor)Random.Range(0, 4);
            }

            UpdatePatternClientRpc(cubeCol);
        }

        //Clearing old cubes
        foreach (var c in _cubes)
        {
            Destroy(c);
        }

        _cubes.Clear();

        //Creating new cubes
        CreateCubes();
    }

    //Void to use update puzzle method by players
    public void UpdatePattern(CubeColor[] cubeCol)
    {
        for (int i = 0; i < _pattern.Length; i++)
        {
            _pattern[i] = cubeCol[i];
            _patternCubes[i].material.color = Cube_cntrl.NameToColor(_pattern[i]);
        }
    }

    [ClientRpc]
    public void UpdatePatternClientRpc(CubeColor[] cubeCol)
    {
        UpdatePattern(cubeCol);
    }

    //Void of creating and spawning new cubes
    void CreateCubes()
    {
        for (int i = 0; i < 9; i++)
        {
            //Randomizing spawn point
            Vector3 pos = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-0.5f, 2.5f), Random.Range(-1.5f, 1.5f));
            pos += cubeSpawnPoint;

            //Randomizing start rotation
            Vector3 angels = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            Quaternion rotatinon = Quaternion.Euler(angels);

            //Instancing cube like NetworkObject
            var cube = Instantiate(cubePref, pos, rotatinon);
            var instanceNetworkObject = cube.GetComponent<NetworkObject>();
            instanceNetworkObject.Spawn();

            if (IsServer)
            {
                //Coloring cubes to right colors
                CubeColor cubeColor = _pattern[i];
                cube.GetComponent<Cube_cntrl>().ChangeColorClientRpc(cubeColor);
            }

            _cubes.Add(cube);
        }
    }

    //Checking that player put cube to puzzle
    public void PutCube(Cube_cntrl cube, int tempID)
    {
        cubes[tempID] = cube;

        cubesOnPuzzle++;

        if (cubesOnPuzzle == 9)
            CheckPuzzle();
    }

    //Removing cube when player took it
    public void RemoveCube(int tempID)
    {
        cubes[tempID] = null;
        cubesOnPuzzle--;
    }

    //Cheching is puzzle solvet right
    private void CheckPuzzle()
    {
        for (int i = 0; i < 9; i++)
        {
            if (cubes[i].colorName != _pattern[i])
                return;
        }

        UpdatePuzzle();
    }
}
