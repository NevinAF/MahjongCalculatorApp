using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Slider))]
public class FormatSlider : MonoBehaviour
{
    public Slider Slider { get; private set; }

    public string Format = "{0}";
    public bool Normalize = false;

    public UnityEvent<string> OnValueChangedEvent;

	private void Awake()
	{
		Slider = GetComponent<Slider>();
	}

    private void OnEnable()
    {
        Slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        Slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        if (Normalize)
        {
            value /= Slider.maxValue;
        }

        OnValueChangedEvent.Invoke(string.Format(Format, value));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Slider == null)
        {
            Slider = GetComponent<Slider>();
        }

        if (Slider == null)
        {
            Debug.LogError("Slider is null");
			return;
		}

        if (OnValueChangedEvent != null)
            OnValueChangedEvent.Invoke(string.Format(Format, Normalize ? Slider.normalizedValue : Slider.value));
    }
#endif
}
