using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Game : Singleton<Game>
{
    #region Private members
    private bool appIsPaused = false;
    private TimeSource timeSource = new TimeSource();
    #endregion

    #region Public properties
    public bool ApplicationIsPaused {
        get {
            return appIsPaused;
        }
    }

    public TimeSource TimeSource {
        get {
            return timeSource;
        }
        set {
            bool wasPaused = timeSource.IsPaused;

            timeSource = value;

            Time.timeScale = timeSource.TimeMultiplier;

            if (wasPaused != timeSource.IsPaused) {
                if (timeSource.IsPaused)
                    this.OnGamePaused();
                else
                    this.OnGameResumed();
            }
        }
    }
    #endregion

    #region Virtual functions
    protected virtual void OnInitializationBefore() { }
    protected virtual void OnInitializationAfter() { }
    protected virtual void OnUpdateBefore() { }
    protected virtual void OnUpdateAfter() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnGUI() { }
    public virtual void OnGamePaused() { }
    public virtual void OnGameResumed() { }
    #endregion

    #region Unity callbacks
    new void Awake()
    {
        base.Awake();

        this.OnInitializationBefore();
    }

    void Start()
    {
        timeSource.Reset();

        GameCamera.Instance.Initialize();
        this.OnInitializationAfter();
    }

    void Update()
    {
        if (timeSource != null)
            timeSource.Update();

        if (!timeSource.IsPaused)
        {
        }

        this.OnUpdateBefore();
    }

    void LateUpdate()
    {
        if (!timeSource.IsPaused)
        {
        }

        this.OnUpdateAfter();
    }

    void FixedUpdate()
    {
        this.OnFixedUpdate();
    }

    void OnApplicationPause(bool pause)
    {
    }
    #endregion
};
