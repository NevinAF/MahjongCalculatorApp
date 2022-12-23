using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class TileGroupUI : UIBehaviour, ILayoutGroup
{
	public RectTransform RectTransform => (RectTransform)transform;
    [NonSerialized] public List<TileUI> GroupTiles = new List<TileUI>();

	protected override void Awake()
	{
		GroupTiles.Clear();

		foreach (Transform child in transform)
		{
			var tile = child.GetComponent<TileUI>();
			if (tile != null) GroupTiles.Add(tile);
		}
	}

	private void Update()
	{
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		if (GroupTiles.Count <= 0) return;
		if (GroupTiles.Count == 1) {
			GroupTiles[0].RectTransform.anchoredPosition = Vector2.zero;
			return;
		}

		// Get the total width of the children.
		float totalWidth = 0;
		foreach (TileUI tile in GroupTiles)
		{
			totalWidth += tile.RectTransform.rect.width;
		}


		float padding = (GroupTiles.Count <= 1) ? 0 : (RectTransform.rect.width - totalWidth) / (GroupTiles.Count - 1);
		float x;
		if (padding > 0)
		{
			padding = 0;
			x = -totalWidth / 2f;
		}
		else x = -RectTransform.rect.width / 2f;

		foreach (TileUI tile in GroupTiles)
		{
			var half = tile.RectTransform.rect.width / 2f;
			x += half;
			tile.RectTransform.anchoredPosition = new Vector2(x, 0);
			x += half + padding;
		}
	}

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateLayout();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        UpdateLayout();
    }

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
        UpdateLayout();
	}


	public void SetLayoutHorizontal()
	{
		GroupTiles.Clear();

        foreach (Transform child in transform)
        {
			var tile = child.GetComponent<TileUI>();
            if (tile != null) GroupTiles.Add(tile);
        }

		DrivenRectTransformTracker dt = new DrivenRectTransformTracker();
		dt.Clear();

		UpdateLayout();
		foreach (TileUI tile in GroupTiles)
		{
			dt.Add(this, tile.RectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot);
			tile.RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			tile.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			tile.RectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
	}

	public void SetLayoutVertical()
	{
	}

	public void ClearTiles()
	{
		while (GroupTiles.Count > 0)
		{
			var tile = GroupTiles[0];

			GroupTiles.RemoveAt(0);
			if (tile != null)
				Destroy(tile.gameObject);
		}
	}

#if UNITY_EDITOR
	// private DrivenRectTransformTracker? dt;

	// protected override void OnValidate()
	// {
    //     base.OnValidate();
	// }


#endif
}