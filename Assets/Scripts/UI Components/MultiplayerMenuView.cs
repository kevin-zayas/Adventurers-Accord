using FishNet;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MultiplayerMenuView : View
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button creditsButton;

    [SerializeField]
    private Button restartServerButton;

    public override void Initialize()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        connectButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());

        exitButton.onClick.AddListener(() => Quit());

        creditsButton.onClick.AddListener(() =>
        {
            CreditsPopUp popUp = Instantiate(Resources.Load<CreditsPopUp>("PopUps/CreditsPopUp"));
            popUp.transform.SetParent(GameObject.Find("Canvas").transform);
            popUp.transform.localPosition = Vector3.zero;
        });

        restartServerButton.onClick.AddListener(() =>
        {
            

            SendPostRequest();
        });

        base.Initialize();
    }

    public void SendPostRequest()
    {
        StartCoroutine(PostRequest());
    }

    private IEnumerator PostRequest()
    {
        print("Restarting Server");
        string token = "efe81a97a8bd587f5b1172d7c025796b";
        string matchId = "6b1a76142bbb4d3ead2733e43c81e77c-studio-us-east.playflow.dev";
        string url = "https://api.cloud.playflow.app/restart_game_server";

        // Create a new UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // Set headers
        request.SetRequestHeader("accept", "application/json");
        request.SetRequestHeader("match-id", matchId);
        request.SetRequestHeader("token", token);

        // Set the POST body (empty in this case)
        byte[] bodyRaw = new byte[0];
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("POST request successful!");
            Debug.Log("Response: " + request.downloadHandler.text);
        }
    }

    public void Quit()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
    Application.Quit();
#elif (UNITY_WEBGL)
    Application.OpenURL("about:blank");
#endif
    }
}
