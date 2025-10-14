using System;
using UnityEngine;

[Serializable] 
public class Tile
{
    public int id;
    public ColorTile color;
    public Vector2 positionGrid;

    public Tile(int _id, ColorTile _color, Vector2 _positionGrid)
    {
        id = _id;
        color = _color;
        positionGrid = _positionGrid;
    }
    
}

public enum ColorTile
{
    Blue,
    Red,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink,
    White
}
