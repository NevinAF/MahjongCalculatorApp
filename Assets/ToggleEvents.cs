using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class ToggleEvents : MonoBehaviour
{
    private Toggle _toggle;
    public UnityEvent OnToggledOnEvent;
    public UnityEvent OnToggledOffEvent;
    public UnityEvent<bool> OnChangedEventInverse;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        if (value)
        {
            OnToggledOnEvent.Invoke();
        }
        else
        {
            OnToggledOffEvent.Invoke();
        }

        OnChangedEventInverse.Invoke(!value);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (_toggle == null)
            _toggle = GetComponent<Toggle>();

        if (_toggle == null)
            return;

        _toggle.onValueChanged.RemoveListener(OnValueChanged);
		_toggle.onValueChanged.AddListener(OnValueChanged);
	}
}
