using System.Collections.Generic;
using UnityEngine;

public class SlotsContainers : BaseMonoBehaviour
{
    public List<Slot> slots;
    [SerializeField]
    private Slot _slotPrefab;
    [SerializeField]
    private float _spaceing;

    protected override void Start()
    {
        base.Start();
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
        if(slots.Count <= 0)
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
}
