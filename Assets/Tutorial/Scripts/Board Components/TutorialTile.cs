using UnityEngine;

public class TutorialTile : MonoBehaviour
{
    [field: SerializeField]
    public Transform[] PawnPositions { get; private set; }

    public TutorialPlayer owningPlayer;

    [SerializeField]
    private MeshRenderer meshRenderer;

    private Color _defaultColor;

    private void Start()
    {
        _defaultColor = meshRenderer.sharedMaterial.color;
    }

    private void LateUpdate()
    {
        if (owningPlayer == null)
        {
            meshRenderer.material.color = _defaultColor;

            return;
        }
        // set borders of cards in a similar way
        meshRenderer.material.color = TutorialGameManager.Instance.Players.IndexOf(owningPlayer) switch
        {
            0 => Color.red,
            1 => Color.blue,
            2 => Color.green,
            3 => Color.yellow,

            _ => Color.white
        };

    }


}