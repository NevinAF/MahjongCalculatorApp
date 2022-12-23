using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CustomToggleGroup : MonoBehaviour
{
    public List<Toggle> Toggles = new List<Toggle>();

    private int _currentSelectedIndex = -1;
	public int SelectedIndex { get; set; } = -1;

	public UnityEvent<int> OnChangedEvent;

    private void Awake()
    {
		for (int i = 0; i < Toggles.Count; i++)
        {
            int index = i;
			Toggles[index].onValueChanged.AddListener(value => OnValueChanged(index));
            Toggles[index].group = null;
        }
    }

    private void LateUpdate()
    {
        if (_currentSelectedIndex != SelectedIndex)
        {
			UnityEngine.Debug.Log(gameObject.name + ", SelectedIndex: " + SelectedIndex);
			_currentSelectedIndex = SelectedIndex;

            for (int i = 0; i < Toggles.Count; i++)
            {
                Toggles[i].isOn = i == SelectedIndex;
            }
        }
    }


	private void OnValueChanged(int index)
	{
        if (SelectedIndex == index)
        {
            if (!Toggles[index].isOn)
            {
                Toggles[index].isOn = true;
            }
			return;
		}

        if (!Toggles[index].isOn)
        {
			return;
		}

        SelectedIndex = index;

        for (int i = 0; i < Toggles.Count; i++)
        {
            if (Toggles[i] != Toggles[index] && Toggles[i].isOn)
            {
                Toggles[i].isOn = false;
            }
        }

        OnChangedEvent.Invoke(index);
	}


}
