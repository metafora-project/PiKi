using System;
using UnityEngine;

public class TimeSource
{
    #region Protected members
    protected float lastPauseTime = 0.0f;
    protected float totalPauseTime = 0.0f;
    protected float realTime = 0.0f;
    protected float totalTime = 0.0f;
    protected float deltaTime = 0.0f;
    protected float timeMultiplier = 1.0f;
    protected int pauseCounter = 0;
    #endregion

    #region Public properties
    public float TotalTime {
        get {
            return totalTime;
        }
    }

    public float DeltaTime {
        get {
            return this.IsPaused ? 0.0f : deltaTime;
        }
    }

    public float TimeMultiplier {
        get {
            return this.IsPaused ? 0.0f : timeMultiplier;
        }
        set {
            timeMultiplier = Math.Max(0.0f, value);

            if (this.IsMaster)
                Time.timeScale = timeMultiplier;
        }
    }

    public bool IsPaused {
        get {
            return pauseCounter > 0;
        }
    }
    
    public bool IsMaster {
        get {
            return this == Game.Instance.TimeSource;
        }
    }
    #endregion

    #region Virtual methods
    protected virtual void OnPause() { }
    protected virtual void OnResume() { }
    #endregion

    #region Public methods
	public void Pause()
    {
        if (0 == pauseCounter) {
            lastPauseTime = Time.realtimeSinceStartup;

            if (this.IsMaster) {
                Time.timeScale = 0.0f;

                Game.Instance.OnGamePaused();
            }

            this.OnPause();
        }

        ++pauseCounter;
    }

    public void Resume()
    {
        DebugUtils.Assert(pauseCounter >= 1);

        --pauseCounter;
        if (0 == pauseCounter) {
            totalPauseTime += (Time.realtimeSinceStartup - lastPauseTime);

            if (this.IsMaster) {
                Time.timeScale = timeMultiplier;

                Game.Instance.OnGameResumed();
            }

            this.OnResume();
        }
    }

    public void Reset()
    {
        lastPauseTime  = Time.realtimeSinceStartup;
        totalPauseTime = 0.0f;
        realTime       = Time.realtimeSinceStartup;
        totalTime      = 0.0f;
        deltaTime      = 0.0f;
        timeMultiplier = 1.0f;
    }
    
    public void Update()
    {
        if (this.IsPaused)
            return;

        float prevRealTime = realTime;

        realTime  = Time.realtimeSinceStartup - totalPauseTime;
        deltaTime = (realTime - prevRealTime) * timeMultiplier;
        totalTime += deltaTime;
    }
    #endregion
};
