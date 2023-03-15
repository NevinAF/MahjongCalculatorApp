using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProcessCameraManager : Manager<ProcessCameraManager>
{
	public TMP_Text StatusText;
	public Slider ProgressSlider;
	public GameObject TilePredictionPrefab;
	public Transform TilePredictionParent;
	public List<TilePrediction> TilePredictions { get; private set; }

	private Texture2D m_texture2D;
	private Color32Array m_color32Array = new Color32Array();
	private bool initialized = false;

	/// <summary>
	/// State of the processing:
	///     -1 = not started
	///     0 = Sent Request - 10% Done
	///     1 = Request Acknowledged and Processing - 20% Done
	///     2 = IS Complete / Classification Running - 70% Done
	///     3 = Done Successfully - 100% Done
	/// </summary>
	private int _state = -1;
	private int State { get { return _state; } set {
		if (value == _state) return;
		if (value != -1 && value != _state + 1)
		{
			DebugLogBadState("State was set to an invalid value", _state, value);
			return;
		}
		_state = value; 
	} }

	/// <summary>
	/// When the object is enabled, assume that the current camera texture is the one we want to use for processing.
	/// </summary>
	private void OnEnable()
	{
		if (m_color32Array.colors == null || m_color32Array.colors.Length != WebCameraManager.WebCamTexture.width * WebCameraManager.WebCamTexture.height)
			m_color32Array.colors = new Color32[WebCameraManager.WebCamTexture.width * WebCameraManager.WebCamTexture.height];

		WebCameraManager.WebCamTexture.GetPixels32(m_color32Array.colors);

		if (m_texture2D == null || m_texture2D.width != WebCameraManager.WebCamTexture.width || m_texture2D.height != WebCameraManager.WebCamTexture.height)
			m_texture2D = new Texture2D(WebCameraManager.WebCamTexture.width, WebCameraManager.WebCamTexture.height, TextureFormat.RGBA32, false);

		m_texture2D.SetPixels32(m_color32Array.colors);
		m_texture2D.Apply();

		WebSocketManager.OnStatusUpdateEvent += OnStatusUpdate;
		WebSocketManager.OnISCompleteEvent += OnISComplete;
		WebSocketManager.OnClassificationEvent += OnClassification;

		State = 0;
		StatusText.text = "Sending Image to Server for Processing...";
		ProgressSlider.value = 0.1f + UnityEngine.Random.Range(-0.025f, 0.025f);
		WebSocketManager.SendTextureRequest(m_texture2D.EncodeToPNG());
	}

	private void OnDisable()
	{
		WebSocketManager.OnStatusUpdateEvent -= OnStatusUpdate;
		WebSocketManager.OnISCompleteEvent -= OnISComplete;
		WebSocketManager.OnClassificationEvent -= OnClassification;

		WebSocketManager.SendStringRequest("Cancel");
		State = -1;
	}

	private void OnStatusUpdate(string message, WebSocketManager.RequestStatusCodes messageCode)
	{
		if (messageCode == WebSocketManager.RequestStatusCodes.OK && message == "IS Started")
		{
			State = 1;
			StatusText.text = "Image is being processed for tiles...";
			ProgressSlider.value = 0.2f + UnityEngine.Random.Range(-0.04f, 0.04f);
		}
	}

	private void OnISComplete(int[][] boxes, byte[][] masks)
	{
		State = 2;

		TilePredictions = new List<TilePrediction>();

		for (int i = 0; i < boxes.Length; i++)
		{
			TilePrediction tilePrediction = Instantiate(TilePredictionPrefab, TilePredictionParent).GetComponent<TilePrediction>();
			tilePrediction.Mask = new Mask(boxes[i], masks[i]);
			TilePredictions.Add(tilePrediction);
		}

		StatusText.text = "Tiles are being classified (" + TilePredictions.Count + " tiles)...";
		ProgressSlider.value = 0.7f + UnityEngine.Random.Range(-0.16f, 0.1f);
	}

	private void OnClassification(int[] predictionIndices, float[] confidences)
	{
		for (int i = 0; i < predictionIndices.Length; i++)
			TilePredictions[i].SetPredictionIndex(predictionIndices[i], confidences[i]);

		State = 3;
		StatusText.text = "Done!";
		ProgressSlider.value = 1f;
	}

	private void DebugLogBadState(string message, int currentState, int expectedState)
	{
		Debug.LogError("ProcessCameraManager Bad State: " + message + " - Current State: " + currentState + " - Trying to set State: " + expectedState);
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Color32Array
	{
		[FieldOffset(0)]
		public byte[] byteArray;

		[FieldOffset(0)]
		public Color32[] colors;
	}
}
