using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class TilePrediction : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform)transform;
    public Image Image { get; private set; }

    private void Awake()
    {
        Image = GetComponent<Image>();
    }

    [SerializeField] private Mask _mask;
    public Mask Mask {
        get => _mask;
        set {
            _mask = value;
            UpdateMask();
        }

    }

	private void UpdateMask()
	{
        // Set the position and size of the image to match the top left and bottom right points of the mask
        RectTransform.anchoredPosition = new Vector2(_mask.x1, -_mask.y1);
        RectTransform.sizeDelta = new Vector2(_mask.Width, _mask.Height);

        Image.sprite = Sprite.Create(_mask.Texture, new Rect(0, 0, _mask.Width, _mask.Height), new Vector2(0f, 0f));
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
	}

#endif
}
