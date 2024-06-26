using Unity.Netcode.Components;

//Simple script that allows clients to have acces to changing transform.parent of NetworkObject's
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
