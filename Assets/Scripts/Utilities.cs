using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


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

    public static List<T> RandomizeList<T>(List<T> _list)
    {
        if (_list == null)
            throw new ArgumentNullException(nameof(_list));
        for (int i = _list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_list[i], _list[j]) = (_list[j], _list[i]);
        }
        return _list;
    }
} 