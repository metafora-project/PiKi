using UnityEngine;
using System.Collections;

public class TrailComponent : MonoBehaviour
{
    #region Public Fields
    public float stepTime;
    public GameObject TrailUnit;
    #endregion

    #region Protected Fields
    protected int _updateSteps;
    protected int _updateLoop;
    protected float _timeStep;
    protected float _startTime;
    protected ArrayList _trailUnitList;
    #endregion
	
	#region Get/Set Modifiers
	public float TimeStep {
		get { return _timeStep;	}
		set { _timeStep = value; }
	}
	#endregion
	
	#region Public Members
	public void StartTrail(float time)
    {
		_startTime = time;
	}
	
	public void StopTrail()
    {
		_startTime = -1.0f;
	}

    public void ResetTrail()
    {
        foreach (BallTrace item in _trailUnitList)
        {
            Destroy(item.obj);
        }
        _trailUnitList.Clear();
    }

    public void PrintTrailXDifferences()
    {
        for (int i = 1; i < _trailUnitList.Count; i++)
        {
            GameObject previous = ((BallTrace)_trailUnitList[i - 1]).obj as GameObject;
            GameObject current = ((BallTrace)_trailUnitList[i]).obj as GameObject;
            Debug.Log("Difference " + current.name + " -> " + previous.name + ": " + (current.transform.position.x - previous.transform.position.x));
        }
    }
    #endregion

    #region Protected Members
    protected void UpdateTrail(float time)
    {
		if ( _startTime	> 0.0f ) {
            if ((_updateLoop++) % _updateSteps == 0)
                AddTrailUnit();
		}
	}

    protected void AddTrailUnit()
    {
		GameObject ballTrail = (GameObject) Instantiate ( TrailUnit, transform.position, Quaternion.identity );
		ballTrail.name = "balltrail_" + _trailUnitList.Count;
        TrailUnitComponent comp = ballTrail.AddComponent<TrailUnitComponent>();
        comp.BallData = new BallTrace(ballTrail, rigidbody.position, rigidbody.rotation, rigidbody.velocity, rigidbody.angularVelocity);
        _trailUnitList.Add(comp.BallData);
	}
	#endregion
	
	#region Unity Callback	
	void Start () {
		_timeStep	= 0.1f;
		_startTime	= -1.0f;
		_trailUnitList = new ArrayList();
        _updateLoop = 1;
        _updateSteps = Mathf.FloorToInt(stepTime / Time.fixedDeltaTime);
	}
	
	void FixedUpdate () {
		//Debug.Log ( "Time: " + Time.time );
		UpdateTrail(Time.time);
	}
	
	void OnDestroy() {
		foreach (BallTrace item in _trailUnitList)
        {
			Destroy ( item.obj );
            item.obj = null;
		}
        _trailUnitList.Clear();
		_trailUnitList = null;
	}
	#endregion
}
