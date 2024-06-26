using Unity.Netcode;
using UnityEngine;

//Simple class for cube logic
public class Cube_cntrl : NetworkBehaviour
{
    public CubeColor colorName;

    //Sets a color to cube
    private void ChangeColor(CubeColor _color)
    {
        colorName = _color;
        Color color = NameToColor(colorName);
        gameObject.GetComponent<Renderer>().material.color = color;
    }

    [ClientRpc]
    public void ChangeColorClientRpc(CubeColor _color)
    {
        ChangeColor(_color);
    }

    //Static void to convert enum CubeColor to UnityEngie.Color (also uses on Puzzle script)
    public static Color NameToColor(CubeColor cName)
    {
        switch (cName)
        {
            case (CubeColor.red):
                return Color.red;

            case (CubeColor.green):
                return Color.green;

            case (CubeColor.blue):
                return Color.blue;

            default: return Color.white;
        }
    }
}

//Simple enum list for nessesary colors
public enum CubeColor
{
    white,
    red,
    green,
    blue,
}