using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Base class for the New Update System
/// </summary>
public class BaseMonoBehaviour : MonoBehaviour
{
    protected bool isPaused = false;
    
    /// <summary>
    /// If you want pause an specific object, you can use this method
    /// </summary>
    public bool PauseIndividual { get => isPaused; set => isPaused = value; }
    
    
    /// <summary>
    /// Normal Start method but with the subscribe to the event
    /// </summary>
    protected virtual void Start() => UpdateManager.AddUpdate(this);
    
    /// <summary>
    /// New Update method
    /// </summary>
    public virtual void OnUpdate() { }
    /// <summary>
    /// New FixedUpdate method
    /// </summary>
    public virtual void OnFixedUpdate() { }
    /// <summary>
    /// New LateUpdate method
    /// </summary>
    public virtual void OnLateUpdate() { }

    /// <summary>
    /// OnDestroy method with the unsubscribe to the event
    /// </summary>
    protected virtual void OnDestroy() => UpdateManager.RemoveUpdate(this);
    
}

