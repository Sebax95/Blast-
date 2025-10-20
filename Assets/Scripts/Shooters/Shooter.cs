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
    private bool _canProceed;

    private SlotsContainers _slotsContainers;
    
    private Coroutine _ShootCoroutine;
    
    [SerializeField]
    private BulletSpawner _bulletSpawner;

    private bool _isUsed;
    public bool IsPickeable { get; set; }

    private void OnEnable() => _gridTiles.Subscribe(this);

    private void OnDisable() => _gridTiles.Unsubscribe(this);

    private void Awake()
    {
        _gridTiles = FindAnyObjectByType<GridTiles>();
        _view = GetComponent<ShooterView>();
        _bulletSpawner = FindAnyObjectByType<BulletSpawner>();
        _canProceed = true;
        _isUsed = false;
    }

    protected override void Start()
    {
        base.Start();
        _view.SetColor(color);
        _view.SetText(quantityBullets.ToString());
        _slotsContainers = FindAnyObjectByType<SlotsContainers>();
    }

    private void OnSelectedSlot()
    {
        transform.DOMove(slotSelected.transform.position, .2f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            _ShootCoroutine = StartCoroutine(Shoot());
        });
        transform.SetParent(slotSelected.transform);
    }
    

    private IEnumerator Shoot()
    {
        while (quantityBullets > 0)
        {
            ChangeTarget();
            yield return new WaitUntil(()=> _canProceed);

            if (Target == null || Target.Tile == null)
            {
                _canProceed = false;
                yield return null;
                continue;
            }

            int columnX = (int)Target.Tile.positionGrid.x;

            if (!_gridTiles.TryClaimColumn(columnX))
            {
                _canProceed = false;
                yield return null;
                continue;
            }

            SetupBullet(columnX);
            
            _view.SetText(quantityBullets.ToString());
            yield return new WaitForSeconds(speedShooting);
        }
        _view.Die();
        slotSelected.RestartSlot();
    }
    
    private void SetupBullet(int columnX)
    {
        _canProceed = false;
        var bullet = _bulletSpawner.GetBullet();
        bullet.transform.SetParent(transform);
        bullet.transform.localPosition = Vector3.zero;
        bullet.SetTrailColor(Utilities.GetColorTile(color));
        
        var hasTarget = Target != null && Target.Tile != null;

        bool called = false;
        void OnShot(Bullet b)
        {
            if (called) return;
            called = true;
            BulletShot(b, columnX, hasTarget);
        }
        DOVirtual.DelayedCall(1.0f, () =>
        {
            if (called) return;
            called = true;
            BulletShot(bullet, columnX, hasTarget);
        });

        if (hasTarget) 
            bullet.MoveToTarget(Target.transform, OnShot);
        else
            OnShot(bullet);
        
        bullet.EnableTrail(true);
    }
    private void BulletShot(Bullet bullet, int columnX, bool hadTargetAtFireTime)
    {
        quantityBullets--;

        if (hadTargetAtFireTime)
            _gridTiles.RemoveFirstLayerAtColumn(columnX);
        else
            _gridTiles.ReleaseColumn(columnX);

        slotSelected.DieShooter();
        _bulletSpawner.ReturnBullet(bullet);
        _canProceed = true;
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
                _canProceed = true;
                return true;
            }
        }

        Target = null;
        _canProceed = false;
        return false;
    }

    public void OnNotify(ObserverMessage message)
    {
        if(message != ObserverMessage.UpdateRow)
            return;
        if (_canProceed) 
            return;
        _canProceed = ChangeTarget();
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
