using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class UpdateManager : MonoBehaviour
{
    public static UpdateManager Instance;
    private List<BaseMonoBehaviour> _updates;
    [SerializeField]
    private bool _isPaused;
    
    private List<Rigidbody> _rigidbodies = new List<Rigidbody>();
    private Dictionary<Rigidbody, Vector3> _velocities = new Dictionary<Rigidbody, Vector3>();
    public static Action OnPause;
    public static Action OnUnPause;

    public bool IsPause { get => _isPaused; private set => _isPaused = value; }

    private void Awake()
    {
        #region Singleton
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        #endregion
        //Limit the framerate to 144
        Application.targetFrameRate = 144;
        
        _updates = new List<BaseMonoBehaviour>();
        _rigidbodies = FindObjectsOfType<Rigidbody>().ToList();
        foreach (var t in _rigidbodies.Where(t => t.isKinematic).ToList().Where(t => t != null))
            _rigidbodies.Remove(t);
    }

    public static void AddUpdate(BaseMonoBehaviour subscriber) => Instance._updates.Add(subscriber);
    public static void RemoveUpdate(BaseMonoBehaviour subscriber) => Instance._updates.Remove(subscriber);

    /// <summary>
    /// To pause everything
    /// </summary>
    public static void PauseUnPause()
    {
        if(Instance.IsPause)
            Instance.UnPause();
        else
            Instance.Pause();
    }

    #region Updates
    private void Update()
    {
        Profiler.BeginSample("Update Manager Update");
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.P))
            PauseUnPause();
#endif
        if (_updates == null || IsPause) return;
        for (var index = 0; index < _updates.Count; index++)
            if (!_updates[index].PauseIndividual)
                _updates[index].OnUpdate();
        Profiler.EndSample();
    }


    private void FixedUpdate()
    {
        Profiler.BeginSample("Update Manager FixedUpdate");
        if (_updates == null || IsPause) return;
        for (var index = 0; index < _updates.Count; index++)
            if(!_updates[index].PauseIndividual)
                _updates[index].OnFixedUpdate();
        Profiler.EndSample();
    }

    private void LateUpdate()
    {
        Profiler.BeginSample("Update Manager LateUpdate");
        if (_updates == null || IsPause) return;
        for (var index = 0; index < _updates.Count; index++)
            if(!_updates[index].PauseIndividual)
                _updates[index].OnLateUpdate();
        Profiler.EndSample();
    }
    

    #endregion

    #region Methods to control the Rigidbody and call events
    
    private void GetAllVelocity()
    {
        _velocities.Clear();
        foreach (var t in _rigidbodies.Where(x=> x != null))
            _velocities.Add(t, t.linearVelocity);
    }

    private void Pause()
    {
        IsPause = true;
        GetAllVelocity();
        foreach (var t in _rigidbodies)
            t.isKinematic = true;
        OnPause?.Invoke();
    }

    private void UnPause()
    {
        IsPause = false;
        foreach (var t in _rigidbodies)
        {
            t.isKinematic = false;
            t.linearVelocity = _velocities[t];
        }
        OnUnPause?.Invoke();
    }
    #endregion
}