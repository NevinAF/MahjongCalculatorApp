using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "RectTransformKeyframe", menuName = "RectTransformKeyframe", order = 0)]
public class ScriptedRectTransformKeyframe : ScriptableObject
{
	public RectTransformKeyframe Keyframe;

	public void ApplyTo(RectTransform rectTransform)
	{
		Keyframe.ApplyTo(rectTransform);
	}

	public void CopyFrom(RectTransform rectTransform)
	{
		Keyframe.CopyFrom(rectTransform);
	}
}

#if UNITY_EDITOR
// Create a custom editor for the component.
[CustomEditor(typeof(ScriptedRectTransformKeyframe))]
public class RectTransformKeyframeEditor : Editor
{
	private ScriptedRectTransformKeyframe rectTransformKeyframe;

	private void OnEnable()
	{
		rectTransformKeyframe = (ScriptedRectTransformKeyframe)target;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Apply to RectTransform"))
		{
			// Get the currently selected game object
			GameObject selectedGameObject = Selection.activeGameObject;
			if (selectedGameObject == null)
			{
				Debug.LogWarning("No game object selected");
				return;
			}

			if (selectedGameObject.transform is not RectTransform rectTransform)
			{
				Debug.LogWarning("Selected game object does not have a RectTransform");
				return;
			}

			rectTransformKeyframe.ApplyTo(selectedGameObject.transform as RectTransform);
		}

		if (GUILayout.Button("Copy from RectTransform"))
		{
			// Get the currently selected game object
			GameObject selectedGameObject = Selection.activeGameObject;
			if (selectedGameObject == null)
			{
				Debug.LogWarning("No game object selected");
				return;
			}

			if (selectedGameObject.transform is not RectTransform rectTransform)
			{
				Debug.LogWarning("Selected game object does not have a RectTransform");
				return;
			}

			rectTransformKeyframe.CopyFrom(selectedGameObject.transform as RectTransform);
		}
	}
}
#endif

[System.Serializable]
public struct RectTransformKeyframe
{
	public Vector2 anchoredPosition;
	public Vector2 anchorMin;
	public Vector2 anchorMax;
	public Vector2 sizeDelta;
	public Vector2 pivot;

	public void ApplyTo(RectTransform rectTransform)
	{
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.sizeDelta = sizeDelta;
		rectTransform.pivot = pivot;
	}

	public void CopyFrom(RectTransform rectTransform)
	{
		anchoredPosition = rectTransform.anchoredPosition;
		anchorMin = rectTransform.anchorMin;
		anchorMax = rectTransform.anchorMax;
		sizeDelta = rectTransform.sizeDelta;
		pivot = rectTransform.pivot;
	}

	public RectTransformKeyframe(RectTransform rectTransform)
	{
		anchoredPosition = rectTransform.anchoredPosition;
		anchorMin = rectTransform.anchorMin;
		anchorMax = rectTransform.anchorMax;
		sizeDelta = rectTransform.sizeDelta;
		pivot = rectTransform.pivot;
	}

	public static RectTransformKeyframe Lerp(RectTransformKeyframe a, RectTransformKeyframe b, float t)
	{
		return new RectTransformKeyframe
		{
			anchoredPosition = Vector2.Lerp(a.anchoredPosition, b.anchoredPosition, t),
			anchorMin = Vector2.Lerp(a.anchorMin, b.anchorMin, t),
			anchorMax = Vector2.Lerp(a.anchorMax, b.anchorMax, t),
			sizeDelta = Vector2.Lerp(a.sizeDelta, b.sizeDelta, t),
			pivot = Vector2.Lerp(a.pivot, b.pivot, t),
		};
	}
}