using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBS.Core
{
    public class FSM2
    {
        public class Object<T, K>
            where T : Object<T, K>
        {
            public delegate void Function(T self, float time);

            #region Protected members
            protected TimeSource timeSource = null;
            protected Dictionary<K, State<T, K>> states = new Dictionary<K,State<T,K>>();
            protected State<T, K> state = null;
            protected State<T, K> prevState = null;
            #endregion

            #region Ctors
            public Object()
            {
                timeSource = Game.Instance.TimeSource; // TimeManager.Instance.MasterSource;
            }

            public Object(TimeSource source)
            {
                timeSource = source;
            }
            #endregion

            #region Public properties
            public K PrevState
            {
                get
                {
                    return prevState.key;
                }
            }

            public K State
            {
                get
                {
                    return state.key;
                }

                set
                {
                    prevState = state;

                    if (prevState != null)
                        prevState.onExit(this as T, timeSource.TotalTime);

                    State<T, K> nextState;
                    if (states.TryGetValue(value, out nextState))
                    {
                        state = nextState;
                        state.onEnter(this as T, timeSource.TotalTime);
                    }
                    else
                    {
                        state = null;
                    }
                }
            }

            public TimeSource TimeSource
            {
                get
                {
                    return timeSource;
                }
                set
                {
                    timeSource = value;
                }
            }
            #endregion

            #region Public methods
            public void AddState(K key, Function onEnter, Function onExec, Function onExit)
            {
                State<T, K> newState = new State<T, K>();

                newState.key = key;
                newState.onEnter = onEnter;
                newState.onExec = onExec;
                newState.onExit = onExit;

                states.Add(key, newState);
            }

            public void Update()
            {
                if (null == state) return;

                state.onExec(this as T, timeSource.TotalTime);
            }
            #endregion
        }

        public class State<T, K>
            where T : Object<T, K>
        {
            public K key;
            public Object<T, K>.Function onEnter;
            public Object<T, K>.Function onExec;
            public Object<T, K>.Function onExit;
        }
    }
}
