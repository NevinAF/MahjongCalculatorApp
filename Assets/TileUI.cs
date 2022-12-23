using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(EventTrigger))]
public class TileUI : MonoBehaviour
{
	public RectTransform RectTransform => (RectTransform)transform;
	[SerializeField] private TileCatalogEntry _tileData;
	public TileCatalogEntry TileData {
		get => _tileData;
		set {
			if (_tileData == value) return;
			_tileData = value;
			RefreshTileData();
		}
	}

	public Canvas Canvas { get; private set; }
	public EventTrigger EventTrigger { get; private set; }

	public Image TileImage;
	public TMP_Text TileNameText;

	public Image InnerImage;
	public Color Inner_SelectedColor = Color.white;
	public Color Inner_UnselectedColor = Color.white;
	public Image OuterImage;
	public Color Outer_SelectedColor = Color.white;
	public Color Outer_UnselectedColor = Color.white;

	private bool _selected;
	public bool Selected {
		get => _selected;
		set {
			if (_selected == value) return;
			_selected = value;

			if (InnerImage != null)
				InnerImage.color = _selected ? Inner_SelectedColor : Inner_UnselectedColor;
			if (OuterImage != null)
				OuterImage.color = _selected ? Outer_SelectedColor : Outer_UnselectedColor;
		}
	}
	public bool Interactable = false;
	public bool focusable = false;

	private void Awake()
	{
		Canvas = GetComponent<Canvas>();
		EventTrigger = GetComponent<EventTrigger>();

		var pointerEnter = new EventTrigger.Entry();
		pointerEnter.eventID = EventTriggerType.PointerEnter;
		pointerEnter.callback.AddListener((data) => { OnPointerEnter((BaseEventData)data); });
		EventTrigger.triggers.Add(pointerEnter);

		var pointerExit = new EventTrigger.Entry();
		pointerExit.eventID = EventTriggerType.PointerExit;
		pointerExit.callback.AddListener((data) => { OnPointerExit((BaseEventData)data); });
		EventTrigger.triggers.Add(pointerExit);

		var pointerClick = new EventTrigger.Entry();
		pointerClick.eventID = EventTriggerType.PointerClick;
		pointerClick.callback.AddListener((data) => { OnPointerClick((BaseEventData)data); });
		EventTrigger.triggers.Add(pointerClick);
	}

	public void OnPointerClick(BaseEventData data)
	{
		if (Interactable)
			Selected = !Selected;
	}

	public void OnPointerExit(BaseEventData data)
	{
		if (focusable)
			Focused = false;
	}

	public void OnPointerEnter(BaseEventData data)
	{
		if (focusable)
			Focused = true;
	}

	private void RefreshTileData()
	{
		if (TileImage == null || TileNameText == null || TileData == null) return;
		// UnityEngine.Debug.Log("Refreshing tile data");
		TileImage.sprite = TileData.Sprite;
		TileNameText.text = TileData.ShortName.ToUpper();
	}

	private bool _focused;
	public bool Focused {
		get => _focused;
		set {
			if (_focused == value) return;
			_focused = value;
			if (_focused)
			{
				transform.localScale = Vector3.one * 1.1f;
				if (Canvas != null)
				{
					Canvas.overrideSorting = true;
					Canvas.sortingOrder = 1;
				}
			} else {
				transform.localScale = Vector3.one;
				if (Canvas != null)
				{
					Canvas.overrideSorting = false;
					Canvas.sortingOrder = 0;
				}
			}
		}
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (TileData != null && TileImage != null && TileNameText != null)
			RefreshTileData();
	}
#endif
}