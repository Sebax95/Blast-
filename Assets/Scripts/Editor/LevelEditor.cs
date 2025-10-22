using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class LevelEditor : EditorWindow
{
    private int rows = 8;
    private int cols = 8;
    private ColorTile activeColor = ColorTile.White;
    private Color emptyColor = Color.gray;

    private Tile[,] grid;
    private Vector2 scroll;
    private float cellSize = 24f;
    private float cellPadding = 2f;
    
    private static readonly Color[] colorMap =
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.magenta,
        new Color(1f, 0.5f, 0f), //orange
        new Color(1f, 0.41f, 0.71f),//pink
        Color.white,
    };

    [MenuItem("Tools/Level Editor")]
    public static void Open()
    {
        var win = GetWindow<LevelEditor>("Level Editor");
        win.minSize = new Vector2(420, 300);
        win.InitGrid();
    }

    private void OnEnable()
    {
        if (grid == null)
            InitGrid();
    }

    private void InitGrid()
    {
        grid = new Tile[rows, cols];
        int id = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = new Tile(id++, ColorTile.White, new Vector2(c, r));
            }
        }
    }

    private void OnGUI()
    {
        DrawHeader();
        EditorGUILayout.Space(6);
        DrawGrid();
        EditorGUILayout.Space(6);
        DrawCounts();
        EditorGUILayout.Space(6);
        DrawExport();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int newRows = Mathf.Clamp(EditorGUILayout.IntField("Rows", rows), 1, 128);
        int newCols = Mathf.Clamp(EditorGUILayout.IntField("Columns", cols), 1, 128);
        cellSize = Mathf.Clamp(EditorGUILayout.FloatField("Cell Size", cellSize), 12f, 60f);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Active Color", GUILayout.Width(100));
        activeColor = (ColorTile)EditorGUILayout.EnumPopup(activeColor);
        GUILayout.Space(8);
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = GetUnityColor(activeColor);
        GUILayout.Box("", GUILayout.Width(24), GUILayout.Height(16));
        GUI.backgroundColor = prev;
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            bool sizeChanged = (newRows != rows) || (newCols != cols);
            rows = newRows;
            cols = newCols;
            if (sizeChanged)
                ResizeOrReinitGrid(rows, cols);
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clean (White)"))
        {
            ApplyToAll((t) => t.color = ColorTile.White);
        }

        if (GUILayout.Button("Fill with Active Color"))
        {
            ApplyToAll((t) => t.color = activeColor);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void ResizeOrReinitGrid(int newRows, int newCols)
    {
        if (grid == null)
        {
            InitGrid();
            return;
        }

        var newGrid = new Tile[newRows, newCols];
        int id = 0;
        for (int r = 0; r < newRows; r++)
        {
            for (int c = 0; c < newCols; c++)
            {
                if (r < grid.GetLength(0) && c < grid.GetLength(1))
                {
                    var old = grid[r, c];
                    old.id = id;
                    old.positionGrid = new Vector2(c, r);
                    newGrid[r, c] = old;
                }
                else
                {
                    newGrid[r, c] = new Tile(id, ColorTile.White, new Vector2(c, r));
                }

                id++;
            }
        }

        grid = newGrid;
    }

    private void ApplyToAll(Action<Tile> op)
    {
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            op(grid[r, c]);
        Repaint();
    }

    private void DrawGrid()
    {
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

        float gridWidth = cols * (cellSize + cellPadding) + cellPadding;
        float gridHeight = rows * (cellSize + cellPadding) + cellPadding;

        Rect viewRect = GUILayoutUtility.GetRect(gridWidth, gridHeight, GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(false));
        Rect scrollViewRect = new Rect(0, 0, gridWidth + 10, gridHeight + 10);

        scroll = GUI.BeginScrollView(viewRect, scroll, scrollViewRect);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = c * (cellSize + cellPadding) + cellPadding;
                float y = r * (cellSize + cellPadding) + cellPadding;
                Rect cell = new Rect(x, y, cellSize, cellSize);

                Color bg = GetUnityColor(grid[r, c].color);
                var prevCol = GUI.backgroundColor;
                GUI.backgroundColor = bg;

                if (GUI.Button(cell, GUIContent.none))
                {
                    grid[r, c].color = activeColor;
                }

                GUI.backgroundColor = prevCol;
                
                Handles.color = Color.black * 0.5f;
                Handles.DrawAAPolyLine(2f, new Vector3[]
                {
                    new Vector3(cell.xMin, cell.yMin),
                    new Vector3(cell.xMax, cell.yMin),
                    new Vector3(cell.xMax, cell.yMax),
                    new Vector3(cell.xMin, cell.yMax),
                    new Vector3(cell.xMin, cell.yMin)
                });
            }
        }

        GUI.EndScrollView();

        if (Event.current.isMouse && Event.current.type == EventType.MouseDrag &&
            viewRect.Contains(Event.current.mousePosition))
        {
            Vector2 local = Event.current.mousePosition + scroll;
            int c = Mathf.FloorToInt((local.x - cellPadding) / (cellSize + cellPadding));
            int r = Mathf.FloorToInt((local.y - cellPadding) / (cellSize + cellPadding));
            if (r >= 0 && r < rows && c >= 0 && c < cols)
            {
                grid[r, c].color = activeColor;
                Repaint();
            }
        }
    }

    private void DrawCounts()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Count by color", EditorStyles.boldLabel);

        var counts = Enum.GetValues(typeof(ColorTile))
            .Cast<ColorTile>()
            .ToDictionary(ct => ct, ct => 0);

        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            counts[grid[r, c].color]++;

        int total = rows * cols;
        EditorGUILayout.LabelField($"Total Tiles: {total}");

        foreach (var kv in counts)
        {
            EditorGUILayout.BeginHorizontal();
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = GetUnityColor(kv.Key);
            GUILayout.Box("", GUILayout.Width(18), GUILayout.Height(12));
            GUI.backgroundColor = prev;

            EditorGUILayout.LabelField($"{kv.Key}: {kv.Value}");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawExport()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Export the actual grid in a ScriptableObject LevelGenerator.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create a new LevelGenerator"))
        {
            CreateOrOverwriteLevelGenerator(null);
        }

        LevelGenerator targetAsset = null;
        targetAsset =
            (LevelGenerator)EditorGUILayout.ObjectField("Save in:", targetAsset, typeof(LevelGenerator), false);

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Export a file..."))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save LevelGenerator", "LevelGenerator", "asset",
                "Select a file to save the LevelGenerator to");
            if (!string.IsNullOrEmpty(path))
            {
                var asset = ScriptableObject.CreateInstance<LevelGenerator>();
                FillLevelGenerator(asset);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(asset);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateOrOverwriteLevelGenerator(LevelGenerator existing)
    {
        if (existing == null)
        {
            var asset = CreateInstance<LevelGenerator>();
            FillLevelGenerator(asset);

            const string resourcesDir = "Assets/Resources";
            const string levelsDir = resourcesDir + "/Levels";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(levelsDir))
                AssetDatabase.CreateFolder(resourcesDir, "Levels");

            string baseName = "LevelGenerator";
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{levelsDir}/{baseName}.asset");

            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
        }
        else
        {
            FillLevelGenerator(existing);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(existing);
        }
    }

    private void FillLevelGenerator(LevelGenerator asset)
    {
        var copy = new Tile[rows, cols];
        int total = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var t = grid[r, c];
                copy[r, c] = new Tile(t.id, t.color, t.positionGrid);
                total++;
            }
        }
        asset.SetGrid(copy, rows, cols);
        asset.BuildShootersFromCounts();
        EditorUtility.SetDirty(asset);
    }

    private Color GetUnityColor(ColorTile ct)
    {
        int idx = (int)ct;
        if (idx >= 0 && idx < colorMap.Length) return colorMap[idx];
        return emptyColor;
    }
}