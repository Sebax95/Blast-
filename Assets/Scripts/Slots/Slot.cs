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
        _slotsContainers.AddShooter(shooter, index);
        isUsed = true;
    }
    public Shooter GetShooter() => _actualShooter;

    public void DieShooter()
    {
        isUsed = false;
        _slotsContainers.RemoveShooter(_actualShooter);
        _actualShooter = null;
    }
}
