using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeploymentManager : MonoBehaviour
{
    private static readonly string apiUrl = "https://api.edgegap.com/v1/";
    private static readonly string authToken = "63cd9a6e-131d-4fe7-893b-c6c750c44b58";

    private static readonly string appName = "adventurers-accord";
    private static readonly string appVersion = "1.0.0.5";
    private static readonly string[] ipList = { "192.168.1.1" };

    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_Text joinGameButtonText;

    [SerializeField] private GameObject networkManagerObject;
    private Tugboat tugboatComponent;

    public static DeploymentManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        tugboatComponent = networkManagerObject.GetComponent<Tugboat>();
    }

    /// <summary>
    /// Starts the deployment management process.
    /// </summary>
    public void InitiateDeploymentCheck()
    {
        StartCoroutine(CheckAndManageDeploymentCoroutine());
    }

    /// <summary>
    /// Manages the deployment lifecycle by checking for active deployments or creating a new one.
    /// </summary>
    private IEnumerator CheckAndManageDeploymentCoroutine()
    {
        UnityWebRequest statusRequest = CreateWebRequest(apiUrl + "deployments", "GET");
        yield return statusRequest.SendWebRequest();

        if (statusRequest.result == UnityWebRequest.Result.Success)
        {
            string response = statusRequest.downloadHandler.text;
            Debug.Log($"Active Deployments: {response}");

            if (IsDeploymentActive(response, out string clientAddress, out int gamePort))
            {
                SetNetworkConfiguration(clientAddress, gamePort);
                SetJoinGameButton(true, "Join Game");
            }
            else
            {
                Debug.Log("No active deployments found. Creating a new deployment...");
                yield return CreateNewDeployment();
            }
        }
        else
        {
            LogError("Failed to check active deployments", statusRequest);
        }
    }

    /// <summary>
    /// Checks if the deployment is active and extracts relevant data
    /// </summary>
    private bool IsDeploymentActive(string response, out string clientAddress, out int gamePort)
    {
        clientAddress = ExtractValueFromJson(response, "fqdn");
        gamePort = int.TryParse(ExtractValueFromJson(response, "external"), out int port) ? port : 0;

        return response.Contains("\"total_count\": 1");
    }

    /// <summary>
    /// Creates a new deployment and waits for it to become ready.
    /// </summary>
    private IEnumerator CreateNewDeployment()
    {
        string jsonPayload = $"{{\"app_name\":\"{appName}\",\"version_name\":\"{appVersion}\",\"ip_list\":[\"{string.Join("\",\"", ipList)}\"]}}";

        UnityWebRequest request = CreateWebRequest(apiUrl + "deploy", "POST", jsonPayload);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log($"Deployment Created Successfully: {response}");

            string requestId = ExtractValueFromJson(response, "request_id");
            print(requestId);
            yield return FetchDeploymentStatus(requestId);
        }
        else
        {
            LogError("Failed to create deployment", request);
        }
    }

    /// <summary>
    /// Fetches the deployment status until it becomes ready.
    /// </summary>
    private IEnumerator FetchDeploymentStatus(string requestId)
    {
        string statusUrl = $"{apiUrl}/status/{requestId}";
        while (true)
        {
            UnityWebRequest statusRequest = CreateWebRequest(statusUrl, "GET");
            yield return statusRequest.SendWebRequest();

            if (statusRequest.result == UnityWebRequest.Result.Success)
            {
                string response = statusRequest.downloadHandler.text;
                string currentStatus = ExtractValueFromJson(response, "current_status");

                if (currentStatus == "Status.READY")
                {
                    string clientAddress = ExtractValueFromJson(response, "fqdn");
                    int gamePort = int.Parse(ExtractValueFromJson(response, "external"));
                    SetNetworkConfiguration(clientAddress, gamePort);
                    SetJoinGameButton(true, "Join Game");
                    yield break;
                }
            }
            else
            {
                LogError("Failed to fetch deployment status", statusRequest);
            }

            yield return new WaitForSeconds(5);
        }
    }

    /// <summary>
    /// Creates a UnityWebRequest with the specified method, URL, and optional payload.
    /// </summary>
    private UnityWebRequest CreateWebRequest(string url, string method, string jsonPayload = "{}")
    {
        UnityWebRequest request = new UnityWebRequest(url, method)
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", $"token {authToken}");
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    /// <summary>
    /// Sets the client address and port for the Tugboat component.
    /// </summary>
    private void SetNetworkConfiguration(string clientAddress, int port)
    {
        tugboatComponent.SetClientAddress(clientAddress);
        tugboatComponent.SetPort((ushort)port);
        Debug.Log($"Client Address set to: {clientAddress}, Game Port set to: {port}");
    }

    /// <summary>
    /// Configures the Join Game button.
    /// </summary>
    private void SetJoinGameButton(bool interactable, string buttonText)
    {
        joinGameButton.interactable = interactable;
        joinGameButtonText.text = buttonText;
    }

    /// <summary>
    /// Extracts a value from JSON using a key.
    /// </summary>
    private string ExtractValueFromJson(string json, string key)
    {
        int startIndex = json.IndexOf($"\"{key}\": ") + $"\"{key}\": ".Length;
        int endIndex = json.IndexOf(',' , startIndex);
        return json[startIndex..endIndex].Trim('"');
    }

    /// <summary>
    /// Logs an error with the details of the UnityWebRequest failure.
    /// </summary>
    private void LogError(string message, UnityWebRequest request)
    {
        Debug.LogError($"{message}: {request.error}");
        Debug.LogError($"Response Code: {request.responseCode}");
        Debug.LogError($"Response Body: {request.downloadHandler.text}");
    }
}
