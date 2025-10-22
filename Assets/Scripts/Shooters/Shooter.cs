using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shooter : BaseMonoBehaviour, IObserver, IPointerClickHandler
{
    private ShooterView _view;
    public ColorTile color;
    public int quantityBullets;
    public float speedShooting;
    public Slot slotSelected;
    public TileObject Target { get; set; }
    private GridTiles _gridTiles;
    public bool canProceed;

    private SlotsContainers _slotsContainers;
    
    private Coroutine _ShootCoroutine;
    
    [SerializeField]
    private BulletSpawner _bulletSpawner;

    private bool _isUsed;
    public bool IsPickeable { get; set; }

    public bool isShooting;
    public bool HasNextShotIntent => quantityBullets > 0 && (isShooting || canProceed || Target != null);

    private void OnEnable() => _gridTiles.Subscribe(this);

    private void OnDisable() => _gridTiles.Unsubscribe(this);

    private void Awake()
    {
        _gridTiles = FindAnyObjectByType<GridTiles>();
        _view = GetComponent<ShooterView>();
        _bulletSpawner = FindAnyObjectByType<BulletSpawner>();
        canProceed = true;
        _isUsed = false;
        isShooting = false;
    }

    protected override void Start()
    {
        base.Start();
        _view.SetColor(color);
        _view.SetText(quantityBullets.ToString());
        _slotsContainers = FindAnyObjectByType<SlotsContainers>();
    }
    public void UpdateBulletsText() => _view.SetText(quantityBullets.ToString());
    private void OnSelectedSlot()
    {
        transform.DOMove(slotSelected.transform.position, .2f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            
            StartShooting();
            _view.SoundSlot();
            _slotsContainers.OnShooterAdded?.Invoke(this);
            _slotsContainers?.RequestCheckFullSlots();
        });
        transform.SetParent(slotSelected.transform);
    }
    
    public void StartShooting()
    {
        if (_ShootCoroutine != null) return;
        _ShootCoroutine = StartCoroutine(Shoot());
    }

    public void StopBullet()
    {
        if (_ShootCoroutine != null)
        {
            StopCoroutine(_ShootCoroutine);
            _ShootCoroutine = null;
        }
    }

    private IEnumerator Shoot()
    {
        while (quantityBullets > 0)
        {
            ChangeTarget();
            isShooting = false;
            yield return new WaitUntil(()=> canProceed);
            if (Target == null || Target.Tile == null)
            {
                canProceed = false;
                yield return null;
                continue;
            }

            int columnX = (int)Target.Tile.positionGrid.x;

            if (!_gridTiles.TryClaimColumn(columnX))
            {
                canProceed = false;
                yield return null;
                continue;
            }

            SetupBullet(columnX);

            UpdateBulletsText();
            yield return new WaitForSeconds(speedShooting);
        }
        ShooterDead();
    }
    
    private void SetupBullet(int columnX)
    {
        canProceed = false;
        isShooting = true;
        _slotsContainers?.RequestCheckFullSlots();
        var bullet = _bulletSpawner.GetBullet();
        bullet.transform.SetParent(transform);
        bullet.transform.localPosition = Vector3.zero;
        bullet.SetTrailColor(Utilities.GetColorTile(color));

        var hasTarget = Target != null && Target.Tile != null;

        bool shotResolved = false;

        void SafeShot(Bullet b)
        {
            if (shotResolved) return;
            shotResolved = true;
            BulletShot(b, columnX, hasTarget);
        }
        
        DOVirtual.DelayedCall(1.0f, () => SafeShot(bullet));

        if (hasTarget)
            bullet.MoveToTarget(Target.transform, SafeShot);
        else
            SafeShot(bullet);

        bullet.EnableTrail(true);
    }
    private void BulletShot(Bullet bullet, int columnX, bool hadTargetAtFireTime)
    {
        quantityBullets = Mathf.Max(0, quantityBullets - 1);

        if (hadTargetAtFireTime)
            _gridTiles.RemoveFirstLayerAtColumn(columnX);
        else
            _gridTiles.ReleaseColumn(columnX);
        isShooting = false;
        _bulletSpawner.ReturnBullet(bullet);
        UpdateBulletsText();
        canProceed = true;
        _slotsContainers?.RequestCheckFullSlots();

        if (quantityBullets == 0)
        {
            StopBullet();
            ShooterDead();
        }
    }
    private bool ChangeTarget()
    {
        var firstLayer = _gridTiles.GetFirstLayer();
        var tile = firstLayer.FirstOrDefault(t => t.color == color);

        if (tile != null)
        {
            var targetObj = _gridTiles.GetTileObjectFromTile(tile);
            if (targetObj != null)
            {
                Target = targetObj;
                canProceed = true;
                _slotsContainers?.RequestCheckFullSlots();
                return true;
            }
        }

        Target = null;
        canProceed = false;
        
        _slotsContainers?.RequestCheckFullSlots();

        return false;
    }

    public void ShooterDead()
    {
        _view.Die();
        slotSelected.DieShooter();
    }


    public void OnNotify(ObserverMessage message)
    {
        if(message != ObserverMessage.UpdateRow)
            return;
        if (canProceed) 
            return;
        canProceed = ChangeTarget();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isUsed || !IsPickeable)
            return;
        slotSelected = _slotsContainers.GetFirstSlotEnable();
        if(slotSelected == null)
            return;
        slotSelected.SetShooter(this);
        _isUsed = true;
        ShooterContainer.OnShooterSelected(this);
        OnSelectedSlot();
    }
}
