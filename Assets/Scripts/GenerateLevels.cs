using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateLevels : BaseMonoBehaviour
{
    private GridTiles grid;
    private ShooterContainer _shooterContainer;
    private string levelName = "LevelGenerator";
    public static int LevelsCount;
    [SerializeField] private List<LevelGenerator> levelAsset;

    protected override void Start()
    {
        base.Start();
        grid = FindObjectOfType<GridTiles>();
        _shooterContainer = FindObjectOfType<ShooterContainer>();
        LevelsCount = 1;
        var level = levelAsset[LevelsCount];
        if (level == null)
            return;

        grid.BuildFromLevel(level);
        _shooterContainer.CreateShootersFromLevel(level);
    }

    private LevelGenerator LoadFromResources(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return Resources.Load<LevelGenerator>($"Levels/{name}");
    }
}