using System;
using UnityEngine;

public class GameCamera : Singleton<GameCamera> {
    protected FiniteStateMachine fsm;
    protected CameraFader fader;

    public CameraFader Fader {
        get {
            return fader;
        }
    }

    public FiniteStateMachine FSM {
        get {
            return fsm;
        }
    }

    new void Awake()
    {
        base.Awake();

        fsm = null;
        fader = null;
    }

    public void Initialize()
    {
        fsm = gameObject.AddComponent<FiniteStateMachine>();
        fader = gameObject.AddComponent<CameraFader>();
    }
};
