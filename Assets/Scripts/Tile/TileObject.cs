using System;
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

    public void UpdatePosition() =>
        transform.position = grid.GetTilePosition((int)Tile.positionGrid.x, (int)Tile.positionGrid.y);
}