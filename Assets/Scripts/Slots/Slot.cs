using UnityEngine;

public class Slot : BaseMonoBehaviour
{
    public int index;
    public bool isUsed = false;
    private Shooter _actualShooter;
    private SlotsContainers _slotsContainers;
    
    private void Awake() => _slotsContainers = FindAnyObjectByType<SlotsContainers>();

    public void SetShooter(Shooter shooter)
    {
        _actualShooter = shooter;
        isUsed = true;
    }
    
    public void RestartSlot() => isUsed = false;

    public void DieShooter()
    {
        _slotsContainers.RemoveShooter(_actualShooter);
        _actualShooter = null;
        index = -1;
    }
}
