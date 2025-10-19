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
        while (quantityBullets > 0)
        {
            ChangeTarget();
            yield return new WaitUntil(()=> _canProceed);

            if (Target == null || Target.Tile == null)
            {
                _canProceed = false;
                continue;
            }

            SetupBullet();
       
            _view.SetText(quantityBullets.ToString());
            yield return new WaitForSeconds(speedShooting);
        }
        _view.Die();
        _slotSelected.RestartSlot();
    }
    
    private void SetupBullet()
    {
        _canProceed = false;
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

        if (Target != null && Target.Tile != null)
            _gridTiles.RemoveFirstLayerAtColumn((int)Target.Tile.positionGrid.x);
        

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
        _slotSelected = _slotsContainers.GetFirstSlotEnable();
        if(_slotSelected == null)
            return;
        _slotSelected.SetShooter(this);
        _isUsed = true;
        ShooterContainer.OnShooterSelected(this);
        OnSelectedSlot();
    }
}
