using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WebCameraManager : MonoBehaviour
{
	public static readonly Rect DefaultRect = new Rect(0, 0, 1, 1);
	public static readonly Rect VerticalFlippedRect = new Rect(1, 0, -1, 1);

	public static WebCameraManager Instance { get; private set; }

	private WebCamTexture m_webCamTexture;
	public static WebCamTexture WebCamTexture
	{
		get => Instance.m_webCamTexture;
		private set {
			if (Instance.m_webCamTexture != null)
			{
				Instance.m_webCamTexture.Stop();
			}

			Instance.m_webCamTexture = value;

			Instance.UpdateRawImages();
		}
	}

	private int m_currentCamera = -1;
	public static int CameraIndex
	{
		get => Instance.m_currentCamera;
		set {
			if (Instance.m_currentCamera == value) return;

			Instance.m_currentCamera = value;
			WebCamTexture = new WebCamTexture(WebCamTexture.devices[Instance.m_currentCamera].name, Screen.width, Screen.height, 30);
		}
	}

	public List<RawImage> RawImages;

	public void AddImage(RawImage rawImage)
	{
		if (RawImages == null)
		{
			RawImages = new List<RawImage>();
		}

		RawImages.Add(rawImage);
	}

	public void RemoveImage(RawImage rawImage)
	{
		if (RawImages == null)
		{
			return;
		}

		RawImages.Remove(rawImage);
	}


	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("Multiple CameraManagers in scene! Destroying self...");
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		// Default to the phones back camera
		WebCamTexture = new WebCamTexture(WebCamTexture.devices.Last().name, Screen.width, Screen.height, 30);

		ChangeToNextCamera();
	}

	private bool loadingWebcam = false;
	private void Update()
	{
		UnityEngine.Debug.Log("WebCamTexture.didUpdateThisFrame: " + WebCamTexture.didUpdateThisFrame + " loadingWebcam: " + loadingWebcam);
		if (WebCamTexture.didUpdateThisFrame && loadingWebcam)
		{
			if (WebCamTexture.width < 100)
			{
				return;
			}

			UnityEngine.Debug.Log("Playing Camera:: name: " + WebCamTexture.deviceName + " index: " + CameraIndex + " rotation: " + WebCamTexture.videoRotationAngle + " width: " + WebCamTexture.width + " height: " + WebCamTexture.height + " fps: " + WebCamTexture.requestedFPS);

			UpdateRawImages();
			loadingWebcam = false;
		}
	}

	public static float ImageWidth { get; private set; }
	public static float ImageHeight { get; private set; }
	public static Rect ImageUVRect { get; private set; }
	public static float ImageRotation { get; private set; }

	private void UpdateRawImages()
	{
		if (Instance.RawImages == null)
			return;

		ImageWidth = (float)WebCamTexture.width;
		ImageHeight = (float)WebCamTexture.height;
		ImageUVRect = WebCamTexture.videoVerticallyMirrored ? VerticalFlippedRect : DefaultRect;
		ImageRotation = -WebCamTexture.videoRotationAngle + (WebCamTexture.videoVerticallyMirrored ? 180 : 0);

		foreach (RawImage raw in Instance.RawImages)
		{
			// Set the width and height to as large as possible within its parent while maintaining the aspect ratio
			RectTransform parent = raw.rectTransform.parent.GetComponent<RectTransform>();
			float parentWidth = parent.rect.width;
			float parentHeight = parent.rect.height;

			float aspectRatio = ImageWidth / ImageHeight;
			float width = parentWidth;
			float height = parentWidth / aspectRatio;

			if (height > parentHeight)
			{
				height = parentHeight;
				width = parentHeight * aspectRatio;
			}

			raw.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			raw.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			raw.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			raw.rectTransform.localPosition = new Vector3(0, 0, 0);

			raw.texture = WebCamTexture;
			raw.rectTransform.sizeDelta = new Vector2(width, height);
			raw.rectTransform.localEulerAngles = new Vector3(0, 0, ImageRotation);
			raw.uvRect = ImageUVRect;

		}
	}

	public void Play()
	{
		WebCamTexture.Play();
		loadingWebcam = true;
	}

	public void Pause()
	{
		WebCamTexture.Pause();
		loadingWebcam = false;
	}

	public void Stop()
	{
		WebCamTexture.Stop();
		loadingWebcam = false;
	}

	public bool CheckPermissions(bool requestIfNot)
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
			{
				if (requestIfNot) Application.RequestUserAuthorization(UserAuthorization.WebCam);
				return false;
			}
		}

		return true;
	}

	public void _ChangeToNextCamera() { ChangeToNextCamera(); } // Wrapper for serializable functions

	public static void ChangeToNextCamera()
	{
		if (WebCamTexture.devices.Length <= 0)
		{
			Debug.LogError("No Webcam devices were found");
		}

		if (CameraIndex + 1 >= WebCamTexture.devices.Length)
			CameraIndex = 0;
		else
			CameraIndex++;
	}

#if UNITY_EDITOR
	[Header("Debug")]
	[SerializeField]
	private bool debug = false;

	private void OnGUI()
	{
		if (debug)
		{
			GUIStyle style = new GUIStyle() { fontSize = 50 };
			GUI.Label(new Rect(50, 10, 1000, 100), "Camera: " + WebCamTexture.deviceName, style);
			GUI.Label(new Rect(50, 70, 1000, 100), "Camera Index: " + CameraIndex, style);
			GUI.Label(new Rect(50, 130, 1000, 100), "Rotation: " + WebCamTexture.videoRotationAngle, style);

			// Make sure the button style is still the same, but with a different font size
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 50 };
			if (GUI.Button(new Rect(50, 190, 1000, 100), "Change Camera", buttonStyle))
			{
				ChangeToNextCamera();
			}
		}
	}



	private void OnValidate()
	{
		if (m_webCamTexture == null)
		{
			m_webCamTexture = new WebCamTexture(Screen.width, Screen.height, 30);
		}

		if (RawImages != null)
		{
			DrivenRectTransformTracker dt = new DrivenRectTransformTracker();
			dt.Clear();

			RawImages.ForEach(raw => {
				raw.texture = m_webCamTexture;
				dt.Add(raw, raw.rectTransform, DrivenTransformProperties.SizeDelta | DrivenTransformProperties.Anchors);
			});
		}
	}
#endif
}
