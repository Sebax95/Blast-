using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : BaseMonoBehaviour
{
    public static GameManager Instance;
    public static Action<int> OnGridTilesChanged;
    public static Action OnGameOver;

    
    [SerializeField]
    private GameObject WinPanel;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    protected override void Start()
    {
        base.Start();
        WinPanel = Utilities.FindByTagInactive("Win");
    }

    private void Init()
    {
        if(!WinPanel)
            WinPanel = Utilities.FindByTagInactive("Win");
        OnGridTilesChanged += CheckGridTiles;
        OnGameOver += Lose;
        SoundManager.PlaySound("Menu Background");
        WinPanel.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    public void Play() => SceneManager.LoadScene("Game");

    public void Quit() => Application.Quit();
    
    private void CheckGridTiles(int obj)
    {
        if (obj < 1)
            Win();
    }
    
    private void Win()
    {
        WinPanel.SetActive(true);
        Vector3 initialScale = WinPanel.transform.localScale;
        WinPanel.transform.localScale = Vector3.zero;
        SoundManager.PlaySound("Win Claps");
        WinPanel.transform.DOScale(initialScale, 1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            GenerateLevels.LevelsCount++;
        });
        
    }
    
    private void Lose()
    {
        SoundManager.Instance.StopAudio("Music Background");
        SoundManager.PlaySound("Lose");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Init();
    }
    
    private void OnDisable()
    {
        OnGridTilesChanged -= CheckGridTiles;
        OnGameOver -= Lose;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
