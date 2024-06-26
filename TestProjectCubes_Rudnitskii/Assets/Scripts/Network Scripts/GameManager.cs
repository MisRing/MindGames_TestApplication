using Unity.Netcode;
using UnityEngine;

//Sample GUI script for start network session
public class GameManager : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 700, 700));

        if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            StartButtons();
        else
            StatusLabels();

        GUILayout.EndArea();
    }

    public void StartButtons()
    {
        if (GUILayout.Button("Host"))
            NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client"))
            NetworkManager.Singleton.StartClient();
    }

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : "Client";

        GUILayout.Label("You are " + mode);
    }
}
