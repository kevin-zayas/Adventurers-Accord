using FishNet.Object.Synchronizing;

public class Hand : CardHolder
{
    //public static Hand Instance { get; private set; }
    public readonly SyncVar<Player> controllingPlayer = new();
    public readonly SyncVar<int> playerID = new();

    protected override void Start()
    {
        base.Start();
        HolderType = CardHolderType.Hand;
    }
}
