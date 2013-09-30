using UnityEngine;
using System.Collections;

public class CannonComponent : MonoBehaviour
{
    #region Constants
    public const string POWER = "INITIAL_VELOCITY";
    public const string ANGLE = "ANGLE";
    #endregion

    #region Public Components
    public GameObject ball;
    #endregion

    #region Protected Components
    protected float _angle;
    protected float _currentAngle;
    protected float _intensity;
    protected float _backShotTimer;
    protected Transform _cannon;
    protected Transform _ballTr;
    protected GameObject _smokeEmitter;
    protected GameObject _cannonObj;
    protected PikiObject _pObject;
    protected CannonBallComponent _cannonBallComp;
    #endregion

    #region Getters/Setters
    public Vector3 StartBallPosition
    {
        get { return _ballTr.position; }
    }

    public float CannonAngle
    {
        get { return _angle; }
        set
        {
            _angle = value;
            DataContainer.Instance.CannonAngle = _angle;
        }
    }

    public float CannonPower
    {
        get { return _intensity; }
        set
        {
            _intensity = value;
            DataContainer.Instance.CannonPower = _intensity;
        }
    }
    #endregion

	#region Unity Callbacks
    void Awake()
    {
        _cannon = transform.FindChild("cannon");
        _ballTr = _cannon;
        _angle = Configuration.Cannon.MIN_ANGLE;
        _currentAngle = _angle;
        _intensity = Configuration.Cannon.MIN_POWER;
        _backShotTimer = -1.0f;
    }

    void Start()
    {
        _pObject = new PikiObject("1", XMPPBridge.CANNON);
        _cannonObj = GameObject.Find("CannonGroup/cannon");
        _smokeEmitter = GameObject.Find("CannonGroup/cannon/CannonSmoke");
        _cannonBallComp = (CannonBallComponent)ball.GetComponent("CannonBallComponent");
    }

    void Update()
    {
        if (_angle != _currentAngle)
        {
            _cannon.Rotate(new Vector3(1.0f, 0.0f, 0.0f), _currentAngle);
            _currentAngle = _angle;
            _cannon.Rotate(new Vector3(1.0f, 0.0f, 0.0f), -_currentAngle);
        }
    }
	#endregion
	
	#region Public Functions
	public void Load ( )
	{
        _cannonBallComp.Load(_ballTr.position);
        CameraStates.FlyingCameraState.SetTarget(ball.transform);
        MyGame.Instance.FSM.State = GameplayStates.LaunchState;
	}
	
	public void Shoot ( )
	{
		((ParticleEmitter)(_smokeEmitter.GetComponent("ParticleEmitter"))).Emit(4);
		StartBackshot ( );
        _cannonBallComp.Shoot(_ballTr.forward, _intensity);
		
		((AudioSource)(_cannonObj.GetComponent ( "AudioSource" ))).Play();

        _pObject.ClearProperties();
        _pObject.SetPropertiesFromState();
        XMPPBridge.Instance.SendCannonShotMessage(_pObject);
        MyGame.Instance.PrevCannonAngle = DataContainer.Instance.CannonAngle;
        MyGame.Instance.PrevCannonPower = DataContainer.Instance.CannonPower;
	}
	
	public void DestroyBall ( )
	{
        _cannonBallComp.Reset(_ballTr.position);
	}

	public void StartBackshot()
	{
		_backShotTimer = Time.time;
	}
	
	public void UpdateBackshot()
	{
		_cannon.transform.Translate ( new Vector3 ( 0.0f, 0.0f, 0.0f ) );
		if ( _backShotTimer > 0.0f ) {
			float currentTime = Time.time;
			float factor = ( currentTime - _backShotTimer ) / 0.15f;
			if ( factor >= 1.0f ) {
				factor = 1.0f;
				_backShotTimer = -1.0f;
			}
			_cannon.transform.Translate ( new Vector3 ( 0.0f, 0.0f, -Mathf.Sin ( factor * Mathf.PI * 2.0f ) * 0.08f ) );
		}
	}
	#endregion
}
