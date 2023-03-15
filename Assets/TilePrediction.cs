using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class TilePrediction : MonoBehaviour
{
	public RectTransform RectTransform => (RectTransform)transform;
	public TMP_Text Text;
	public Image BorderImage;
	public RawImage MaskImage;
	public Color Color = Color.white;

	private void Awake()
	{
		RectTransform.anchorMin = new Vector2(0f, 1f);
		RectTransform.anchorMax = new Vector2(0f, 1f);
		RectTransform.pivot = new Vector2(0f, 1f);

		// Generate a random color for the border.
		float hue = UnityEngine.Random.Range(0f, 1f);
		Color = Color.HSVToRGB(hue, 0.7f, 1f);
		Color.a = 0.5f;
	}

	private Mask _mask;
	public Mask Mask {
		get => _mask;
		set {
			_mask = value;
			UpdateMask();
		}
	}

	[SerializeField, Range(-1, 37)] private int _predictionIndex = -1;
	public int PredictionIndex { get => _predictionIndex; }

	public void SetPredictionIndex(int index, float confidence = -1)
	{
		_predictionIndex = index;

		if (PredictionIndex == -1) Text.text = "";
		else if (confidence == -1) Text.text = TileCatalog.Catalog[PredictionIndex].ToString();
		else Text.text = $"{TileCatalog.Catalog[PredictionIndex]} ({confidence * 100:0.}%)";
	}

	public void UpdateMask()
	{
		float x1_norm = _mask.x1 / WebCameraManager.ImageWidth;
		float y1_norm = _mask.y1 / WebCameraManager.ImageHeight;
		float x2_norm = _mask.x2 / WebCameraManager.ImageWidth;
		float y2_norm = _mask.y2 / WebCameraManager.ImageHeight;

		RectTransform parent = (RectTransform)transform.parent;
		float x1 = x1_norm * parent.rect.width;
		float y1 = y1_norm * parent.rect.height;
		float x2 = x2_norm * parent.rect.width;
		float y2 = y2_norm * parent.rect.height;

		// Set the position and size of the image to match the top left and bottom right points of the mask
		RectTransform.anchoredPosition = new Vector2(x1, -y1);
		RectTransform.sizeDelta = new Vector2(x2 - x1, y2 - y1);

		MaskImage.texture = _mask.Texture;
		MaskImage.uvRect = new Rect(
			WebCameraManager.ImageUVRect.x,
			WebCameraManager.ImageUVRect.y,
			WebCameraManager.ImageUVRect.width,
			WebCameraManager.ImageUVRect.height * -1);

		MaskImage.color = Color;
		BorderImage.color = Color;
	}




#if UNITY_EDITOR

	[Header("Editor Only")]
	[SerializeField] private int _previewXPos = 0;
	[SerializeField] private int _previewYPos = 0;
	[SerializeField] private int _previewWidth = 0;
	[SerializeField] private int _previewHeight = 0;

	DrivenRectTransformTracker? dt;

	private void OnValidate()
	{
		if (!dt.HasValue)
		{
			dt = new DrivenRectTransformTracker();
			dt.Value.Clear();

			dt.Value.Add(this, RectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta | DrivenTransformProperties.Pivot);

			RectTransform.anchorMin = new Vector2(0f, 1f);
			RectTransform.anchorMax = new Vector2(0f, 1f);
			RectTransform.pivot = new Vector2(0f, 1f);
		}

		if (RectTransform.anchoredPosition.x != _previewXPos ||
			RectTransform.anchoredPosition.y != -_previewYPos || 
			RectTransform.sizeDelta.x != _previewWidth ||
			RectTransform.sizeDelta.y != _previewHeight
		) UnityEditor.EditorApplication.delayCall += () =>
		{
			// Set the position and size of the image to match the top left and bottom right points of the mask
			RectTransform.anchoredPosition = new Vector2(_previewXPos, -_previewYPos);
			RectTransform.sizeDelta = new Vector2(_previewWidth, _previewHeight);

			// Set the anchor and pivot to the top left corner of the image
			RectTransform.anchorMin = new Vector2(0f, 1f);
			RectTransform.anchorMax = new Vector2(0f, 1f);
			RectTransform.pivot = new Vector2(0f, 1f);
		};

		if (Text != null) SetPredictionIndex(_predictionIndex);
	}

#endif
}
