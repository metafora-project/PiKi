using UnityEngine;
using System.Collections;

public class HoopComponent : MonoBehaviour
{
    #region Public Fields
    public Transform ballTransform;
    #endregion

    #region Protected Fields
    protected float _radius;
    protected Vector3 _prevBallPos;
    protected Plane _basePlane;
    protected bool _active;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        _radius = 2.5f;
        _prevBallPos = Vector3.zero;
        _basePlane = new Plane(Vector3.left, transform.position);
        _active = true;
    }

    void Start ()
    {
        if (null == ballTransform)
        {
            ballTransform = GameObject.Find("CannonBall").transform;
        }
	}

	void Update ()
    {
        if (!_active)
            return;

        if (ballTransform.position.x < transform.position.x)
        {
            _prevBallPos = ballTransform.position;
        }
        else
        {
            Vector3 distVector = ballTransform.position - _prevBallPos;
            Ray ray = new Ray(_prevBallPos, distVector.normalized);
            float dist;
            if (_basePlane.Raycast(ray, out dist))
            {
                float distance = (ballTransform.position - transform.position).sqrMagnitude;
                if (distance < _radius * _radius)
                    OnPassInside(distance);
                else
                    OnPassOutside(distance);
            }
        }
    }
    #endregion

    #region Protected Members
    protected void OnPassInside(float distance)
    {
        _active = false;
        Debug.Log("OnPassInside: " + distance);
    }

    protected void OnPassOutside(float distance)
    {
        _active = false;
        Debug.Log("OnPassOutside: " + distance);
    }
    #endregion
}