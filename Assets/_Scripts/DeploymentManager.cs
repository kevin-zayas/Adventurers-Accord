using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeploymentManager : MonoBehaviour
{
    private static readonly string deploymentApiUrl = "https://api.edgegap.com/v1/deploy";
    private static readonly string deploymentsStatusApiUrl = "https://api.edgegap.com/v1/deployments";
    private static readonly string authToken = "63cd9a6e-131d-4fe7-893b-c6c750c44b58";

    private static readonly string appName = "adventurers-accord";
    private static readonly string appVersion = "1.0.0.5";
    private static readonly string[] ipList = { "192.168.1.1" };

    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_Text joinGameText;

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

    // Entry point for deployment management
    public void InitiateDeploymentCheck()
    {
        StartCoroutine(CheckAndManageDeploymentCoroutine());
    }

    // Coroutine to check and manage deployment state
    private IEnumerator CheckAndManageDeploymentCoroutine()
    {
        // Step 1: Check if there are any active deployments
        UnityWebRequest statusRequest = CreateRequest(deploymentsStatusApiUrl, "GET");
        yield return statusRequest.SendWebRequest();

        if (statusRequest.result == UnityWebRequest.Result.Success)
        {
            string response = statusRequest.downloadHandler.text;
            Debug.Log($"Active Deployments: {response}");

            // Step 2: Check if any active deployment exists
            if (IsDeploymentActive(response, out string clientAddress, out int gamePort))
            {
                Debug.Log($"Active deployment found. Host: {clientAddress}, Game Port: {gamePort}");

                tugboatComponent.SetClientAddress(clientAddress);
                tugboatComponent.SetPort((ushort)gamePort);

                Debug.Log($"Client Adress set to: {clientAddress}");
                Debug.Log($"Game Port set to: {gamePort}");

                joinGameButton.interactable = true;
                joinGameText.text = "Join Game";
            }
            else
            {
                Debug.Log("No active deployments found. Creating a new deployment...");
                // Step 3: Create a new deployment if none exists
                yield return CreateNewDeployment();
            }
        }
        else
        {
            Debug.LogError($"Failed to check active deployments: {statusRequest.error}");
        }
    }

    // Helper method to check if the deployment is active and extract relevant data
    private bool IsDeploymentActive(string response, out string fqdn, out int portNumber)
    {
        fqdn = null;
        portNumber = 0;

        // Check if the response contains active deployments
        if (response.Contains("\"total_count\": 1"))
        {
            fqdn = GetDeploymentClientAddress(response);
            portNumber = GetDeploymentPortNumber(response);

            return true;
        }
        return false;
    }

    // Create a request with the appropriate method and URL
    private UnityWebRequest CreateRequest(string url, string method)
    {
        UnityWebRequest request = new UnityWebRequest(url, method)
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}")),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", $"token {authToken}");
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    // Create a new deployment and store the request_id for later
    private IEnumerator CreateNewDeployment()
    {
        // Manually create JSON payload
        string jsonPayload = $"{{\"app_name\":\"{appName}\",\"version_name\":\"{appVersion}\",\"ip_list\":[\"{string.Join("\",\"", ipList)}\"]}}";

        Debug.Log($"Payload: {jsonPayload}"); // Log the payload to verify its structure

        // Configure UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(deploymentApiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Set headers
        request.SetRequestHeader("Authorization", $"token {authToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Handle response
        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log($"Deployment Created Successfully: {response}");

            string requestId = GetDeploymentRequestId(response);
            Debug.Log($"Request ID stored: {requestId}");

            // Now fetch the status of the deployment
            yield return FetchDeploymentStatus(requestId);
        }
        else
        {
            Debug.LogError($"Error: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Body: {request.downloadHandler.text}");
        }
    }

    // Fetch the status of the deployment using the given request_id
    private IEnumerator FetchDeploymentStatus(string requestId)
    {
        string statusUrl = $"https://api.edgegap.com/v1/status/{requestId}";
        string currentStatus = null;
        string clientAddress = null;
        int gamePort = 0;

        while (true)
        {
            UnityWebRequest statusRequest = CreateRequest(statusUrl, "GET");
            yield return statusRequest.SendWebRequest();

            if (statusRequest.result == UnityWebRequest.Result.Success)
            {
                string response = statusRequest.downloadHandler.text;
                Debug.Log($"Deployment Status: {response}");

                // Extract the current status
                currentStatus = GetDeploymentCurrentStatus(response);
                Debug.Log($"Current Status: {currentStatus}");

                // Check if the deployment is ready
                if (currentStatus == "Status.READY")
                {
                    Debug.Log("Deployment is ready!");

                    clientAddress = GetDeploymentClientAddress(response);
                    gamePort = GetDeploymentPortNumber(response);
                    break;
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch deployment status: {statusRequest.error}");
            }

            // Wait for a few seconds before the next check
            yield return new WaitForSeconds(5);
        }

        // Set the client address and port once ready
        tugboatComponent.SetClientAddress(clientAddress);
        tugboatComponent.SetPort((ushort)gamePort);

        Debug.Log($"Client Address set to: {clientAddress}");
        Debug.Log($"Game Port set to: {gamePort}");

        joinGameButton.interactable = true;
        joinGameText.text = "Join Game";
    }

    // Extracts the current status from the response
    private string GetDeploymentCurrentStatus(string response)
    {
        int statusStartIndex = response.IndexOf("\"current_status\": \"") + "\"current_status\": \"".Length;
        int statusEndIndex = response.IndexOf("\"", statusStartIndex);
        string status = response[statusStartIndex..statusEndIndex];
        return status;
    }

    private string GetDeploymentClientAddress(string response)
    {
        int fqdnStartIndex = response.IndexOf("\"fqdn\": \"") + "\"fqdn\": \"".Length;
        int fqdnEndIndex = response.IndexOf("\"", fqdnStartIndex);
        string fqdn = response[fqdnStartIndex..fqdnEndIndex];

        return fqdn;
    }

    private int GetDeploymentPortNumber(string response)
    {
        int gamePortStartIndex = response.IndexOf("\"external\": ") + "\"external\": ".Length;
        int gamePortEndIndex = response.IndexOf(",", gamePortStartIndex);
        string gamePortString = response[gamePortStartIndex..gamePortEndIndex];
        int gamePort = int.Parse(gamePortString);

        return gamePort;
    }

    private string GetDeploymentRequestId(string response)
    {
        int requestIdStartIndex = response.IndexOf("\"request_id\": \"") + "\"request_id\": \"".Length;
        int requestIdEndIndex = response.IndexOf("\"", requestIdStartIndex);
        string requestId = response[requestIdStartIndex..requestIdEndIndex];

        return requestId;
    }
}
