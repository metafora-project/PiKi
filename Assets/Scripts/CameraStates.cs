using UnityEngine;
using System;

public class CameraStates
{
    #region Static Members
    public static Vector3 orthoPosition = Configuration.OrthoCamera.INITIAL_TARGET_POS;
	public static float orthoSize = Configuration.OrthoCamera.INITIAL_SIZE;
    public static float FadeTimer;
    public static float ChangeCameraTime = 5.0f;
    #endregion

    #region States definition
    public static OffgameCamera OffgameCameraState = new OffgameCamera();
    public static InitialCamera InitialCameraState = new InitialCamera();
    public static StartLaunchCamera StartLaunchCameraState = new StartLaunchCamera();
    public static FlyingCamera FlyingCameraState = new FlyingCamera();
    public static SideCamera SideCameraState = new SideCamera();
    public static BackCamera BackCameraState = new BackCamera();
    public static TopCamera TopCameraState = new TopCamera();
    #endregion

    #region Utility Functions
    public static bool IsEditState ( FiniteStateMachine.StateBase state ) {
		return ( state == SideCameraState || state == BackCameraState || state == TopCameraState );
	}

    public static void InitializeCameras()
    {
        OffgameCameraState.source = GameObject.Find("camera_placeholder").transform;
        OffgameCameraState.target = GameObject.Find("camera_target").transform;
        InitialCameraState.target = GameObject.Find("CannonGroup").transform;
        StartLaunchCameraState.target = InitialCameraState.target;
        FlyingCameraState.target = StartLaunchCameraState.target;

        OffgameCameraState.upVector = Vector3.up;
        InitialCameraState.upVector = Vector3.up;
        StartLaunchCameraState.upVector = Vector3.up;
        FlyingCameraState.upVector = Vector3.up;

        FadeTimer = 0.75f;
    }
    #endregion

    #region OffgameCamera State
    public class OffgameCamera : FiniteStateMachine.StateBase
    {
        public Transform source;
        public Transform target;
        public Vector3 upVector;

        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            GameCamera.Instance.camera.orthographic = false;
            /*
            self.GameObject.transform.position = source.position;
            self.GameObject.transform.localRotation = source.localRotation;
            Vector3 targetOffset = new Vector3(0.72f, 1.5f, 0.0f);
            self.GameObject.transform.LookAt(target.position + targetOffset, upVector);
            */
            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }

    }
    #endregion
	
	#region InitialCamera State
	public class InitialCamera : FiniteStateMachine.StateBase {
		
		public Transform target;
		public Vector3 upVector;
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			GameCamera.Instance.camera.orthographic = false;
			
			self.GameObject.transform.position = new Vector3 ( 0.8f, 1.5f, -4.0f );
			Vector3 targetOffset = new Vector3 ( 0.0f, 1.5f, 0.0f  );
            self.GameObject.transform.LookAt(target.position + targetOffset, upVector);
            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region StartLaunchCamera State
	public class StartLaunchCamera : FiniteStateMachine.StateBase {
		
		public Transform target;
		public Vector3 upVector;
		
		private Vector3 _currenTargetPos;
		
		public void SetTarget ( Transform target )
		{
			this.target = target;
		}
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			GameCamera.Instance.camera.orthographic = false;
            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
			
			_currenTargetPos = self.GameObject.transform.position + self.GameObject.transform.forward * 100.0f;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
			_currenTargetPos = Vector3.Lerp ( _currenTargetPos, target.position, Time.deltaTime * 8.0f );
			self.GameObject.transform.LookAt ( _currenTargetPos, upVector );
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region FlyingCamera State
	public class FlyingCamera : FiniteStateMachine.StateBase {
		
		public Transform target;
		public Vector3 upVector;
		
		private float distance;
		private Vector3 sourceOffset;
		private Vector3 targetOffset;
		private Vector3 _currentTargetPos;
		private float _height;
		
		public void SetTarget ( Transform target )
		{
			this.target = target;
		}
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			GameCamera.Instance.camera.orthographic = false;
            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
			
			_height = 2.0f;
			sourceOffset = new Vector3 ( 1.0f, -1.0f, -8.0f );
			targetOffset = new Vector3 ( 1.0f, _height, 0.0f );
			
			_currentTargetPos = self.GameObject.transform.position + self.GameObject.transform.forward * 100.0f;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {		
			float minHeight = 5.5f;
			float clampedHeight = Mathf.Clamp ( target.position.y, minHeight, Mathf.Infinity );
			float distFactor = ( clampedHeight - minHeight ) * 1.1f;
			
			Vector3 targetPosition = target.position + targetOffset;
			
			targetPosition.y = Mathf.Min( targetPosition.y, _height + distFactor * 0.5f);
			
			Vector3 sourcePosition = targetPosition + sourceOffset + Vector3.back * distFactor;
			sourcePosition.y = Mathf.Clamp ( sourcePosition.y, Configuration.PerpsCamera.BOTTOM_LIMIT, Mathf.Infinity );
			self.GameObject.transform.position = Vector3.Lerp (self.GameObject.transform.position, sourcePosition, Time.deltaTime * 7.0f);

			targetPosition = Vector3.Lerp (_currentTargetPos, targetPosition, Time.deltaTime * 6.0f);
			self.GameObject.transform.LookAt ( targetPosition, upVector );
			_currentTargetPos = targetPosition;
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region SideCamera State
	public class SideCamera : FiniteStateMachine.StateBase {
		
		public Vector3 position;
		public float size;
		public Vector3 forwardVector;
		public Vector3 upVector;
		public Vector3 targetOffset;
		
		private Vector3 _currentMousePos;

        private bool _changedCamera = false;
        private float _timer = -1.0f;

        private float sideScreenSize = 0.15f;
        private float topScreenSize = 0.2f;
        private float autoScrollSpeed = 40.0f;
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            //position = Vector3.zero;

			targetOffset	= new Vector3 ( 0.0f, 0.0f, -200.0f );
			forwardVector	= new Vector3 ( 0.0f, 0.0f, 1.0f );
			upVector		= new Vector3 ( 0.0f, 1.0f, 0.0f );
			
			GameCamera.Instance.camera.orthographic = true;

			//if ( !CameraStates.IsEditState ( self.PrevState ) ) {
				position = Configuration.OrthoCamera.INITIAL_TARGET_POS;
				CameraStates.orthoPosition = position + targetOffset;
				size = Configuration.OrthoCamera.INITIAL_SIZE;
				GameCamera.Instance.camera.orthographicSize = size;
			//}
			
			self.GameObject.transform.position = CameraStates.orthoPosition;
			self.GameObject.transform.LookAt (CameraStates.orthoPosition + forwardVector * 10.0f, upVector);

            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
            _changedCamera = false;
            _timer = time;

            MyGame.Instance.OrthoCametaLocked = false;
            _currentMousePos = Input.mousePosition;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
            if (!_changedCamera)
            {
                if (time - _timer > ChangeCameraTime)
                {
                    _changedCamera = true;
                    MyGame.Instance.cameraCounter++;
                }
            }

            if (MyGame.Instance.OrthoCametaLocked)
            {
                _currentMousePos = Input.mousePosition;
            }
            else
            {

                Vector3 translationOffset = Vector3.zero;
                Vector3 sizeOffset = Vector3.zero;

                if (Input.GetMouseButton(0))
                {
                    translationOffset = Input.mousePosition - _currentMousePos;
                    _currentMousePos = Input.mousePosition;
                }
                else if (Input.GetMouseButton(1))
                {
                    sizeOffset = Input.mousePosition - _currentMousePos;
                    _currentMousePos = Input.mousePosition;
                }
                else
                {
                    _currentMousePos = Input.mousePosition;
                    if (_currentMousePos.y > Screen.height * topScreenSize && _currentMousePos.y < Screen.height - Screen.height * topScreenSize)
                    {
                        if (_currentMousePos.x > 0 && _currentMousePos.x < Screen.width * sideScreenSize)
                        {
                            float speedFactor = (Screen.width * sideScreenSize - _currentMousePos.x) / Screen.width * sideScreenSize;
                            targetOffset += Vector3.left * autoScrollSpeed * speedFactor;
                        }
                        else if (_currentMousePos.x > Screen.width - (Screen.width * sideScreenSize) && _currentMousePos.x < Screen.width)
                        {
                            float speedFactor = (Screen.width * sideScreenSize - (Screen.width - _currentMousePos.x)) / Screen.width * sideScreenSize;
                            targetOffset += Vector3.right * autoScrollSpeed * speedFactor;
                        }
                    }
                }

                targetOffset.x = Mathf.Max(targetOffset.x, Configuration.OrthoCamera.INITIAL_TARGET_POS.x);

                GameCamera.Instance.camera.orthographicSize += sizeOffset.y * 0.1f;
                GameCamera.Instance.camera.orthographicSize = Mathf.Clamp(GameCamera.Instance.camera.orthographicSize, Configuration.OrthoCamera.MIN_SIZE, Configuration.OrthoCamera.MAX_SIZE);

                position -= translationOffset * 0.004f * GameCamera.Instance.camera.orthographicSize;
                CameraStates.orthoPosition = position + targetOffset;
                self.GameObject.transform.position = CameraStates.orthoPosition;
            }
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region BackCamera State
	public class BackCamera : FiniteStateMachine.StateBase {
		
		public Vector3 position;
		public float size;
		public Vector3 forwardVector;
		public Vector3 upVector;
		public Vector3 targetOffset;

        private Vector3 _currentMousePos;

        private bool _changedCamera = false;
        private float _timer = -1.0f;
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			targetOffset = new Vector3 ( -200.0f, 0.0f, 0.0f );
			forwardVector	= new Vector3 ( 1.0f, 0.0f, 0.0f );
			upVector		= new Vector3 ( 0.0f, 1.0f, 0.0f );
			
			GameCamera.Instance.camera.orthographic = true;
			
			//if ( !CameraStates.IsEditState ( self.PrevState ) ) {
				position = Configuration.OrthoCamera.INITIAL_TARGET_POS;
				CameraStates.orthoPosition = position + targetOffset;
				size = Configuration.OrthoCamera.INITIAL_SIZE;
				GameCamera.Instance.camera.orthographicSize = size;
			//}
			
			self.GameObject.transform.position = CameraStates.orthoPosition;
			self.GameObject.transform.LookAt (CameraStates.orthoPosition + forwardVector * 10.0f, upVector);

            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
            _changedCamera = false;
            _timer = time;
            _currentMousePos = Input.mousePosition;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
            if (!_changedCamera)
            {
                if (time - _timer > ChangeCameraTime)
                {
                    _changedCamera = true;
                    MyGame.Instance.cameraCounter++;
                }
            }

			Vector3 translationOffset = Vector3.zero;
			Vector3 sizeOffset = Vector3.zero;
			
			if ( Input.GetMouseButton(0) ) {
				translationOffset = Input.mousePosition - _currentMousePos;
				_currentMousePos = Input.mousePosition;
			} if ( Input.GetMouseButton(1) ) {
				sizeOffset = Input.mousePosition - _currentMousePos;
				_currentMousePos = Input.mousePosition;
			} else {
				_currentMousePos = Input.mousePosition;
			}
			
			GameCamera.Instance.camera.orthographicSize += sizeOffset.y * 0.1f;
			GameCamera.Instance.camera.orthographicSize = Mathf.Clamp ( GameCamera.Instance.camera.orthographicSize, Configuration.OrthoCamera.MIN_SIZE, Configuration.OrthoCamera.MAX_SIZE );
			
			position -= new Vector3 ( 0.0f, translationOffset.y, 0.0f ) * 0.004f * GameCamera.Instance.camera.orthographicSize;
			CameraStates.orthoPosition = position + targetOffset;
			self.GameObject.transform.position = CameraStates.orthoPosition;
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
	#region TopCamera State
	public class TopCamera : FiniteStateMachine.StateBase {
		
		public Vector3 position;
		public float size;
		public Vector3 forwardVector;
		public Vector3 upVector;
		public Vector3 targetOffset;

        private Vector3 _currentMousePos;

        private bool _changedCamera = false;
        private float _timer = -1.0f;
		
		public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
			targetOffset	= new Vector3 ( 0.0f, 200.0f, 0.0f );
			forwardVector	= new Vector3 ( 0.0f, -1.0f, 0.0f );
			upVector		= new Vector3 ( 1.0f, 0.0f, 0.0f );
			
			GameCamera.Instance.camera.orthographic = true;
			
			//if ( !CameraStates.IsEditState ( self.PrevState ) ) {
				position = Configuration.OrthoCamera.INITIAL_TARGET_POS;
				CameraStates.orthoPosition = position + targetOffset;
				size = Configuration.OrthoCamera.INITIAL_SIZE;
				GameCamera.Instance.camera.orthographicSize = size;
			//}
			
			self.GameObject.transform.position = CameraStates.orthoPosition;
			self.GameObject.transform.LookAt (CameraStates.orthoPosition + forwardVector * 10.0f, upVector);

            GameCamera.Instance.Fader.FadeIn(CameraStates.FadeTimer);
            _changedCamera = false;
            _timer = time;
            _currentMousePos = Input.mousePosition;
        }		
		
		public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
            if (!_changedCamera)
            {
                if (time - _timer > ChangeCameraTime)
                {
                    _changedCamera = true;
                    MyGame.Instance.cameraCounter++;
                }
            }

			Vector3 translationOffset = Vector3.zero;
			Vector3 sizeOffset = Vector3.zero;
			
			if ( Input.GetMouseButton(0) ) {
				translationOffset = Input.mousePosition - _currentMousePos;
				_currentMousePos = Input.mousePosition;
			} if ( Input.GetMouseButton(1) ) {
				sizeOffset = Input.mousePosition - _currentMousePos;
				_currentMousePos = Input.mousePosition;
			} else {
				_currentMousePos = Input.mousePosition;
			}
			
			GameCamera.Instance.camera.orthographicSize += sizeOffset.y * 0.1f;
			GameCamera.Instance.camera.orthographicSize = Mathf.Clamp ( GameCamera.Instance.camera.orthographicSize, Configuration.OrthoCamera.MIN_SIZE, Configuration.OrthoCamera.MAX_SIZE );
			
			position -= new Vector3 ( translationOffset.y, 0.0f, 0.0f ) * 0.004f * GameCamera.Instance.camera.orthographicSize;
			CameraStates.orthoPosition = position + targetOffset;
			self.GameObject.transform.position = CameraStates.orthoPosition;
        }		
		
		public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
	
	}
	#endregion
	
}
