using System.Linq;
using UnityEngine;
using NativeWebSocket;

public class WebSocketManager : Manager<WebSocketManager>
{
    public static WebSocket ServerSocket { get; private set; }

    public enum RequestStatusCodes { OK = 0, ERROR = 1, WARNING = 2, DONE = 3, DEBUG = 4 }
    public delegate void OnStatusUpdate(string message, RequestStatusCodes messageCode);
    public delegate void OnISComplete(int[][] boxes, byte[][] masks);
    public delegate void OnClassification(int objectIndex, int predictionIndex, float confidence);

    public static event OnStatusUpdate OnStatusUpdateEvent;
    public static event OnISComplete OnISCompleteEvent;
    public static event OnClassification OnClassificationEvent;

    [SerializeField]
    private string m_serverAddress = "ws://localhost:8080";

    private void Start()
    {
        ServerSocket = new WebSocket(m_serverAddress);

#if UNITY_EDITOR
        if (debug) ConnectDebugEvents();
#endif

        ServerSocket.OnOpen += () => OnStatusUpdateEvent.Invoke("Connection open!", RequestStatusCodes.OK);
        ServerSocket.OnError += (e) => OnStatusUpdateEvent.Invoke("Error! " + e, RequestStatusCodes.ERROR);

        ServerSocket.OnMessage += ProcessMessage;
    }

    private void ProcessMessage(byte[] bytes)
    {
        // Process as a json string
        string message = System.Text.Encoding.UTF8.GetString(bytes);

        // Parse the json string
        var json = JsonUtility.FromJson<MessageData>(message);

		if (string.IsNullOrEmpty(json.message))
		{
            Debug.LogError("Received message from socket with empty MessageData.message! All messages need to have a message attached. Aborting...");
		}

        switch (json.messageType)
        {
            case MessageData.MessageType.IS:
                ProcessISMessage(json);
                break;
            case MessageData.MessageType.CLASSIFICATION:
                ProcessClassificationMessage(json);
                break;
            case MessageData.MessageType.INFO:
                if (json.message == "pong")
                    OnStatusUpdateEvent.Invoke(json.message, RequestStatusCodes.OK);
                else OnStatusUpdateEvent.Invoke(json.message, RequestStatusCodes.DEBUG);
                break;
            case MessageData.MessageType.ERROR:
                OnStatusUpdateEvent.Invoke(json.message, RequestStatusCodes.ERROR);
                break;
            default:
                Debug.LogError("Received message from socket with unknown MessageType! Aborting...");
                break;
        }
    }

    private void ProcessISMessage(MessageData data)
    {
        Debug.Assert(data.boxes != null, "Received an IS message from socket with no 'boxes' data! Aborting...");
        Debug.Assert(data.masks != null, "Received an IS message from socket with no 'masks' data! Aborting...");
        Debug.Assert(data.boxes.Length == data.masks.Length, "Received message from socket with 'boxes' and 'masks' data but they are not the same length (boxes = " + data.boxes.Length + ", masks = " + data.masks.Length + ")! Aborting...");
        Debug.Assert(data.boxes.Length != 0, "Received message from socket with 'boxes' and 'masks' data but they are empty! Aborting...");
        Debug.Assert(data.boxes.All(x => x.Length == 4), "Received message from socket with 'boxes' data but they are not 4 long! Aborting...");

        byte[][] compressedMasks = new byte[data.masks.Length][]; // Create a jagged array to store the masks
        for (int i = 0; i < data.masks.Length; i++)
        {
            compressedMasks[i] = System.Convert.FromBase64String(data.masks[i]); // Convert each mask from base64 to byte[]
        }

        // Invoke the event
        OnISCompleteEvent.Invoke(data.boxes, compressedMasks);
    }

    private void ProcessClassificationMessage(MessageData data)
    {
        Debug.Assert(data.objectIndex != null, "Received a CLASSIFICATION message from socket with no 'objectIndex' data! Aborting...");
        Debug.Assert(data.predictionIndex != null, "Received a CLASSIFICATION message from socket with no 'predictionIndex' data! Aborting...");
        Debug.Assert(data.confidence != null, "Received a CLASSIFICATION message from socket with no 'confidence' data! Aborting...");

        Debug.Assert(data.objectIndex.Value >= 0, $"Received a CLASSIFICATION message from socket with an 'objectIndex' value that is out of bounds ({data.objectIndex.Value})! Aborting...");
        Debug.Assert(data.predictionIndex.Value >= 0 && data.predictionIndex.Value < TileCatalog.Catalog.Length, $"Received a CLASSIFICATION message from socket with an 'predictionIndex' value that is out of bounds ({data.predictionIndex.Value})! Aborting...");
        Debug.Assert(data.confidence.Value >= 0 && data.confidence.Value <= 1, $"Received a CLASSIFICATION message from socket with a 'confidence' value that is out of bounds ({data.confidence.Value})! Aborting...");

        OnClassificationEvent.Invoke(data.objectIndex.Value, data.predictionIndex.Value, data.confidence.Value);
	}

    public static void SendRequest(string message)
    {
        if (ServerSocket == null)
        {
            Debug.LogError("SendRequest(): ServerSocket is null! Aborting...");
            return;
        }

        if (ServerSocket.State != WebSocketState.Open)
        {
            Debug.LogError("SendRequest(): ServerSocket is not open! Aborting...");
            return;
        }

        ServerSocket.SendText(message);
    }

    public static void SendRequest(Texture2D texture)
    {
        if (ServerSocket == null)
        {
            Debug.LogError("SendRequest(): ServerSocket is null! Aborting...");
            return;
        }

        if (ServerSocket.State != WebSocketState.Open)
        {
            Debug.LogError("SendRequest(): ServerSocket is not open! Aborting...");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        ServerSocket.Send(bytes);
    }

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private bool debug = false;

    private void ConnectDebugEvents()
    {
        ServerSocket.OnOpen += () =>
        {
            Debug.Log("<<Debug Socket>> Connection open!");
        };

        ServerSocket.OnError += (e) =>
        {
            Debug.Log("<<Debug Socket>> Error! " + e);
        };

        ServerSocket.OnClose += (e) =>
        {
            Debug.Log("<<Debug Socket>> Connection closed!\n" + e);
        };

        ServerSocket.OnMessage += (bytes) =>
        {
            Debug.Log("<<Debug Socket>> OnMessage!\n" + System.Text.Encoding.UTF8.GetString(bytes));
        };
    }
#endif

    public struct MessageData
    {
        public enum MessageType { IS = 0, CLASSIFICATION = 1, INFO = 2, ERROR = 3 }

		public string message;
        public MessageType messageType;

		public int[][] boxes;
        public string[] masks;
		public int? objectIndex;
		public int? predictionIndex;
        public float? confidence;
	}
}
