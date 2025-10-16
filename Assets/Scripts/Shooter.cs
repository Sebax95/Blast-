using System;
using System.Collections;
using UnityEngine;

public class Shooter : BaseMonoBehaviour, IObserver
{
    private ShooterView _view;
    public ColorTile color;
    public int quantityBullets;
    public float speedShooting;
    public Slot slotSelected;
    public TileObject Target { get; set; }
    private GridTiles _gridTiles;
    private bool _canProceed;

    private Coroutine _ShootCoroutine;

    private void OnEnable() => _gridTiles.Subscribe(this);

    private void OnDisable() => _gridTiles.Unsubscribe(this);

    private void Awake()
    {
        _gridTiles = FindAnyObjectByType<GridTiles>();
        _view = GetComponent<ShooterView>();
        _canProceed = true;
    }

    protected override void Start()
    {
        base.Start();
        _view.SetColor(color);
    }

    public void OnSelectedSlot()
    {
        _ShootCoroutine = StartCoroutine(Shoot());
    }

    public override void OnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            OnSelectedSlot();
        }
    }

    private IEnumerator Shoot()
    {
        while (quantityBullets > 0)
        {
            ChangeTarget();
            while (!_canProceed)
                yield return null;

            _gridTiles.RemoveFirstLayerAtColumn((int)Target.Tile.positionGrid.x);
            quantityBullets--;
            yield return new WaitForSeconds(speedShooting);
        }
    }

    private bool ChangeTarget()
    {
        var firstLayer = _gridTiles.GetFirstLayer();
        foreach (var tile in firstLayer)
        {
            if (tile.color == color)
            {
                Target = _gridTiles.GetTileObjectFromTile(tile);
                return true;
            }
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
}
