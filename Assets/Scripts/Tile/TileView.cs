using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TileView : BaseMonoBehaviour
{
    [SerializeField] private Sprite[] _sprites;
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
        _spritesDic = new Dictionary<ColorTile, Sprite>
        {
            { ColorTile.Blue, _sprites[0] },
            { ColorTile.Red, _sprites[1] },
            { ColorTile.Green, _sprites[2] },
            { ColorTile.Yellow, _sprites[3] },
            { ColorTile.Purple, _sprites[4] },
            { ColorTile.Orange, _sprites[5] },
            { ColorTile.Pink, _sprites[6] },
            { ColorTile.White, _sprites[7] }
        };
    }

    public void UpdateSprite(ColorTile color) => _sp.sprite = _spritesDic[color];

    public void OrderLayer(int y) => _sp.sortingOrder = -y;

    public void DeathAnimation(Action callback) => transform.DOScale(Vector3.one * 0.2f, 0.3f).SetEase(Ease.InElastic)
        .OnComplete(() => callback?.Invoke());
}
