using UnityEngine;
using System.Collections;

public class GizmoComponent : MonoBehaviour
{
    public enum GizmoType { HORIZONTAL, VERTICAL, BOTH }
    
    public GameObject gizmoAxis;
    public GizmoType type;

    private GameObject _gizmoObj;
    private GizmoGroupComponent _gizmo;

    private bool _active;
    public bool Active
    {
        get
        {
            _active = MyGame.Instance.IsTeacher || (DataContainer.Instance.Permissions & (DataContainer.Authorization.BlockMove | DataContainer.Authorization.TrampolineMove | DataContainer.Authorization.HoopMove)) > 0;
            return _active;
        }
        set
        {
            _active = value;
        }
    }
    
    #region UnityCallback
    void Awake()
    {
        _active = false;
    }

    void OnMouseDown()
    {
        if (!Active) return;
        MyGame.Instance.OrthoCametaLocked = true;
    }

    void OnMouseUp()
    {
        if (!Active) return;
        MyGame.Instance.OrthoCametaLocked = false;
        ObjectManager.Instance.SelectedObject = gameObject;
        if (gameObject.name.StartsWith("block"))
            MaterialManager.Instance.CurrentMaterial = MaterialManager.BLOCK;
        else if (gameObject.name.StartsWith("trampoline"))
            MaterialManager.Instance.CurrentMaterial = MaterialManager.TRAMPOLINE;
    }

    void Update()
    {
        if (!Active) return;
        float scale = GameCamera.Instance.camera.orthographicSize / Configuration.OrthoCamera.INITIAL_SIZE;
        if (null != _gizmoObj)
            _gizmoObj.transform.localScale = new Vector3(scale, scale, scale);
    }
    #endregion

    public void removeGizmo() {
        if (!Active) return;
        if (_gizmoObj)
        {
            gameObject.layer = 0;
            foreach (Transform child in transform) {
                child.gameObject.layer = 0;
            }
            Destroy(_gizmoObj);
            _gizmoObj = null;
            Destroy(_gizmo);
            _gizmo = null;
        }
    }

    public void resetGizmo() {
        if (!Active) return;
        removeGizmo();
        gameObject.layer = 2;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 2;
        }
        _gizmoObj = Instantiate(gizmoAxis, transform.position + new Vector3(0.0f, 0.0f, -82.5f), transform.rotation) as GameObject;
        _gizmoObj.transform.localScale *= GameCamera.Instance.camera.orthographicSize / Configuration.OrthoCamera.INITIAL_SIZE;
        _gizmo = _gizmoObj.GetComponent<GizmoGroupComponent>();
        _gizmo.setType(type);
        _gizmo.setParent(transform);
    }
}
