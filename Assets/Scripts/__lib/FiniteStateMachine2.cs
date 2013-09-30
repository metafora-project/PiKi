using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

public class FiniteStateMachine2 : MonoBehaviour
{
    public enum UpdateFunction
    {
        Update = 0,
        LateUpdate,
        FixedUpdate
    }

    #region Public classes
    public class FSMObject2 : FSM2.Object<FSMObject2, int>
    {
        public GameObject go;

        public FSMObject2(GameObject _go)
        {
            go = _go;
        }
    }

    [Serializable]
    public class StateType
    {
        public int id;
        public string onEnterMessage;
        public string onExecMessage;
        public string onExitMessage;

        public void onEnter(FSMObject2 fsmObject, float time)
        {
            fsmObject.go.SendMessage(onEnterMessage, time, SendMessageOptions.RequireReceiver);
        }

        public void onExec(FSMObject2 fsmObject, float time)
        {
            fsmObject.go.SendMessage(onExecMessage, time, SendMessageOptions.RequireReceiver);
        }

        public void onExit(FSMObject2 fsmObject, float time)
        {
            fsmObject.go.SendMessage(onExitMessage, time, SendMessageOptions.RequireReceiver);
        }
    }
    #endregion

    #region Public members
    public bool manualUpdate = false;
    public UpdateFunction updateFunction = UpdateFunction.Update;
    public StateType[] states;
    public int startState;
    #endregion

    #region Protected members
    protected FSMObject2 fsmObject = null;
    #endregion

    #region Public properties
    public int PrevState
    {
        get
        {
            return fsmObject.PrevState;
        }
    }

    public int State
    {
        get
        {
            return fsmObject.State;
        }
        set
        {
            fsmObject.State = value;
        }
    }

    public TimeSource TimeSource
    {
        get
        {
            return fsmObject.TimeSource;
        }
        set
        {
            fsmObject.TimeSource = value;
        }
    }
    #endregion

    #region Public methods
    public void ForceUpdate()
    {
        fsmObject.Update();
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {       
        fsmObject = new FSMObject2(gameObject);
        foreach (StateType state in states)
            fsmObject.AddState(state.id, state.onEnter, state.onExec, state.onExit);
        fsmObject.State = startState;
    }

    void Update()
    {
        if (manualUpdate)
            return;

        if (UpdateFunction.Update == updateFunction)
            fsmObject.Update();
    }

    void LateUpdate()
    {
        if (manualUpdate)
            return;

        if (UpdateFunction.LateUpdate == updateFunction)
            fsmObject.Update();
    }

    void FixedUpdate()
    {
        if (manualUpdate)
            return;

        if (UpdateFunction.FixedUpdate == updateFunction)
            fsmObject.Update();
    }
    #endregion
}
