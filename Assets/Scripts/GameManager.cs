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
    
    private GameObject _winPanel;
    private GameObject _losePanel;
    private GameObject _thanksPanel;
    private Button _restartButtonWin; 
    private Button _restartButtonLose; 
    private Button _quitButton; 
    
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
        _winPanel = Utilities.FindByTagInactive("Win");
    }

    private void Init()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {

            if (!_winPanel)
                _winPanel = Utilities.FindByTagInactive("Win");
            if (!_losePanel)
                _losePanel = Utilities.FindByTagInactive("Lose");
            if (!_thanksPanel)
                _thanksPanel = Utilities.FindByTagInactive("Thanks");
            if (!_restartButtonWin)
                _restartButtonWin = _winPanel.GetComponentInChildren<Button>();
            if (!_restartButtonLose)
                _restartButtonLose = _losePanel.GetComponentInChildren<Button>();
            if (!_quitButton)
                _quitButton = _thanksPanel.GetComponentInChildren<Button>();

            _restartButtonWin.onClick.AddListener(RestartLevel);
            _restartButtonLose.onClick.AddListener(RestartLevel);
            _quitButton.onClick.AddListener(Quit);

            _restartButtonWin.interactable = false;


            _winPanel.SetActive(false);
            _losePanel.SetActive(false);
            _thanksPanel.SetActive(false);

            OnGridTilesChanged -= CheckGridTiles;
            OnGameOver -= Lose;

            OnGridTilesChanged += CheckGridTiles;
            OnGameOver += Lose;
        }

        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Game")
        {
            SoundManager.Instance?.StopAudio("Menu Background");
            SoundManager.PlaySound(GenerateLevels.LevelsCount % 2 == 0 ? "Music Background" : "Music Background 2");
        }
        else
        {
            SoundManager.Instance?.StopAudio("Music Background");
            SoundManager.PlaySound("Menu Background");
        }

        if (_winPanel) _winPanel.SetActive(false);
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
        if(GenerateLevels.LevelsCount == 20)
        {
            Thanks();
            return;
        }
        _winPanel.SetActive(true);
        Vector3 initialScale = _winPanel.transform.localScale;
        _winPanel.transform.localScale = Vector3.zero;
        SoundManager.PlaySound("Win Claps");
        _winPanel.transform.DOScale(initialScale, 1f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            GenerateLevels.LevelsCount++;
            _restartButtonWin.interactable = true;
        });
        
    }
    
    private void Lose()
    {
        SoundManager.Instance.StopAudio("Music Background");
        SoundManager.PlaySound("Lose");
        _losePanel.SetActive(true);
        Vector3 initialScale = _losePanel.transform.localScale;
        _losePanel.transform.localScale = Vector3.zero;
        _losePanel.transform.DOScale(initialScale, 1f).SetEase(Ease.OutBack);
    }

    private void Thanks()
    {
        _thanksPanel.SetActive(true);
        Vector3 initialScale = _thanksPanel.transform.localScale;
        _thanksPanel.transform.localScale = Vector3.zero;
        _thanksPanel.transform.DOScale(initialScale, 1f).SetEase(Ease.OutBack);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UnSubscribe()
    {
        OnGridTilesChanged -= CheckGridTiles;
        OnGameOver -= Lose;
    }
    
    private void OnDisable()
    {
        UnSubscribe();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
