using UnityEngine;
using System.Collections;

[System.Serializable]
public class MovableObject
{
    public string name;
    public int type;
    public float x;
    public float y;
    public int score;
    public bool hit;

    [System.NonSerialized]
    public GameObject obj;

    public MovableObject(int type, GameObject obj, int score = 0, bool hit = false)
    {
        this.name = obj.name;
        this.type = type;
        this.obj = obj;
        this.x = obj.transform.position.x;
        this.y = obj.transform.position.y;
        this.score = score;
        this.hit = hit;
    }

    public Vector3 Position
    {
        get
        {
            return new Vector3(x, y, 0.0f);
        }

        set
        {
            x = value.x;
            y = value.y;
        }
    }
}