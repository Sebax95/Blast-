using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateLevels : BaseMonoBehaviour
{
    private GridTiles grid;
    private ShooterContainer _shooterContainer;
    private string levelName = "LevelGenerator";
    public static int LevelsCount = 0;
    [SerializeField] private List<LevelGenerator> levelAsset;
    public int forceLevel = 2;

    protected override void Start()
    {
        base.Start();
        grid =  FindAnyObjectByType<GridTiles>();
        _shooterContainer = FindAnyObjectByType<ShooterContainer>();
        var level = levelAsset[forceLevel == -1 ? LevelsCount : forceLevel];
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