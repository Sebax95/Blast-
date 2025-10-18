using System;
using DG.Tweening;
using UnityEngine;

public class Bullet : BaseMonoBehaviour
{
    [SerializeField] private float _speed;
    public BulletSpawner spawner;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    public static void TurnOn(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.Reset();
    }

    public static void TurnOff(Bullet bullet)
    {
        bullet.transform.SetParent(bullet.spawner.transform);
        bullet.gameObject.SetActive(false);
        bullet.EnableTrail(false);
    }

    private void Reset()
    {
        transform.position = Vector3.zero;
    }

    public void MoveToTarget(Transform target, Action<Bullet> callback)
    {
        var inverseVelocity = 1f / _speed;
        transform.DOMove(target.position, 0.2f * inverseVelocity).SetEase(Ease.Flash).OnComplete(() => callback?.Invoke(this));
    }
    
    public void SetTrailColor(Color color) => _trailRenderer.startColor = color;
    
    public void EnableTrail(bool enable) => _trailRenderer.enabled = enable;
}
