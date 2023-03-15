using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestPhotoManager : Manager<RequestPhotoManager>
{
    public GameObject NoCameraPermissionPanel;
    public GameObject NoCameraPanel;
	public GameObject NoServerPanel;
    public GameObject GoodToGo;

    private bool cameraPermission = false;
    private bool cameraAvailable = false;
    private bool serverOpen = false;

    private void OnEnable()
    {
		cameraPermission = WebCameraManager.Instance.CheckPermissions(true);
        cameraAvailable = WebCamTexture.devices.Length > 0;
        serverOpen = WebSocketManager.CheckServerConnection(true);

		if (NoCameraPanel != null)
		{
            NoCameraPermissionPanel.SetActive(!cameraPermission);
			NoCameraPanel.SetActive(!cameraAvailable);
		}
        else NoCameraPermissionPanel.SetActive(!cameraPermission && !cameraAvailable);
		NoServerPanel.SetActive(!serverOpen);

        if (cameraPermission && cameraAvailable && serverOpen)
        {
            GoodToGo.SetActive(true);
        }
	}

    private void Update()
    {
        if (!cameraPermission)
        {
            if (WebCameraManager.Instance.CheckPermissions(false))
            {
                cameraPermission = true;
                NoCameraPermissionPanel.SetActive(NoCameraPanel != null ? false : !cameraAvailable);
            }
        }

        if (!cameraAvailable)
        {
            if (WebCamTexture.devices.Length > 0)
            {
                cameraAvailable = true;
                if (NoCameraPanel != null)
                    NoCameraPanel.SetActive(false);
                else NoCameraPermissionPanel.SetActive(!cameraPermission);
            }
        }

		if (!serverOpen)
		{
			if (WebSocketManager.CheckServerConnection(false))
			{
				serverOpen = true;
				NoServerPanel.SetActive(false);
			}
		}

        if (cameraPermission && cameraAvailable && serverOpen)
        {
            GoodToGo.SetActive(true);
        }
	}



	private void OnServerSocketOpen()
	{
		throw new NotImplementedException();
	}
}
