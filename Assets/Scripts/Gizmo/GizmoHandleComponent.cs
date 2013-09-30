using UnityEngine;
using System.Collections;

public class GizmoHandleComponent : MonoBehaviour
{
    public enum GizmoAxis { X, Y };

    public GizmoAxis axis;
    private float _mouseSensibility = 72.0f;
    private Transform _otherTransform;

    #region Unity Callbacks
    void OnMouseDown()
    {
        MyGame.Instance.OrthoCametaLocked = true;
    }

    void OnMouseUp()
    {
        MyGame.Instance.OrthoCametaLocked = false;
    }

    void OnMouseDrag()
    {
        float delta = 0.0f;
        float scale = GameCamera.Instance.camera.orthographicSize / Configuration.OrthoCamera.INITIAL_SIZE;
        switch (axis)
        {
            case (GizmoAxis.X):
                delta = Input.GetAxis("Mouse X") * Time.deltaTime * _mouseSensibility * scale;
                _otherTransform.Translate(Vector3.right * delta);
                break;
            case (GizmoAxis.Y):
                delta = Input.GetAxis("Mouse Y") * Time.deltaTime * _mouseSensibility * scale;
                _otherTransform.Translate(Vector3.up * delta);
                break;
        }
        ObjectManager.Instance.ObjectList[_otherTransform.gameObject.name].Position = _otherTransform.position;
    }
    #endregion

    public void setParent(Transform parent) {
        _otherTransform = parent;    
    }

    public void setVisibility(bool value)
    {
        gameObject.active = value;
        transform.GetChild(0).gameObject.active = value;
    }
}
