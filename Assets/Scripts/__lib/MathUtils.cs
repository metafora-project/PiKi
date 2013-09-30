using System;
using UnityEngine;

public class MathUtils
{
    #region Constants
    public static float ToRadians = Mathf.PI / 180.0f;
    public static float ToDegrees = 180.0f / Mathf.PI;
    public static float Epsilon = 1.0e-5f;

    public static Vector3 Vector3Right   = new Vector3(1.0f, 0.0f, 0.0f);
    public static Vector3 Vector3Forward = new Vector3(0.0f, 0.0f, 1.0f);
    public static Vector3 Vector3Up      = new Vector3(0.0f, 1.0f, 0.0f);
    public static Vector3 Vector3One     = new Vector3(1.0f, 1.0f, 1.0f);
    #endregion

    #region Enums
    public enum ClipStatus {
        Inside = 0,
        Overlapping,
        Outside
    };
    #endregion

    #region Vector3 functions
    public static String ToString(Vector3 vector)
    {
        return "( " + vector.x + ", " + vector.y + ", " + vector.z + " )";
    }

    public static bool LessEqualAll(Vector3 v1, Vector3 v2)
    {
        return v1.x <= v2.x && v1.y <= v2.y && v1.z <= v2.z;
    }

    public static bool LessAll(Vector3 v1, Vector3 v2)
    {
        return v1.x < v2.x && v1.y < v2.y && v1.z < v2.z;
    }

    public static bool LessEqualAny(Vector3 v1, Vector3 v2)
    {
        return v1.x <= v2.x || v1.y <= v2.y || v1.z <= v2.z;
    }

    public static bool LessAny(Vector3 v1, Vector3 v2)
    {
        return v1.x < v2.x || v1.y < v2.y || v1.z < v2.z;
    }

    public static bool GreaterEqualAll(Vector3 v1, Vector3 v2)
    {
        return v1.x >= v2.x && v1.y >= v2.y && v1.z >= v2.z;
    }

    public static bool GreaterAll(Vector3 v1, Vector3 v2)
    {
        return v1.x > v2.x && v1.y > v2.y && v1.z > v2.z;
    }

    public static bool GreaterEqualAny(Vector3 v1, Vector3 v2)
    {
        return v1.x >= v2.x || v1.y >= v2.y || v1.z >= v2.z;
    }

    public static bool GreaterAny(Vector3 v1, Vector3 v2)
    {
        return v1.x > v2.x || v1.y > v2.y || v1.z > v2.z;
    }

    public static Vector3 RotateVector(Vector3 from, Vector3 to, float t, float maxAngle)
    {
        Vector3 axis = Vector3.Cross(from, to);

        if (Vector3.Dot(axis, axis) < Epsilon)
            return from;

        float angle = Vector3.Angle(from, to);
        Quaternion q = Quaternion.AngleAxis(Mathf.Min(maxAngle, angle * t), axis.normalized);

        return q * from;
    }

    public static Vector3 RotateVector(Vector3 from, Vector3 to, float t)
    {
        return RotateVector(from, to, t, 180.0f);
    }
    #endregion
 
    #region Bounds functions
    public static bool Contains(Bounds a, Bounds b)
    {
        return LessAll(a.min, b.min) && GreaterEqualAll(a.max, b.max);
    }

    public static bool Intersect(Bounds a, Bounds b)
    {
        return !(LessAny(a.max, b.min) || GreaterAny(a.min, b.max));
    }

    public static ClipStatus GetClipStatus(Bounds a, Bounds b)
    {
        if (Contains(a, b))
            return ClipStatus.Inside;
        else if (Intersect(a, b))
            return ClipStatus.Overlapping;
        else
            return ClipStatus.Outside;
    }
    #endregion

    #region Matrix4x4 functions
    public static Matrix4x4 MatrixFromBasis(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 position)
    {
        Matrix4x4 mat = new Matrix4x4();
        mat.SetColumn(0, new Vector4(xAxis.x, xAxis.y, xAxis.z, 1.0f));
        mat.SetColumn(1, new Vector4(yAxis.x, yAxis.y, yAxis.z, 1.0f));
        mat.SetColumn(2, new Vector4(zAxis.x, zAxis.y, zAxis.z, 1.0f));
        mat.SetColumn(3, new Vector4(position.x, position.y, position.z, 1.0f));
        return mat;
    }

    public static Matrix4x4 MatrixFromQuaternion(Quaternion q)
    {
        float oom = 1.0f / Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
        q.x *= oom;
        q.y *= oom;
        q.z *= oom;
        q.w *= oom;
        return Matrix4x4.TRS(Vector3.zero, q, Vector3One);
    }
    #endregion
};
