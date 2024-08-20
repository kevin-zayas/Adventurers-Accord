using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    //[SerializeField] string matchId;
    public static ApiManager Instance { get; private set; }
    BuildType buildType;

    string finalToken;
    string finalMatchId;

    string testToken;
    string testMatchId;

    public enum BuildType
    {
        Final,
        Test
    }
    private void Awake()
    {
        Instance = this;
        finalToken = "efe81a97a8bd587f5b1172d7c025796b";
        finalMatchId = "2726223d33a344968466f5b9991d4697-studio-us-east.playflow.dev";

        testToken = "3d65b1b4af0445a224841e24e312b197";
        testMatchId = "17d29124cf324cac8974516fbec4e97e-free-us-east.playflow.dev";

        buildType = BuildType.Test;
    }

    public void RestartGameServer()
    {
        StartCoroutine(SendPostRequest());
    }

    private IEnumerator SendPostRequest()
    {
        print("Restarting Server");
        string token;
        string matchId;
        string url = "https://api.cloud.playflow.app/restart_game_server";

        if (buildType == BuildType.Final)
        {
            token = finalToken;
            matchId = finalMatchId;
        }
        else
        {
            token = testToken;
            matchId = testMatchId;
        }

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
