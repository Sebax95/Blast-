using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelGenerator", menuName = "Level/LevelGenerator", order = 1)]
public class LevelGenerator : ScriptableObject
{
    [Min(1)] public int rows = 1;
    [Min(1)] public int cols = 1;
    
    [SerializeField] private List<Tile> tilesLinear = new();
    [SerializeField, HideInInspector] private int totalTiles;

    [SerializeField] public List<ShooterConfig> shooters = new();

    public int TotalTiles => Mathf.Max(totalTiles, rows * cols);

    public Tile GetTile(int r, int c)
    {
        if (!IsInBounds(r, c)) return null;
        int idx = r * cols + c;
        return idx >= 0 && idx < tilesLinear.Count ? tilesLinear[idx] : null;
    }

    public void SetTile(int r, int c, Tile tile)
    {
        if (!IsInBounds(r, c)) return;
        int idx = r * cols + c;
        EnsureSize(rows, cols);
        tilesLinear[idx] = tile;
        RecomputeTotals();
    }
    public bool IsInBounds(int r, int c) => r >= 0 && r < rows && c >= 0 && c < cols;
    
    public IEnumerable<(int r, int c, Tile tile)> Enumerate()
    {
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int idx = r * cols + c;
            if (idx >= 0 && idx < tilesLinear.Count)
                yield return (r, c, tilesLinear[idx]);
        }
    }
    
    public Dictionary<ColorTile, int> GetColorCounts()
    {
        var counts = new Dictionary<ColorTile, int>();
        foreach (ColorTile ct in Enum.GetValues(typeof(ColorTile)))
            counts[ct] = 0;

        foreach (var t in tilesLinear)
        {
            if (t != null) counts[t.color]++;
        }

        return counts;
    }
    
    public void SetGrid(Tile[,] source, int sourceRows, int sourceCols)
    {
        rows = Mathf.Max(1, sourceRows);
        cols = Mathf.Max(1, sourceCols);
        tilesLinear = new List<Tile>(rows * cols);

        int id = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var s = source[r, c];
                // Copia defensiva del Tile
                var copy = new Tile(id++, s.color, s.positionGrid);
                tilesLinear.Add(copy);
            }
        }

        RecomputeTotals();
        BuildShootersFromCounts();
    }

    public Tile[,] To2DArray()
    {
        var arr = new Tile[rows, cols];
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            int idx = r * cols + c;
            arr[r, c] = idx < tilesLinear.Count ? tilesLinear[idx] : null;
        }

        return arr;
    }
    public void BuildShootersFromCounts()
    {
        var counts = GetColorCounts();
        shooters = new List<ShooterConfig>();
        foreach (var kv in counts)
        {
            int packs = Mathf.Min(20, kv.Value / 10);
            for (int i = 0; i < packs; i++)
                shooters.Add(new ShooterConfig { color = kv.Key, bullets = 10 });
            int remainder = kv.Value % 10;
            if (remainder > 0 && packs < 20)
                shooters.Add(new ShooterConfig { color = kv.Key, bullets = remainder });
        }
    }
    private void EnsureSize(int targetRows, int targetCols)
    {
        int target = Mathf.Max(1, targetRows) * Mathf.Max(1, targetCols);
        if (tilesLinear == null) tilesLinear = new List<Tile>(target);
        if (tilesLinear.Count == target) return;
        
        var newList = new List<Tile>(target);
        int id = 0;
        for (int r = 0; r < targetRows; r++)
        {
            for (int c = 0; c < targetCols; c++)
            {
                Tile t = null;
                if (r < rows && c < cols)
                {
                    int oldIdx = r * cols + c;
                    if (oldIdx >= 0 && oldIdx < tilesLinear.Count)
                        t = tilesLinear[oldIdx];
                }

                if (t == null)
                    t = new Tile(id, ColorTile.White, new Vector2(c, r));
                t.id = id;
                t.positionGrid = new Vector2(c, r);
                newList.Add(t);
                id++;
            }
        }

        tilesLinear = newList;
        rows = targetRows;
        cols = targetCols;
        RecomputeTotals();
    }

    private void RecomputeTotals() => totalTiles = Mathf.Max(0, tilesLinear?.Count ?? 0);
    
    private void OnValidate()
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);
        EnsureSize(rows, cols);
        RecomputeTotals();
    }
}

[Serializable]
public class ShooterConfig
{
    public ColorTile color;
    public int bullets;
}