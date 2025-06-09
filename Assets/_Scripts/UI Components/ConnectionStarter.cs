using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEditor;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    [SerializeField] private ConnectionType _connectionType;

    private Tugboat _tugboat;
    //private Bayou _bayou;

    private void Awake()
    {
        if (TryGetComponent(out Tugboat tugboat))
            _tugboat = tugboat;
        else
        {
            Debug.LogError("Tugboat not found", gameObject);
            return;
        }

#if UNITY_EDITOR
        if (_connectionType == ConnectionType.Host)
        {
            if (ParrelSync.ClonesManager.IsClone())
            {
                print("Clone Detected, cannot host");
                return;
            }

            print("Host: starting server only");
            _tugboat.StartConnection(true);
        }
        return;

#endif
#if !UNITY_EDITOR
    if (_connectionType == ConnectionType.Host)
        {
            _tugboat.StartConnection(true);
        }
        
#endif
    }

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {

#if UNITY_EDITOR
        if (args.ConnectionState == LocalConnectionState.Stopping)
            EditorApplication.isPlaying = false;
#endif
    }

    public enum ConnectionType
    {
        Host,
        Client
    }
}
