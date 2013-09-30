using UnityEngine;
using System.Collections;

public class TrailUnitComponent : MonoBehaviour
{
    #region Protected Members
    protected float _scaleValue;
    protected float _maxScaleValue;
    protected bool _showGUI;
    protected BallTrace _ballData;
    #endregion

    #region Get/Set Modifiers
    public BallTrace BallData
    {
        get
        {
            return _ballData;
        }
        set
        {
            _ballData = value;
        }
    }
    #endregion

    #region Unity callbacks
    void Start()
    {
        _scaleValue = 0.1f;
        _maxScaleValue = 0.5f;
        gameObject.transform.localScale = new Vector3(_scaleValue, _scaleValue, _scaleValue);
    }

    void Update()
    {
        UpdateScale();
        UpdateMouseOver();
    }

    void OnGUI()
    {
        if (_showGUI)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(10.0f, -10.0f, 0.0f);
            GUI.Window(0, new Rect(Mathf.Min(screenPos.x, Screen.width - 156.0f), Mathf.Min(Screen.height - screenPos.y, Screen.height - 66.0f), 150.0f, 60.0f), DataWindow, "Ball Data");
        }
    }
	#endregion

    #region Protected Members
    protected void UpdateScale()
    {
        if (_scaleValue < _maxScaleValue)
        {
            _scaleValue += Time.deltaTime * 1.5f;
        }
        else
        {
            _scaleValue = _maxScaleValue;
        }
        gameObject.transform.localScale = new Vector3(_scaleValue, _scaleValue, _scaleValue);
    }

    protected void UpdateMouseOver()
    {
        if (MyGame.Instance.FSM.State == GameplayStates.EditModeState)
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            _showGUI = Physics.Raycast(ray, out hitInfo) && (hitInfo.collider.gameObject.name == gameObject.name);
            
        }
    }

    protected void DataWindow(int windowId)
    {
        string posX = _ballData.position.x.ToString("f3");
        string posY = _ballData.position.y.ToString("f3");
        string velX = _ballData.velocity.x.ToString("f3");
        string velY = _ballData.velocity.y.ToString("f3");

        GUI.Label(new Rect(6.0f, 17.0f, 190.0f, 20.0f), "Pos: (" + posX + ", " + posY + ")");
        GUI.Label(new Rect(6.0f, 35.0f, 190.0f, 20.0f), "Vel: (" + velX + ", " + velY + ")");
    }
    #endregion
}
