using System;
using UnityEngine;

public class Utilities
{
    public static Color GetColorTile(ColorTile color)
    {
        return color switch
        {
            ColorTile.Blue => Color.blue,
            ColorTile.Red => Color.red,
            ColorTile.Green => Color.green,
            ColorTile.Yellow => Color.yellow,
            ColorTile.Purple => new Color(0.5608f, 0.1020f, 0.2941f),
            ColorTile.Orange => new Color(1f, 0.5f, 0f),
            ColorTile.Pink => new Color(1f, 0.41f, 0.71f),
            ColorTile.White => Color.white,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }
} 