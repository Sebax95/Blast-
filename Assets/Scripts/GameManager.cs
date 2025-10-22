using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : BaseMonoBehaviour
{
    public static GameManager Instance;
    public static Action<int> OnGridTilesChanged;
    public static Action OnGameOver;

    
    [SerializeField]
    private GameObject WinPanel;
    private Button _restartButton; 
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    protected override void Start()
    {
        base.Start();
        WinPanel = Utilities.FindByTagInactive("Win");
    }

    private void Init()
    {
        if(!WinPanel)
            WinPanel = Utilities.FindByTagInactive("Win");
        if(!_restartButton)
            _restartButton = WinPanel.GetComponentInChildren<Button>();
        _restartButton.onClick.AddListener(RestartLevel);
        
        OnGridTilesChanged -= CheckGridTiles;
        OnGameOver -= Lose;

        OnGridTilesChanged += CheckGridTiles;
        OnGameOver += Lose;
        
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Game")
        {
            SoundManager.Instance?.StopAudio("Menu Background");
            SoundManager.PlaySound("Music Background");
        }
        else
        {
            SoundManager.Instance?.StopAudio("Music Background");
            SoundManager.PlaySound("Menu Background");
        }

        if (WinPanel) WinPanel.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Init();

    private void OnSceneUnloaded(Scene scene) => UnSubscribe();

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
    }

    private void UnSubscribe()
    {
        OnGridTilesChanged -= CheckGridTiles;
        OnGameOver -= Lose;
        // NO desuscribas sceneLoaded aquí; se maneja en OnDisable
    }
    
    private void OnDisable()
    {
        UnSubscribe();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
