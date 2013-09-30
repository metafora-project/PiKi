using UnityEngine;
using System.Collections;

public class BallTrace
{
    public GameObject obj;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public BallTrace(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        this.obj = null;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }

    public BallTrace(GameObject obj, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
    {
        this.obj = obj;
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.angularVelocity = angularVelocity;
    }
}
