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
    private Slot _slotSelected;
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
        transform.DOMove(_slotSelected.transform.position, .2f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            _ShootCoroutine = StartCoroutine(Shoot());
        });
        transform.SetParent(_slotSelected.transform);
    }
    

    private IEnumerator Shoot()
    {
        while (quantityBullets >= 0)
        {
            ChangeTarget();
            while (!_canProceed)
                yield return new WaitForEndOfFrame();
            if(Target.Tile.color != color)
                yield break;
            SetupBullet();
            
            _view.SetText(quantityBullets.ToString());
            _gridTiles.RemoveFirstLayerAtColumn((int)Target.Tile.positionGrid.x);
            
            yield return new WaitForSeconds(speedShooting);
        }
        _view.Die();
        _slotSelected.RestartSlot();
    }

    private void SetupBullet()
    {
        var bullet = _bulletSpawner.GetBullet();
        bullet.transform.SetParent(transform);
        bullet.transform.localPosition = Vector3.zero;
        bullet.SetTrailColor(Utilities.GetColorTile(color));
        bullet.MoveToTarget(Target.transform, BulletShot);
        bullet.EnableTrail(true);
    }
    
    private void BulletShot(Bullet bullet)
    {
        quantityBullets--;
        _bulletSpawner.ReturnBullet(bullet);
    }

    private bool ChangeTarget()
    {
        var firstLayer = _gridTiles.GetFirstLayer();
        var listSelected = firstLayer.Where(tile => tile.color == color && !tile.isSelected && !tile.isBusy);
        foreach (var tile in listSelected)
        {
            tile.isBusy = true;

            Target = _gridTiles.GetTileObjectFromTile(tile);
            Target.Tile.isSelected = true;
            _canProceed = true;
            return true;
        }
        _canProceed = false;
        return Target != null;
    }

    public void OnNotify(ObserverMessage message)
    {
        if(message != ObserverMessage.UpdateRow)
            return;
        if (_canProceed) return;
        if(ChangeTarget())
            _canProceed = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isUsed || !IsPickeable)
            return;
        _slotSelected = _slotsContainers.GetFirstSlotEnable();
        if(_slotSelected == null)
            return;
        _slotSelected.SetShooter(this);
        _isUsed = true;
        ShooterContainer.OnShooterSelected(this);
        OnSelectedSlot();
    }
}
