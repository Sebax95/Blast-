using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotsContainers : BaseMonoBehaviour, IObservable
{
    public List<Slot> slots;
    [SerializeField] private Slot _slotPrefab;
    [SerializeField] private float _spaceing;
    private List<IObserver> _observers = new();
    public Action<Shooter> OnShooterAdded;
    /// <summary>
    /// shooter -> index slot
    /// </summary>
    private Dictionary<Shooter,int> _shooterSlotDic = new();

    protected override void Start()
    {
        base.Start();
        OnShooterAdded += CheckShooterBetweenColors;
        CreateSlots(5);
    }

    public void CreateSlots(int quantity = 2)
    {
        for (int i = 0; i < quantity; i++)
        {
            var obj = Instantiate(_slotPrefab, transform);
            slots.Add(obj);
        }

        ReorderSlots();
    }

    private void ReorderSlots()
    {
        if (slots.Count <= 0)
            return;
        var totalWidth = (slots.Count - 1) * _spaceing;
        float startX = -totalWidth * .5f;
        for (var i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null)
                continue;

            slot.transform.localPosition = new Vector3(startX + i * _spaceing, 0, 0);
        }
    }

    public Slot GetFirstSlotEnable() => slots.FirstOrDefault(s => !s.isUsed);

    public void RemoveShooter(Shooter shooter)
    {
        if(!_shooterSlotDic.ContainsKey(shooter))
            return;
        foreach (var s in _shooterSlotDic.Where(s => s.Key == shooter))
            _shooterSlotDic.Remove(s.Key);
    }
    private void CheckShooterBetweenColors(Shooter shooter)
    {
        if(shooter == null)
            return;
        
        _shooterSlotDic[shooter] = shooter.slotSelected.index;
        
        int streak = 0;
        ColorTile? currentColor = null;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null || !slot.isUsed)
            {
                streak = 0;
                currentColor = null;
                continue;
            }
            
            var shooterAtIndex = _shooterSlotDic.FirstOrDefault(p => p.Value == slot.index).Key;

            if (shooterAtIndex == null)
            {
                streak = 0;
                currentColor = null;
                continue;
            }

            if (currentColor.HasValue && shooterAtIndex.color == currentColor.Value)
                streak++;
            else
            {
                currentColor = shooterAtIndex.color;
                streak = 1;
            }

            if (streak >= 3)
            {
                OnNotify(ObserverMessage.MergeColor);
                break;
            }
            else
            {
                Debug.Log("nothing");
            }
        }
    }


    #region Observers

    public void Subscribe(IObserver observer)
    {
        if(!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        if(_observers.Contains(observer))
            _observers.Remove(observer);
    }

    public void OnNotify(ObserverMessage message)
    {
        foreach (var item in _observers)
            item.OnNotify(ObserverMessage.MergeColor); 
    }

    #endregion
}
