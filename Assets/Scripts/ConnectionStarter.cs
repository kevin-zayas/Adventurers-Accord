using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Bayou;
using FishNet.Transporting.Tugboat;
using UnityEditor;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    [SerializeField] private ConnectionType _connectionType;

    //private Tugboat _tugboat;
    private Bayou _bayou;

    private void Awake()
    {
        //if (TryGetComponent(out Tugboat tug))
        //    _tugboat = tug;
        //else
        //{
        //    Debug.LogError("Tugboat not found", gameObject);
        //    return;
        //}
        if (TryGetComponent(out Bayou bayou))
            _bayou = bayou;
        else
        {
            Debug.LogError("Bayou not found", gameObject);
            return;
        }
#if UNITY_EDITOR
        if (_connectionType == ConnectionType.Host)
        {
            if (ParrelSync.ClonesManager.IsClone())
            {
                //_tugboat.StartConnection(false);
                _bayou.StartConnection(false);
            }
            else
            {
                //_tugboat.StartConnection(true);
                //_tugboat.StartConnection(false);
                _bayou.StartConnection(true);
            }

            return;
        }

        _bayou.StartConnection(false);

#endif
#if !UNITY_EDITOR
        _bayou.StartConnection(true);
        //_tugboat.StartConnection(true);
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
