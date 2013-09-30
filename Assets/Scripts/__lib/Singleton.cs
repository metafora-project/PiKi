using System;
using UnityEngine;

#region Unity warning on script filename workaround
internal class Singleton { };
#endregion

public class Singleton <T> : MonoBehaviour
    where T: UnityEngine.Object
{
    #region Static members
    private static T instance = null;
	private static bool isValid = false;
    #endregion

    #region Static properties
    public static T Instance {
        get {
            if (null == instance) {
                UnityEngine.Object[] instances = FindObjectsOfType(typeof(T));
                DebugUtils.Assert(1 == instances.Length, "Singleton of type {0} have {1} instances!", typeof(T), instances.Length);
                instance = instances[0] as T;
				isValid = true;
            }

            return instance;
        }
    }

	public static bool IsValid {
		get {
			return isValid;
		}
	}
    #endregion

    #region Unity callbacks
    protected void Awake()
    {
//      instance = null;
//		isValid = false;
    }

    protected void OnDestroy()//OnApplicationQuit()
    {
		Debug.Log(String.Format("Deleted singleton of type {0}", typeof(T)));
        instance = null;
		isValid = false;
    }
    #endregion
}
