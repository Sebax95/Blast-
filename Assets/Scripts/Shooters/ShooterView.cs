using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ShooterView : BaseMonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _bulletsText;
    private SpriteRenderer _sp;

    private void Awake()
    {
        _sp = GetComponent<SpriteRenderer>();
    }

    public void SetColor(ColorTile color)
    {
        _sp.color = color switch
        {
            ColorTile.Blue => Color.blue,
            ColorTile.Red => Color.red,
            ColorTile.Green => Color.green,
            ColorTile.Yellow => Color.yellow,
            ColorTile.Purple => Color.magenta,
            ColorTile.Orange => new Color(1f, 0.5f, 0f),
            ColorTile.Pink => new Color(1f, 0.41f, 0.71f),
            ColorTile.White => Color.white,
            _ => _sp.color
        };
    }
    public void SetText(string text) => _bulletsText.SetText(text);

    public void Die()
    {
        transform.DOScale(Vector3.one * 1.05f, 0.2f)
            .OnComplete(() =>
                transform.DOShakeRotation(0.3f, new Vector3(50, 20,0), 10, 90, false).OnComplete(()=>Destroy(gameObject)));
    }
}
