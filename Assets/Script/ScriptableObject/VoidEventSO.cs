using UnityEngine.Events;
using UnityEngine;

[CreateAssetMenu(menuName = "Event/VoidEventSO")]
public class VoidEventSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    public void RasiedEvent()
    {
        OnEventRaised?.Invoke();
    }
}