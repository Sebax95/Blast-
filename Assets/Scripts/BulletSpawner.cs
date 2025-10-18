using UnityEngine;

public class BulletSpawner : BaseMonoBehaviour
{
    private ObjectPool<Bullet> _bulletPool;
    [SerializeField]
    private Bullet _bulletPrefab;
    
    private void Awake()
    {
        _bulletPool = new ObjectPool<Bullet>(FactoryMethod, Bullet.TurnOn, Bullet.TurnOff, 10);
    }

    private Bullet FactoryMethod()
    {
        var bullet = Instantiate(_bulletPrefab, transform, true);
        bullet.spawner = this;
        return bullet;
    }
    
    public void ReturnBullet(Bullet bullet) => _bulletPool.ReturnObject(bullet);
    public Bullet GetBullet() => _bulletPool.GetObject();
    
}
