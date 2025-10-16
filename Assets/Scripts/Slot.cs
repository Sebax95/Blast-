using UnityEngine;

public class Slot : BaseMonoBehaviour
{
    public bool isUsed = false;
    public Shooter _actualShooter;

    
    
    
    private void GetShooter(Shooter shooter)
    {
        _actualShooter = shooter;
    }
}
