using UnityEngine;
using System;

public class GameplayStates
{
    #region States definition
    public static LoadingState LoadingStateState = new LoadingState();
    public static OffgameState OffgameStateState = new OffgameState();
    public static InitialState InitialStateState = new InitialState();
	public static PrepareLaunch PrepareLaunchState = new PrepareLaunch();
	public static Launch LaunchState = new Launch();
	public static EndLaunch EndLaunchState = new EndLaunch();
    public static EditMode EditModeState = new EditMode();
    #endregion

    #region LoadingState State
    public class LoadingState : FiniteStateMachine.StateBase
    {

        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            Debug.Log("GameplayStates.LoadingState.OnEnter");

            Texture2D fadeTex = new Texture2D(1, 1);
            fadeTex.SetPixel(0, 0, Color.black);
            fadeTex.Apply();

            GameCamera.Instance.Fader.FadeTexture = fadeTex;
            GameCamera.Instance.FSM.ManualUpdate = true;
            GameCamera.Instance.FSM.TimeSource = MyGame.Instance.TimeSource;
            GameCamera.Instance.FSM.State = CameraStates.OffgameCameraState;

            MyGame.Instance.Interface.State = PikiInterface.Loading;
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }

    }
    #endregion

    #region OffgameState State
    public class OffgameState : FiniteStateMachine.StateBase
    {

        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            Debug.Log("GameplayStates.OffgameState.OnEnter");
            MyGame.Instance.Interface.State = PikiInterface.SplashPage;
            /*
            if (DataContainer.Instance.playMode == DataContainer.PlayMode.None)
                MyGame.Instance.Interface.State = PikiInterface.ChooseModePage;
            else
                MyGame.Instance.Interface.State = PikiInterface.SplashPage;
            */
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }

    }
    #endregion

	#region InitialState State
	public class InitialState : FiniteStateMachine.StateBase {
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			Debug.Log ( "GameplayStates.InitialState.OnEnter" );
		    GameCamera.Instance.FSM.State = CameraStates.InitialCameraState;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
			if ( !GameCamera.Instance.Fader.IsFading ) MyGame.Instance.FSM.State = GameplayStates.PrepareLaunchState;
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region PrepareLaunch State
	public class PrepareLaunch : FiniteStateMachine.StateBase {
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			Debug.Log ( "GameplayStates.PrepareLaunch.OnEnter" );
            ObjectManager.Instance.ClearSelected();
			if ( GameCamera.Instance.FSM.State != CameraStates.InitialCameraState )
				GameCamera.Instance.FSM.State = CameraStates.InitialCameraState;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region Launch State
	public class Launch : FiniteStateMachine.StateBase {
		
		private float _timer;
		private bool _shooted;
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			Debug.Log ( "GameplayStates.Launch.OnEnter" );
			//GameCamera.Instance.FSM.State = CameraStates.StartLaunchCameraState;
			GameCamera.Instance.FSM.State = CameraStates.FlyingCameraState;
			_timer = time;
			_shooted = false;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
			if ( time - _timer > 1.15f ) {
				if ( !_shooted ) {
					MyGame.Instance.OnShoot();
					_shooted = true;
				}
			}
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region EndLaunch State
	public class EndLaunch : FiniteStateMachine.StateBase {
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			Debug.Log ( "GameplayStates.EndLaunch.OnEnter" );
            MyGame.Instance.Interface.CheckRewardInterface();
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region EditMode State
	public class EditMode : FiniteStateMachine.StateBase {
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			Debug.Log ( "GameplayStates.EditMode.OnEnter" );
			GameCamera.Instance.FSM.State = CameraStates.SideCameraState;

            if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            {
                MyGame.Instance.Interface.State = PikiInterface.IngameEditLaunchPage;
            }
            else if (DataContainer.Instance.playMode == DataContainer.PlayMode.Defence)
            {
                MyGame.Instance.Interface.State = PikiInterface.IngameEditDefencePage;
            }
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
}
