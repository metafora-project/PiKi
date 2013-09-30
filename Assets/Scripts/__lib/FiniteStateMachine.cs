using System;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    #region StateBase & ObjectBase classes
    public abstract class StateBase : FSM.State<ObjectBase> { };

    public class ObjectBase : FSM.Object<ObjectBase, StateBase> {
        protected GameObject gameObject = null;

        public GameObject GameObject {
            get {
                return gameObject;
            }
        }

        public ObjectBase(GameObject go)
        {
            gameObject = go;
        }
    };
    #endregion

    #region Void state class & static instance
    private sealed class Void : StateBase {
        public override void OnEnter(ObjectBase self, float time) { }
        public override void OnExec(ObjectBase self, float time) { }
        public override void OnExit(ObjectBase self, float time) { }
    }

    public static StateBase VoidState = new Void();
    #endregion

    #region Protected members
	protected bool manualUpdate = false;
    protected ObjectBase fsm = null;
    #endregion

    #region Public properties
    public StateBase PrevState {
        get {
            return fsm.PrevState;
        }
    }

    public StateBase State {
        get {
            return fsm.State;
        }
        set {
            fsm.State = value;
        }
    }

    public TimeSource TimeSource {
        get {
            return fsm.TimeSource;
        }
        set {
            fsm.TimeSource = value;
        }
    }
	
	public bool ManualUpdate {
		get {
			return manualUpdate;
		}
		set {
			manualUpdate = value;
		}
	}
    #endregion
	
	#region Public methods
	public void ForceUpdate()
	{
		fsm.Update();
	}
	#endregion
	
    #region Unity callbacks
    void Awake()
    {
        fsm = new ObjectBase(gameObject);
    }

    void Update()
    {
		if (false == manualUpdate)
        	fsm.Update();
    }
    #endregion
};
