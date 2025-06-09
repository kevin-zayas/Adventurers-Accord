using FishNet.Transporting.Tugboat;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MatchmakerManager : MonoBehaviour
{
    private static readonly string baseUrl = "https://om-7wjh0llgan.edgegap.net";
    private static readonly string matchAuthToken = "f0d40dca-662c-4d64-9dc2-9c44327d2cca";

    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_Text joinGameButtonText;

    [SerializeField] private GameObject networkManagerObject;
    private Tugboat tugboatComponent;

    public static MatchmakerManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (networkManagerObject) tugboatComponent = networkManagerObject.GetComponent<Tugboat>();
    }

    /// <summary>
    /// Starts the deployment management process.
    /// </summary>
    public void InitiateMatchmaking()
    {
        StartCoroutine(CreateMatchmakingTicket());
    }

    /// <summary>
    /// Creates a matchmaking ticket by sending a POST request to the matchmaking API.
    /// </summary>
    /// <returns>Coroutine for the matchmaking ticket request.</returns>
    public IEnumerator CreateMatchmakingTicket()
    {
        string url = $"{baseUrl}/tickets";

        // Construct the request body using defined classes
        var requestBodyObject = new MatchmakingRequest
        {
            player_ip = null,
            profile = "simple-example",
            attributes = new Attributes
            {
                beacons = new Beacons
                {
                    Montreal = 12.3f,
                    Toronto = 45.6f,
                    Quebec = 78.9f
                }
            }
        };

        string requestBody = JsonConvert.SerializeObject(requestBodyObject,
        new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include
        });

        UnityWebRequest request = CreateWebRequest(url, "POST", matchAuthToken, requestBody);
        Debug.Log("POST - Creating matchmaking ticket");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;

            // Use a custom class to parse the response JSON
            var response = JsonUtility.FromJson<MatchmakingResponse>(responseText);

            if (response.status == "SEARCHING")
            {
                string ticketId = response.id;
                Debug.Log($"Matchmaking ticket created. ID: {ticketId}");

                yield return GetMatchmakingTicketStatus(ticketId);
            }
            else
            {
                Debug.Log($"Unhandled status: {response.status}");
            }
        }
        else
        {
            LogError("Failed to create matchmaking ticket", request);
            print(request);
        }
    }

    /// <summary>
    /// Periodically fetches the matchmaking ticket status and performs actions based on the status.
    /// </summary>
    /// <param name="ticketId">The ID of the matchmaking ticket.</param>
    private IEnumerator GetMatchmakingTicketStatus(string ticketId)
    {
        string url = $"{baseUrl}/tickets/{ticketId}";

        while (true)
        {
            UnityWebRequest statusRequest = CreateWebRequest(url, "GET", matchAuthToken);
            Debug.Log("GET - Checking matchmaking ticket status");
            yield return statusRequest.SendWebRequest();

            if (statusRequest.result == UnityWebRequest.Result.Success)
            {
                string responseText = statusRequest.downloadHandler.text;
                Debug.Log($"Matchmaking status response: {responseText}");

                //var response = JsonUtility.FromJson<MatchmakingStatusResponse>(responseText);
                MatchmakingStatusResponse response = JsonConvert.DeserializeObject<MatchmakingStatusResponse>(responseText);

                switch (response.status)
                {
                    case "HOST_ASSIGNED":
                        if (response.assignment != null)
                        {
                            print(response.assignment.ports);
                            string fqdn = response.assignment.fqdn;
                            int externalPort = response.assignment.ports["Game Port"].external;


                            SetNetworkConfiguration(fqdn, externalPort);
                            SetJoinGameButton(true, "Join Game");

                            Debug.Log($"Host assigned. FQDN: {fqdn}, External Port: {externalPort}");
                            yield break; // Exit the coroutine
                        }
                        else
                        {
                            Debug.LogWarning("HOST_ASSIGNED status received but no assignment details found.");
                        }
                        break;

                    case "MATCH_FOUND":
                        Debug.Log("Match found.");
                        break;

                    default:
                        Debug.Log($"Unhandled status: {response.status}");
                        break;
                }
            }
            else
            {
                LogError("Failed to fetch matchmaking status", statusRequest);
            }

            yield return new WaitForSeconds(5); // Retry after delay
        }
    }

    [System.Serializable]
    public class MatchmakingRequest
    {
        public string player_ip;
        public string profile;
        public Attributes attributes;
    }

    [System.Serializable]
    public class Attributes
    {
        public Beacons beacons;
    }

    [System.Serializable]
    public class Beacons
    {
        public float Montreal;
        public float Toronto;
        public float Quebec;
    }

    /// <summary>
    /// Data structure to parse the matchmaking ticket response.
    /// </summary>
    [System.Serializable]
    private class MatchmakingResponse
    {
        public string id;
        public string profile;
        public string group_id;
        public string player_ip;
        public string assignment;
        public string created_at;
        public string status;
    }

    /// <summary>
    /// Data structure for parsing matchmaking status response.
    /// </summary>
    private class MatchmakingStatusResponse
    {
        public string id { get; set; }
        public string profile { get; set; }
        public string group_id { get; set; }
        public string player_ip { get; set; }
        public MatchmakingAssignment assignment { get; set; }
        public string created_at { get; set; }
        public string status { get; set; }
    }

    /// <summary>
    /// Data structure for assignment details in the matchmaking response.
    /// </summary>
    private class MatchmakingAssignment
    {
        public string fqdn { get; set; }
        public string public_ip { get; set; }
        public Dictionary<string, MatchmakingPort> ports { get; set; }
        public MatchmakingLocation location { get; set; }
    }

    /// <summary>
    /// Data structure for port details in the assignment.
    /// </summary>
    private class MatchmakingPort
    {
        [JsonProperty("internal")] // Map "internal" field to avoid conflicts with the C# keyword.
        public int internalPort { get; set; }
        public int external { get; set; }
        public string link { get; set; }
        public string protocol { get; set; }
    }

    /// <summary>
    /// Data structure for location details in the assignment.
    /// </summary>
    private class MatchmakingLocation
    {
        public string city { get; set; }
        public string country { get; set; }
        public string continent { get; set; }
        public string administrative_division { get; set; }
        public string timezone { get; set; }
    }

    /// <summary>
    /// Creates a UnityWebRequest with the specified method, URL, and optional payload.
    /// </summary>
    private UnityWebRequest CreateWebRequest(string url, string method, string authToken, string jsonPayload = "{}")
    {
        UnityWebRequest request = new UnityWebRequest(url, method)
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", authToken);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Access-Control-Allow-Origin", "*");
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
        int endIndex = json.IndexOf(',', startIndex);
        return json[startIndex..endIndex].Trim('"');
    }

    /// <summary>
    /// Logs an error with the details of the UnityWebRequest failure.
    /// </summary>
    private void LogError(string message, UnityWebRequest request)
    {
        Debug.LogError($"\nResponse Code: {request.responseCode}");
        Debug.LogError($"\n{message}: {request.error}");
        Debug.LogError($"\nResponse Body: {request.downloadHandler.text}");
        Debug.LogError($"\nIs Connnection Error: {request.result == UnityWebRequest.Result.ConnectionError}");
        Debug.LogError($"\nResult: {request.result}");
    }
}
