using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeAction : BaseMonoBehaviour
{
    private Shooter _shooter;
    
    private void Awake() => _shooter = GetComponent<Shooter>();

    public void Merge(float delaySeconds) => StartCoroutine(MergeAfterDelay(delaySeconds));

    private IEnumerator MergeAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SoundManager.PlaySound("Merge");
        _shooter.StartShooting();
    }

    public void UpdateBullets(int bulletsA, int bulletsB)
    {
        var ab = bulletsA + bulletsB;
        _shooter.quantityBullets += ab;
        _shooter.UpdateBulletsText();
    }
}
