using System;
using System.Collections.Generic;

public class FSM {
    public class Object <T, S>
        where T : Object<T, S>
        where S : State<T>
    {
        #region Protected members
        protected S state = null;
        protected S prevState = null;
        protected TimeSource timeSource = null;
        #endregion

        #region Ctors
        public Object()
        {
            timeSource = Game.Instance.TimeSource;
        }

        public Object(TimeSource source)
        {
            timeSource = source;
        }
        #endregion

        #region Public properties
        public S PrevState {
            get {
                return prevState;
            }
        }

        public S State {
            get {
                return state;
            }
            set {
                prevState = state;
                state = value;

                if (prevState != null)
                    prevState.OnExit(this as T, timeSource.TotalTime);

                if (state != null)
                    state.OnEnter(this as T, timeSource.TotalTime);
            }
        }

        public TimeSource TimeSource {
            get {
                return timeSource;
            }
            set {
                timeSource = value;
            }
        }
        #endregion

        #region Public methods
        public void Update()
        {
            if (null == state) return;

            state.OnExec(this as T, timeSource.TotalTime);
        }
        #endregion
    };

    public abstract class State<T>
    {
        #region Virtual methods
        public virtual void OnEnter(T self, float time) { }
        public virtual void OnExec(T self, float time) { }
        public virtual void OnExit(T self, float time) { }
        #endregion
    };
};
