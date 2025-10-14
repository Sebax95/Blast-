using System;
using System.Collections.Generic;
using UnityEngine;

public class TileView : BaseMonoBehaviour
{
    [SerializeField]
    private Sprite[] _sprites;
    private Dictionary<ColorTile, Sprite> _spritesDic;
    private SpriteRenderer _sp;

    private void Awake()
    {
        _sp = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        InitDictionary();
    }

    protected override void Start()
    {
        base.Start();
        InitDictionary();
    }

    private void InitDictionary()
    {
        _spritesDic = new Dictionary<ColorTile, Sprite>();
        _spritesDic.Add(ColorTile.Blue, _sprites[0]);
        _spritesDic.Add(ColorTile.Red, _sprites[1]);
        _spritesDic.Add(ColorTile.Green, _sprites[2]);
        _spritesDic.Add(ColorTile.Yellow, _sprites[3]);
        _spritesDic.Add(ColorTile.Purple, _sprites[4]);
        _spritesDic.Add(ColorTile.Orange, _sprites[5]);
        _spritesDic.Add(ColorTile.Pink, _sprites[6]);
        _spritesDic.Add(ColorTile.White, _sprites[7]);
    }

    public void UpdateSprite(ColorTile color)
    {
        _sp.sprite = _spritesDic[color];
    }
}
