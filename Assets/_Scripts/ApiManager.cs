using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    //[SerializeField] string matchId;
    public static ApiManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public void RestartGameServer()
    {
        StartCoroutine(SendPostRequest());
    }

    private IEnumerator SendPostRequest()
    {
        print("Restarting Server");
        //string token = "efe81a97a8bd587f5b1172d7c025796b";
        //string matchId = "cbb7a6acd06643c4a12e631864ed0b35-studio-us-east.playflow.dev";

        string token = "3d65b1b4af0445a224841e24e312b197";
        string matchId = "492d6255d4d943ebaf78f2f9a11e0052-free-us-east.playflow.dev";
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


}
