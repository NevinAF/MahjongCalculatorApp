using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCameraManager : MonoBehaviour
{
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
			Instance.m_webCamTexture.Play();

			if (Instance.rawImage != null)
            {
                Instance.rawImage.texture = WebCamTexture;
            }
        }
    }

    private int m_currentCamera = -1;
	public static int CameraIndex
    {
        get => Instance.m_currentCamera;
        set {
            if (Instance.m_currentCamera == value) return;

            Instance.m_currentCamera = value;
			WebCamTexture = new WebCamTexture(WebCamTexture.devices[Instance.m_currentCamera].name);
        }
    }

    [SerializeField]
    private RawImage rawImage;

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
		WebCamTexture = new WebCamTexture(Screen.width, Screen.height, 30);

		ChangeToNextCamera();
    }

    private int m_lastRotation = -1;
    private void Update()
    {
        if (WebCamTexture.didUpdateThisFrame && WebCamTexture.videoRotationAngle != m_lastRotation)
        {
            rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -WebCamTexture.videoRotationAngle);
        }
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

        if (rawImage != null)
        {
            rawImage.texture = m_webCamTexture;
        }
    }
#endif
}
