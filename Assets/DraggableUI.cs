using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(EventTrigger))]
public class DraggableUI : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform)transform;
    public EventTrigger EventTrigger { get; private set; }
    private EventTrigger.Entry[] entries;

    private void Awake()
    {
        EventTrigger = GetComponent<EventTrigger>();
    }

    private void OnEnable()
    {
        if (entries == null)
        {
            entries = new EventTrigger.Entry[3];
        }

        entries[0] = AddTrigger(OnBeginDrag, EventTriggerType.BeginDrag);
        entries[1] = AddTrigger(OnDrag, EventTriggerType.Drag);
        entries[2] = AddTrigger(OnEndDrag, EventTriggerType.EndDrag);
    }

    private void OnDisable()
    {
        foreach (EventTrigger.Entry entry in entries)
        {
            EventTrigger.triggers.Remove(entry);
        }
    }

    private void OnBeginDrag(PointerEventData eventData)
    {
        RectTransform.SetAsLastSibling();
    }

    private void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta;
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition = new Vector2(Mathf.Round(RectTransform.anchoredPosition.x), Mathf.Round(RectTransform.anchoredPosition.y));
    }

    private EventTrigger.Entry AddTrigger(UnityEngine.Events.UnityAction<PointerEventData> call, EventTriggerType type)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(data => call((PointerEventData)data));
        EventTrigger.triggers.Add(entry);
        return entry;
    }
}
