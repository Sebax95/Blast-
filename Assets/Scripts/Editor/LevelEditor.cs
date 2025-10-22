using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO; // agregado

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

    private bool isPainting = false;
    private int paintButton = 0; 

    
    private LevelGenerator loadFromAsset;

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
        EditorGUILayout.Space(6);
        DrawImport();
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
        Rect contentRect = new Rect(0, 0, gridWidth, gridHeight);

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        scroll = GUI.BeginScrollView(viewRect, scroll, contentRect);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = c * (cellSize + cellPadding) + cellPadding;
                float y = r * (cellSize + cellPadding) + cellPadding;
                Rect cell = new Rect(x, y, cellSize, cellSize);

                var prevCol = GUI.backgroundColor;
                GUI.backgroundColor = GetUnityColor(grid[r, c].color);
                GUI.Box(cell, GUIContent.none);
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
        
        var e = Event.current;
        if (e.isMouse)
        {
            Vector2 local = e.mousePosition + scroll;
            
            if (contentRect.Contains(local))
            {
                int c = Mathf.FloorToInt((local.x - cellPadding) / (cellSize + cellPadding));
                int r = Mathf.FloorToInt((local.y - cellPadding) / (cellSize + cellPadding));
                bool inside = r >= 0 && r < rows && c >= 0 && c < cols;

                switch (e.GetTypeForControl(controlId))
                {
                    case EventType.MouseDown:
                        if (e.button == paintButton)
                        {
                            GUIUtility.hotControl = controlId;
                            isPainting = true;
                            if (inside)
                            {
                                grid[r, c].color = activeColor;
                                Repaint();
                            }
                            e.Use();
                        }
                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl == controlId && isPainting && e.button == paintButton)
                        {
                            if (inside)
                            {
                                grid[r, c].color = activeColor;
                                Repaint();
                            }
                            e.Use();
                        }
                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == controlId && e.button == paintButton)
                        {
                            if (isPainting && inside)
                            {
                                grid[r, c].color = activeColor;
                                Repaint();
                            }
                            isPainting = false;
                            GUIUtility.hotControl = 0;
                            e.Use();
                        }
                        break;
                }
            }
            else
            {
                if (e.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
                {
                    isPainting = false;
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
            }
        }

        GUI.EndScrollView();
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

            string baseName = "Level ";
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
            int srcRow = (rows - 1) - r;
            for (int c = 0; c < cols; c++)
            {
                var t = grid[srcRow, c];
                copy[r, c] = new Tile(t.id, t.color, new Vector2(c, r));
                total++;
            }
        }
        asset.SetGrid(copy, rows, cols);
        asset.BuildShootersFromCounts();
        EditorUtility.SetDirty(asset);
    }
    
    private void DrawImport()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Importar (JSON o ScriptableObject)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Carga una grilla en este editor.", MessageType.None);
        
        loadFromAsset = (LevelGenerator)EditorGUILayout.ObjectField("LevelGenerator:", loadFromAsset, typeof(LevelGenerator), false);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cargar desde ScriptableObject"))
        {
            if (loadFromAsset == null)
            {
                EditorUtility.DisplayDialog("Sin asset", "Asigna un LevelGenerator válido.", "Ok");
            }
            else
            {
                LoadFromLevelGenerator(loadFromAsset);
            }
        }
        
        if (GUILayout.Button("Cargar desde JSON..."))
        {
            string path = EditorUtility.OpenFilePanel("Selecciona JSON de nivel", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    LoadFromJson(json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error leyendo JSON: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", "No se pudo leer el archivo JSON.", "Ok");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
    
    private void LoadFromLevelGenerator(LevelGenerator level)
    {
        if (level == null) return;

        var data = level.To2DArray();
        rows = level.rows;
        cols = level.cols;
        ResizeOrReinitGrid(rows, cols);

        for (int r = 0; r < rows; r++)
        {
            int dstRow = (rows - 1) - r; 
            for (int c = 0; c < cols; c++)
            {
                var s = data[r, c];
                grid[dstRow, c] = new Tile(s.id, s.color, new Vector2(c, dstRow));
            }
        }
        Repaint();
    }
    
    [Serializable]
    private class JsonTileSimple
    {
        public int id;
        public int color; // enum como int
        public int x;
        public int y;
    }
    [Serializable]
    private class JsonGridWrapper
    {
        public int rows;
        public int cols;
        public JsonTileSimple[] tiles;
    }

    private void LoadFromJson(string json)
    {
        try
        {
            var colorsJagged = JsonUtility.FromJson<WrapperIntArray>(WrapJsonArray(json));
            if (colorsJagged != null && colorsJagged.values != null && colorsJagged.values.Length > 0)
            {
                int rCount = colorsJagged.values.Length;
                int cCount = colorsJagged.values[0].values.Length;
                rows = rCount;
                cols = cCount;
                ResizeOrReinitGrid(rows, cols);
                for (int r = 0; r < rows; r++)
                {
                    int dstRow = (rows - 1) - r;
                    var rowVals = colorsJagged.values[r].values;
                    for (int c = 0; c < cols; c++)
                    {
                        var ct = (ColorTile)Mathf.Clamp(rowVals[c], 0, colorMap.Length - 1);
                        grid[dstRow, c] = new Tile(dstRow * cols + c, ct, new Vector2(c, dstRow));
                    }
                }
                Repaint();
                return;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            var wrapper = JsonUtility.FromJson<JsonGridWrapper>(json);
            if (wrapper != null && wrapper.tiles != null && wrapper.tiles.Length > 0)
            {
                rows = wrapper.rows;
                cols = wrapper.cols;
                ResizeOrReinitGrid(rows, cols);
                foreach (var t in wrapper.tiles)
                {
                    int r = Mathf.Clamp(t.y, 0, rows - 1);
                    int c = Mathf.Clamp(t.x, 0, cols - 1);
                    int dstRow = (rows - 1) - r;
                    var ct = (ColorTile)Mathf.Clamp(t.color, 0, colorMap.Length - 1);
                    grid[dstRow, c] = new Tile(t.id, ct, new Vector2(c, dstRow));
                }
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (grid[r,c] == null)
                            grid[r, c] = new Tile(r * cols + c, ColorTile.White, new Vector2(c, r));
                Repaint();
                return;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            var tilesJagged = JsonUtility.FromJson<WrapperTileArray>(WrapJsonArray(json));
            if (tilesJagged != null && tilesJagged.values != null && tilesJagged.values.Length > 0)
            {
                int rCount = tilesJagged.values.Length;
                int cCount = tilesJagged.values[0].values.Length;
                rows = rCount;
                cols = cCount;
                ResizeOrReinitGrid(rows, cols);
                for (int r = 0; r < rows; r++)
                {
                    int dstRow = (rows - 1) - r;
                    var rowVals = tilesJagged.values[r].values;
                    for (int c = 0; c < cols; c++)
                    {
                        var jt = rowVals[c];
                        var ct = (ColorTile)Mathf.Clamp(jt.color, 0, colorMap.Length - 1);
                        grid[dstRow, c] = new Tile(jt.id, ct, new Vector2(c, dstRow));
                    }
                }
                Repaint();
                return;
            }
        }
        catch
        {
            // ignored
        }

        EditorUtility.DisplayDialog("Unsupported format",
            "The JSON could not be interpreted. Use a matrix of colors or an object with rows, cols, and tiles.", "Ok");
    }
    
    [Serializable]
    private class IntArray { public int[] values; }
    [Serializable]
    private class WrapperIntArray { public IntArray[] values; }
    [Serializable]
    private class TileArray { public JsonTileSimple[] values; }
    [Serializable]
    private class WrapperTileArray { public TileArray[] values; }

    private string WrapJsonArray(string json)
    {
        string trimmed = json.TrimStart();
        if (trimmed.StartsWith("{")) return json;
        return $"{{\"values\":{ConvertRows(json)}}}";
    }

    private string ConvertRows(string json)
    {
        string inner = json.Trim();
        if (inner.StartsWith("[") && inner.EndsWith("]"))
            inner = inner.Substring(1, inner.Length - 2);
        inner = inner.Replace("],[", "]},{\"values\":[");
        return $"[{{\"values\":[{inner}]}}]";
    }
    
    private Color GetUnityColor(ColorTile ct)
    {
        int idx = (int)ct;
        if (idx >= 0 && idx < colorMap.Length) return colorMap[idx];
        return emptyColor;
    }
}