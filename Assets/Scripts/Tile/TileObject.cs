using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class TileObject : BaseMonoBehaviour
{
    public GridTiles grid;
    private TileView _tileView;
    public Tile Tile { get; set; }

    private void Awake()
    {
        _tileView = GetComponent<TileView>();
    }

    #region Pooling

    public static void TurnOn(TileObject obj)
    {
        obj.gameObject.SetActive(true);
        obj.Reset();
    }

    public static void TurnOff(TileObject obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void Reset()
    {
        transform.position = Vector3.zero;
    }
    #endregion
    
    public void UpdateSprite() => _tileView.UpdateSprite(Tile.color);
    public void OrderLayer() => _tileView.OrderLayer((int)Tile.positionGrid.y);

    public void UpdatePosition(TweenCallback callback = null) =>
        transform.DOMove(grid.GetTilePosition(Tile.positionGrid.x, Tile.positionGrid.y), 0.1f).SetEase(Ease.Flash).OnComplete(callback);
}