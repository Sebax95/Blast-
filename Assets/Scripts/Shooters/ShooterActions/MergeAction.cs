using System;
using UnityEngine;

public class MergeAction : BaseMonoBehaviour, IObserver
{
    private SlotsContainers _slotsContainers;
    private void Awake()
    {
        _slotsContainers = FindAnyObjectByType<SlotsContainers>();
    }

    private void OnEnable()
    {
        _slotsContainers.Subscribe(this);
    }
    private void OnDisable()
    {
        _slotsContainers.Unsubscribe(this);
    }


    public void OnNotify(ObserverMessage message)
    {
        if (message == ObserverMessage.MergeColor)
        {
            Debug.Log("3 colores!");
        }
    }
}
