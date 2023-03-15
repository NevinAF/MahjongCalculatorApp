using System.Linq;
using UnityEngine;
using NativeWebSocket;
using System;
using System.Threading.Tasks;

public class WebSocketManager : Manager<WebSocketManager>
{
	public static WebSocket ServerSocket { get; private set; }

	public enum RequestStatusCodes { OK = 0, ERROR = 1, WARNING = 2, DONE = 3, DEBUG = 4 }
	public delegate void OnStatusUpdate(string message, RequestStatusCodes messageCode);
	public delegate void OnISComplete(int[][] boxes, byte[][] masks);
	public delegate void OnClassification(int[] predictionIndices, float[] confidences);

	public static event OnStatusUpdate OnStatusUpdateEvent;
	public static event OnISComplete OnISCompleteEvent;
	public static event OnClassification OnClassificationEvent;

	[SerializeField]
	private string m_serverAddress = "ws://localhost:3500";

	private static Task SocketTask;

	private void Start()
	{
		ServerSocket = new WebSocket(m_serverAddress);

#if UNITY_EDITOR
		if (debug) ConnectDebugEvents();
#endif

		ServerSocket.OnOpen += () => { if (OnStatusUpdateEvent != null) OnStatusUpdateEvent.Invoke("Connection open!", RequestStatusCodes.OK); };
		ServerSocket.OnError += (e) => { if (OnStatusUpdateEvent != null) OnStatusUpdateEvent.Invoke("Error! " + e, RequestStatusCodes.ERROR); };


		ServerSocket.OnMessage += ProcessMessage;
	}

	public static bool CheckServerConnection(bool requestIfNot)
	{
		// This function will also try to restart the connection if it is not open

		if (ServerSocket.State == WebSocketState.Open)
			return true;

		if (ServerSocket.State == WebSocketState.Closed)
		{
			if (requestIfNot)
			{
				if (SocketTask == null || SocketTask.Status == TaskStatus.RanToCompletion)
				{
					SocketTask = new Task(Instance.ConnectToSocket);
					SocketTask.Start();
				}
			}
		}

		return false;
	}

	private void Update()
	{
		if (ServerSocket.State == WebSocketState.Open)
		{
			ServerSocket.DispatchMessageQueue();
		}
	}

	private async void ConnectToSocket()
	{
		UnityEngine.Debug.Log("Attempting to connect to socket...");
		await ServerSocket.Connect();
		UnityEngine.Debug.Log("Connected to socket!");
	}

	private void ProcessMessage(byte[] bytes)
	{
		// Process as a json string
		string message = System.Text.Encoding.UTF8.GetString(bytes);

		// Parse the json string
		var json = JsonUtility.FromJson<MessageData>(message);

		UnityEngine.Debug.Log("Received message from socket: " + json);

		if (string.IsNullOrEmpty(json.message))
		{
			Debug.LogError("Received message from socket with empty MessageData.message! All messages need to have a message attached. Aborting...");
			return;
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
				OnStatusUpdateEvent.Invoke(json.message, RequestStatusCodes.OK);
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
		Debug.Assert(data.boxes.Length / 4 == data.masks.Length, "Received message from socket with 'boxes' and 'masks' data but they are not the same length (boxes = " + data.boxes.Length + ", masks = " + data.masks.Length + ")! Aborting...");
		Debug.Assert(data.boxes.Length != 0, "Received message from socket with 'boxes' and 'masks' data but they are empty! Aborting...");
		Debug.Assert(data.boxes.Length % 4 == 0, "Received message from socket with 'boxes' data but they are not 4 long! Aborting...");

		byte[][] compressedMasks = new byte[data.masks.Length][]; // Create a jagged array to store the masks
		for (int i = 0; i < data.masks.Length; i++)
		{
			compressedMasks[i] = System.Convert.FromBase64String(data.masks[i]); // Convert each mask from base64 to byte[]
		}

		int[][] boxes = new int[data.boxes.Length / 4][]; // Create a jagged array to store the boxes
		for (int i = 0; i < data.boxes.Length / 4; i++)
		{
			boxes[i] = new int[4]; // Create a new array for each box
			for (int j = 0; j < 4; j++)
			{
				boxes[i][j] = data.boxes[i * 4 + j]; // Add each box value to the array
			}
		}

		// Invoke the event
		OnISCompleteEvent.Invoke(boxes, compressedMasks);
	}

	private void ProcessClassificationMessage(MessageData data)
	{
		// Debug.Assert(data.objectIndex != null, "Received a CLASSIFICATION message from socket with no 'objectIndex' data! Aborting...");
		Debug.Assert(data.predictionIndices != null, "Received a CLASSIFICATION message from socket with no 'predictionIndices' data! Aborting...");
		Debug.Assert(data.confidences != null, "Received a CLASSIFICATION message from socket with no 'confidences' data! Aborting...");

		// Debug.Assert(data.objectIndex.Value >= 0, $"Received a CLASSIFICATION message from socket with an 'objectIndex' value that is out of bounds ({data.objectIndex.Value})! Aborting...");
		Debug.Assert(data.predictionIndices.All(x => x >= 0) && data.predictionIndices.All(x => x < TileCatalog.Catalog.Length), $"Received a CLASSIFICATION message from socket with an 'predictionIndices' value that is out of bounds ({data.predictionIndices.Format()})! Aborting...");
		Debug.Assert(data.confidences.All(x => x >= 0) && data.confidences.All(x => x <= 1), $"Received a CLASSIFICATION message from socket with a 'confidences' value that is out of bounds ({data.confidences.Format()})! Aborting...");

		OnClassificationEvent.Invoke(data.predictionIndices, data.confidences);
	}

	public static void SendStringRequest(string message)
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

		SendRequest(System.Text.Encoding.UTF8.GetBytes(message), (byte)MessageTypes.STRING);
	}

	public static void SendTextureRequest(byte[] texture)
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

		SendRequest(texture, (byte)MessageTypes.IMAGE);
	}

	private static void SendRequest(byte[] data, byte type)
	{
		// TODO - Should not be doing a copy here and instead should be adding the extra space when creating the request
		// Send the request, prefixed with the length of the data in bytes and a byte to indicate the type of data
		byte[] length = BitConverter.GetBytes(data.Length);
		byte[] request = new byte[data.Length + 5];
		request[0] = type;
		length.CopyTo(request, 1);
		data.CopyTo(request, 5);

		print("Sending request to server: " + type + " " + data.Length);

		SocketTask = new Task(() => ServerSocket.Send(request));
		SocketTask.Start();
	}

	async private void OnApplicationQuit()
	{
		if (ServerSocket != null)
		{
			await ServerSocket.Close();
		}
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

		public int[] boxes;
		public string[] masks;
		public int[] predictionIndices;
		public float[] confidences;

		public override string ToString()
		{
			return $"{{\nmessage: {message},\nmessageType: {messageType},\nboxes: {(boxes == null ? "null" : boxes.Format())},\nmasks: {(masks == null ? "null" : masks.Format())},\npredictionIndices: {(predictionIndices == null ? "null" : predictionIndices.Format())},\nconfidences: {(confidences == null ? "null" : confidences.Format())}\n}}";
		}
	}

	public enum MessageTypes
	{
		STRING = 0,
		IMAGE = 1
	}
}
