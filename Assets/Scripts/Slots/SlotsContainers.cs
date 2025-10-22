using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SlotsContainers : BaseMonoBehaviour
{
    public List<Slot> slots;
    [SerializeField] private Slot _slotPrefab;
    [SerializeField] private float _spaceing;
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

    private void CreateSlots(int quantity = 2)
    {
        for (int i = 0; i < quantity; i++)
        {
            var obj = Instantiate(_slotPrefab, transform);
            slots.Add(obj);
            obj.index = i;
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
        if (shooter == null) return;
        _shooterSlotDic.Remove(shooter);
    }

    public void AddShooter(Shooter shooter, int index) => _shooterSlotDic.Add(shooter, index);

    private void CheckShooterBetweenColors(Shooter shooter)
    {
        if (shooter == null || slots == null || slots.Count < 3)
            return;

        if (!_shooterSlotDic.TryGetValue(shooter, out var index))
            return;

        Shooter Get(int i) =>
            (i >= 0 && i < slots.Count) ? slots[i]?.GetShooter() : null;

        bool Same(Shooter a, Shooter b, Shooter c) =>
            a != null && b != null && c != null &&
            a.color == b.color && b.color == c.color;

        bool TryWindow(int a, int b, int c, out (Shooter left, Shooter right, Shooter mid) res)
        {
            res = default;
            var A = Get(a);
            var B = Get(b);
            var C = Get(c);
            if (!Same(A, B, C)) return false;
            res = (A, C, B);
            return true;
        }

        if (TryWindow(index - 2, index - 1, index, out var t) || TryWindow(index - 1, index, index + 1, out t) ||
            TryWindow(index, index + 1, index + 2, out t)) 
            ExecuteMerge(t.left, t.right, t.mid);
        else
            CheckFullSlots();
        
    }

    private void CheckFullSlots()
    {
        bool allSlotsFull = slots != null && slots.Count > 0 && slots.All(s => s != null && s.isUsed);
        if (!allSlotsFull)
            return;

        StartCoroutine(CheckFullSlotsDelayed(1f));
    }

    private IEnumerator CheckFullSlotsDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        bool allSlotsFull = slots != null && slots.Count > 0 && slots.All(s => s != null && s.isUsed);
        if (!allSlotsFull)
            yield break;

        bool allBlocked = slots
            .Select(slot => slot?.GetShooter())
            .All(shooter => shooter != null && !shooter.canProceed);

        if (allBlocked)
            GameManager.OnGameOver?.Invoke();
    }

    private void ExecuteMerge(Shooter left, Shooter right, Shooter middle)
    {
        left.StopBullet();
        right.StopBullet();
        middle.StopBullet();
        
        var mergeAction = middle.GetComponent<MergeAction>();
        mergeAction.UpdateBullets(left.quantityBullets, right.quantityBullets);
        left.ShooterDead();
        right.ShooterDead();
        mergeAction.Merge(0.5f);
    }
}
