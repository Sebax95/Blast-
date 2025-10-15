using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Shooter : BaseMonoBehaviour, IObserver
{
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
        _canProceed = true;
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

    public void OnNotify()
    {
        if (_canProceed) return;
        if(ChangeTarget())
            _canProceed = true;
    }
}
