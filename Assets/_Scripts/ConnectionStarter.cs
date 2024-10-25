using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Bayou;
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

        //if (TryGetComponent(out Bayou bayou))
        //    _bayou = bayou;
        //else
        //{
        //    Debug.LogError("Bayou not found", gameObject);
        //    return;
        //}
#if UNITY_EDITOR
        if (_connectionType == ConnectionType.Host)
        {
            if (ParrelSync.ClonesManager.IsClone())
            {
                print("Clone Detected, starting client");
                //_bayou.StartConnection(false);   //dont auto start for client
            }
            else
            {
                print("Host: starting server only");
                _tugboat.StartConnection(true);
                //_bayou.StartConnection(true);
            }

            return;
        }
        print("Not a Host: Starting Client");
        //_bayou.StartConnection(false);  //dont auto start

#endif
#if !UNITY_EDITOR
        //_bayou.StartConnection(true);
        _tugboat.StartConnection(true);
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
