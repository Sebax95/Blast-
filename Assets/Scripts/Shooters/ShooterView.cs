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
        switch (color)
        {
            case ColorTile.Blue: _sp.color = Color.blue; break;
            case ColorTile.Red: _sp.color = Color.red; break;
            case ColorTile.Green: _sp.color = Color.green; break;
            case ColorTile.Yellow: _sp.color = Color.yellow; break;
            case ColorTile.Purple: _sp.color = Color.magenta; break;
            case ColorTile.Orange: _sp.color = new Color(1f, 0.5f, 0f); break;
            case ColorTile.Pink: _sp.color = new Color(1f, 0.41f, 0.71f); break;
            case ColorTile.White: _sp.color = Color.white; break;
            default:
                throw new ArgumentOutOfRangeException(nameof(color), color, null);
        }
    }
    public void SetText(string text) => _bulletsText.SetText(text);

    public void Die()
    {
        transform.DOScale(Vector3.one * 1.05f, 0.2f)
            .OnComplete(() =>
                transform.DOShakeRotation(0.3f, new Vector3(50, 20,0), 10, 90, false).OnComplete(()=>Destroy(gameObject)));
    }
}
