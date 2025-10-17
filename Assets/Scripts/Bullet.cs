using System;
using DG.Tweening;
using UnityEngine;

public class Bullet : BaseMonoBehaviour
{
    [SerializeField] private float _speed;
    public BulletSpawner spawner;
    public static void TurnOn(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.Reset();
    }

    public static void TurnOff(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void Reset()
    {
        transform.position = Vector3.zero;
    }

    public void MoveToTarget(Transform target, Action callback)
    {
        transform.DOMove(target.position, _speed).SetEase(Ease.Flash).OnComplete(() => callback?.Invoke());
    }
}
