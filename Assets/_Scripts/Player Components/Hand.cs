using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Hand : NetworkBehaviour
{
    public readonly SyncVar<Player> controllingPlayer = new();

    public readonly SyncVar<int> playerID = new();
}
