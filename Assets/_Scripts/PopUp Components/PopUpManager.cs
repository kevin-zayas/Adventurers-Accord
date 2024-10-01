using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PopUpManager : NetworkBehaviour
{
    public static PopUpManager Instance { get; private set; }


    [SerializeField] CreditsPopUp CreditsPopUpPrefab;
    [SerializeField] GameOverPopUp GameOverPopUpPrefab;
    [SerializeField] HowToPlayPopUp HowToPlayPopUpPrefab;
    [SerializeField] ResolutionPopUp ResolutionPopUpPrefab;
    [SerializeField] RoundSummaryPopUp RoundSummaryPopUpPrefab;
    [SerializeField] ConfirmationPopUp ConfirmationPopUpPrefab;
    [SerializeField] ConfirmationPopUp EquipConfirmationPopUpPrefab;

    public readonly SyncVar<ResolutionPopUp> CurrentResolutionPopUp = new();
    public readonly SyncVar<GameOverPopUp> GameOverPopUpInstance = new();

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public ResolutionPopUp CreateResolutionPopUp()
    {
        print("Creating PopUp");
        ResolutionPopUp popUp = Instantiate(ResolutionPopUpPrefab);
        CurrentResolutionPopUp.Value = popUp;
        return popUp;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerDespawnResolutionPopUp(ResolutionPopUp popUp)
    {
        print("Despawning PopUp");
        Despawn(popUp.gameObject);
    }

    [Server]
    public RoundSummaryPopUp CreateRoundSummaryPopUp()
    {
        print("Creating Round Summary PopUp");
        RoundSummaryPopUp popUp = Instantiate(RoundSummaryPopUpPrefab);
        return popUp;
    }

    [Server]
    public void CloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp, bool despawn)
    {
        TargetCloseRoundSummaryPopUp(networkConnection, popUp);

        if (despawn)
        {
            Despawn(popUp);
        }
    }

    [TargetRpc]
    public void TargetCloseRoundSummaryPopUp(NetworkConnection networkConnection, GameObject popUp)
    {
        //if (IsServer) return;
        print("closing round summary pop up");
        popUp.SetActive(false);

        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.GameOver)
        {
            Player player = GameManager.Instance.Players[LocalConnection.ClientId];
            this.GameOverPopUpInstance.Value.ServerInitializeGameOverPopup(networkConnection, player);
        }

    }

    [Server]
    public GameOverPopUp CreateGameOverPopUp()
    {
        print("Creating GameOver PopUp");
        GameOverPopUp popUp = Instantiate(GameOverPopUpPrefab);
        GameOverPopUpInstance.Value = popUp;
        return popUp;
    }

    public ConfirmationPopUp CreateConfirmationPopUp()
    {
        ConfirmationPopUp popUp = Instantiate(ConfirmationPopUpPrefab);
        return popUp;
    }

    public ConfirmationPopUp CreateConfirmationPopUp(bool itemPopUp)
    {
        ConfirmationPopUp popUp = Instantiate(EquipConfirmationPopUpPrefab);

        return popUp;
    }

    public HowToPlayPopUp CreateHowToPlayPopUp()
    {
        HowToPlayPopUp howToPlayPopUp = Instantiate(HowToPlayPopUpPrefab);
        howToPlayPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
        howToPlayPopUp.transform.localPosition = Vector3.zero;

        RectTransform rt = howToPlayPopUp.GetComponent<RectTransform>();        // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        return howToPlayPopUp;
    }

    public CreditsPopUp CreateCreditsPopUp()
    {
        CreditsPopUp creditsPopUp = Instantiate(CreditsPopUpPrefab);
        creditsPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
        creditsPopUp.transform.localPosition = Vector3.zero;

        RectTransform rt = creditsPopUp.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;

        return creditsPopUp;
    }
}
