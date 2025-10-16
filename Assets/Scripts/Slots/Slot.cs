using UnityEngine;

public class Slot : BaseMonoBehaviour
{
    public bool isUsed = false;
    private Shooter _actualShooter;
    
    public void SetShooter(Shooter shooter)
    {
        _actualShooter = shooter;
        isUsed = true;
    }
    
    public void RestartSlot()
    {
        isUsed = false;
    }
}
