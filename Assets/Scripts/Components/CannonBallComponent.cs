using UnityEngine;
using System.Collections;

public class CannonBallComponent : MonoBehaviour {
	
    #region Protected Fields
	protected bool isTracing;
    protected bool isPlaying;
    protected int tracingCursor;
    protected float _initialIntensity;
    protected float _startTime;
    protected float _angleCos;
    protected ArrayList positionList;
    protected AudioSource splashSound;
    protected AudioSource bounceSound;
    protected Collision _lastCollision;
    protected TrailComponent _trailComp;
    #endregion

    #region Unity Callbacks
    void Start () {		
		isTracing = false;
		isPlaying = false;
		positionList = new ArrayList();
		tracingCursor = 0;	
		
		GameObject splash = GameObject.Find ( "splashSound" );
		splashSound = (AudioSource)(splash.GetComponent ( "AudioSource" ));
		GameObject bounce = GameObject.Find ( "bounceSound" );
		bounceSound = (AudioSource)(bounce.GetComponent ( "AudioSource" ));
        rigidbody.isKinematic = true;

        _trailComp = (TrailComponent)GetComponent("TrailComponent");
        _lastCollision = null;
	}

    void Update()
    {
        if (MyGame.Instance.IsShooting)
        {
            MyGame.Instance.Distance = Mathf.FloorToInt(rigidbody.position.x);
        }
        
        if (MyGame.Instance.IsShooting && rigidbody.position.y < Configuration.CannonBall.STOP_MINHEIGHT)
        {
            //_isStopped = true;
            _trailComp.StopTrail();
            MyGame.Instance.OnBallStop();
            rigidbody.isKinematic = true;
            splashSound.Play();
        }
	}
	
	void FixedUpdate () {
        if (rigidbody.velocity.y < 0.0f)
            if (null != _lastCollision)
                _lastCollision.collider.isTrigger = false;
	}
	#endregion
	
	#region RigidBody Callbacks
	void OnCollisionEnter(Collision collision) {
		Debug.Log ( "Collision: " + rigidbody.position.x );
		bounceSound.Play();
        if (collision.collider.name.Equals("Terrain"))
        {
            rigidbody.angularDrag = 4.0f;
            _trailComp.StopTrail();
            float distance = transform.position.x;
            float time = Time.time - _startTime;
            float v0 = (distance / time) / _angleCos;
            Debug.Log("Distance: " + distance + ", Time: " + time + ", _angleCos: " + _angleCos + ", V0: " + v0);
            MyGame.Instance.OnBallStop();
            //MyGame.Instance.Score += (int)distance * 10;
            MyGame.Instance.distance = (int)distance;
            MyGame.Instance.Interface.SetDistance(MyGame.Instance.distance);
        }

        if (collision.collider.tag.Equals("Block") || collision.collider.tag.Equals("Trampoline") || collision.collider.tag.Equals("Treasure"))
        {
            Vector3 velocity = gameObject.rigidbody.velocity;
            if (velocity.y > 2)
            {

                float vFactor = MaterialManager.Instance.GetVFactor(collision.collider.tag, velocity.y);
                float hFactor = MaterialManager.Instance.GetHFactor(collision.collider.tag, velocity.x);
                Debug.Log("vFactor: " + vFactor + ", hFactor: " + hFactor);

                velocity.Scale(new Vector3(hFactor, vFactor, 1.0f));

                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = true;
                rigidbody.position = collision.contacts[0].point + new Vector3(0.0f, 0.15f, 0.0f);
                rigidbody.isKinematic = false;

                rigidbody.AddForce(velocity * rigidbody.mass, ForceMode.Impulse);
                collision.collider.isTrigger = true;
                Debug.Log("New impulse: " + velocity * rigidbody.mass);
            }

            if (null != _lastCollision)
                _lastCollision.collider.isTrigger = false;
            _lastCollision = collision;

            if (collision.collider.tag.Equals("Treasure"))
            {
                collision.collider.gameObject.BroadcastMessage("Hitted");
            }
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Hoop"))
        {
            MyGame.Instance.OnHoopPassed();
        }
    }
	#endregion

    #region Public Members
    public void Load(Vector3 position)
    {
        rigidbody.isKinematic = true;
        transform.position = position;
        if (null != _lastCollision)
            if (null != _lastCollision.collider)
                _lastCollision.collider.isTrigger = false;
        _lastCollision = null;
    }

    public void Shoot(Vector3 direction, float intensity)
    {
        rigidbody.isKinematic = false;
        _startTime = Time.time;
        _angleCos = direction.normalized.x;
        _initialIntensity = intensity;

        /*direction = new Vector3(0.5f, 0.5f, 0.0f);
        direction.Normalize();
        _initialIntensity = 40.0f;*/
        Debug.Log("Impulse. " + direction * _initialIntensity * rigidbody.mass);
        rigidbody.AddForce(direction * _initialIntensity * rigidbody.mass, ForceMode.Impulse);

        _trailComp.StartTrail(_startTime);

        MyGame.Instance.distance = 0;
        MyGame.Instance.Interface.SetDistance(MyGame.Instance.distance);
    }

    public void Reset(Vector3 position)
    {
        rigidbody.isKinematic = true;
        transform.position = position;
        _trailComp.ResetTrail();
    }
    #endregion

    #region Protected Members
    protected void StartTracingBall()
    {
		Debug.Log ( "START TRACING" );
		positionList = new ArrayList();
		isTracing = true;
	}

    protected void StopTracingBall()
    {
		Debug.Log ( "STOP TRACING" );
		isTracing = false;
	}

    protected void UpdateTracingBall()
    {
		if ( isTracing ) {
			positionList.Add ( new BallTrace ( rigidbody.position, rigidbody.rotation, rigidbody.velocity, rigidbody.angularVelocity ) );
		}
	}

    protected void MiddleTimeline()
    {
		if ( isTracing ) StopTracingBall();
		Debug.Log ( "MiddleTimeline" );
		rigidbody.isKinematic = true;
		BallTrace currentTrace = ((BallTrace)positionList[100]);
		rigidbody.position = currentTrace.position;
		rigidbody.rotation = currentTrace.rotation;
		rigidbody.isKinematic = false;
		rigidbody.velocity = currentTrace.velocity;
		rigidbody.angularVelocity = currentTrace.angularVelocity;	
	}

    protected void StartPlayingBall()
    {
		if ( isTracing ) StopTracingBall();
		Debug.Log ( "START PLAYING" );
		rigidbody.isKinematic = true;
		tracingCursor = 0;
		isPlaying = true;
	}

    protected void StopPlayingBall()
    {
		Debug.Log ( "STOP PLAYING" );
		rigidbody.isKinematic = false;
		tracingCursor = 0;
		isPlaying = false;
	}

    protected void UpdatePlayingBall()
    {
		if ( isPlaying ) {
			if ( tracingCursor < positionList.Count ) {
				rigidbody.position	= ((BallTrace)positionList[tracingCursor]).position;
				rigidbody.rotation	= ((BallTrace)positionList[tracingCursor]).rotation;
				tracingCursor++;
			} else {
				this.StopPlayingBall ( );
			}
		}
	}
	#endregion
}
