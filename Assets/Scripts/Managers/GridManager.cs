using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour
{
    #region Public Fields
    public GameObject Line;
	public float distance;
    #endregion

    #region Private Fields
    private GameObject _gridHolder;
	private bool _linesShown;
	private float _precision;
	private float _xOffset;
	private float _yOffset;
	private int _linesNum;
    #endregion

    #region Unity Callbacks
    void Start () {
		_gridHolder = GameObject.Find ( "GridHolder" );
		ShowLines ( false );
		_precision = 5.0f;
		_xOffset = 0.0f;
	    _yOffset = 0.0f;
		_linesNum = 20;
		distance = 100.0f;
		CreateLines ( );
		ShowLines ( false );
	}
	
	void Update () {
		CheckLinesVisibility ( );
		if ( _linesShown ) {			
			UpdateLinesSide ( );
		}
	}
	#endregion
	
	#region Lines Management
	private void CreateLines ( ) {
		// creates vertical lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = (GameObject) Instantiate ( Line, Vector3.zero, Quaternion.identity );
			line.name = "LineX" + i;
			line.transform.parent = _gridHolder.transform;
		}
		
		// creates horizontal lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = (GameObject) Instantiate ( Line, Vector3.zero, Quaternion.identity );
			line.name = "LineY" + i;
			line.transform.parent = _gridHolder.transform;
		}
	}	
	
	private void CheckLinesVisibility ( ) {
		if ( MyGame.Instance.IsGridEnabled && CameraStates.IsEditState ( GameCamera.Instance.FSM.State ) ) {
			if ( !_linesShown ) {
				ShowLines ( true );
			}
		} else if ( _linesShown ) {
			ShowLines ( false );
		}	
	}
	
	private void ShowLines ( bool flag ) {
		_linesShown = flag;
		for ( int i = 0; i < transform.childCount; i++ ) {
			transform.GetChild(i).renderer.enabled = flag;	
		}
	}	
	
	private void UpdateLinesSide ( ) {
		Vector3 cameraPosition = GameCamera.Instance.transform.position;
		float cameraSize = GameCamera.Instance.camera.orthographicSize;
		
		if ( cameraSize < 20.0f ) _precision = 2.5f;
		else if ( cameraSize < 40.0f ) _precision = 5.0f;
		else if ( cameraSize < 60.0f ) _precision = 10.0f;
		else if ( cameraSize < 80.0f ) _precision = 20.0f;
		else _precision = 40.0f;
		
		float lineWidth = 0.05f + 0.3f * ( ( cameraSize - Configuration.OrthoCamera.MIN_SIZE ) / ( Configuration.OrthoCamera.MAX_SIZE - Configuration.OrthoCamera.MIN_SIZE ));
		
		if ( GameCamera.Instance.FSM.State == CameraStates.SideCameraState ) UpdateSideCamera ( cameraPosition, cameraSize, lineWidth );
		else if ( GameCamera.Instance.FSM.State == CameraStates.BackCameraState ) UpdateBackCamera ( cameraPosition, cameraSize, lineWidth );
		else if ( GameCamera.Instance.FSM.State == CameraStates.TopCameraState ) UpdateTopCamera ( cameraPosition, cameraSize, lineWidth );
	}
	
	private void UpdateSideCamera ( Vector3 cameraPosition, float cameraSize, float lineWidth ) {
		
		float distToPrecisionX = cameraPosition.x % _precision;
		_xOffset = cameraPosition.x - distToPrecisionX;
		
		float distToPrecisionY = cameraPosition.y % _precision;
		_yOffset = cameraPosition.y - distToPrecisionY;
		
		// update vertical lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineX" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float xPos = _xOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( xPos, cameraSize * 2 + _yOffset, -distance ) );
			comp.SetPosition ( 1, new Vector3 ( xPos, -cameraSize * 2 + _yOffset, -distance ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
		
		// update horizontal lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineY" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float yPos = _yOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( -cameraSize * 2 + _xOffset, yPos, -distance ) );
			comp.SetPosition ( 1, new Vector3 ( cameraSize * 2 + _xOffset, yPos, -distance ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
	}
	
	private void UpdateBackCamera ( Vector3 cameraPosition, float cameraSize, float lineWidth ) {
		
		float distToPrecisionX = cameraPosition.z % _precision;
		_xOffset = cameraPosition.z - distToPrecisionX;
		
		float distToPrecisionY = cameraPosition.y % _precision;
		_yOffset = cameraPosition.y - distToPrecisionY;
				
		// update vertical lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineX" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float xPos = _xOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( -distance, cameraSize * 2 + _yOffset, xPos ) );
			comp.SetPosition ( 1, new Vector3 ( -distance, -cameraSize * 2 + _yOffset, xPos ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
		
		// update horizontal lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineY" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float yPos = _yOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( -distance, yPos, cameraSize * 2 + _xOffset ) );
			comp.SetPosition ( 1, new Vector3 ( -distance, yPos, -cameraSize * 2 + _xOffset ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
	}
	
	private void UpdateTopCamera ( Vector3 cameraPosition, float cameraSize, float lineWidth ) {
		
		float distToPrecisionX = cameraPosition.z % _precision;
		_xOffset = cameraPosition.z - distToPrecisionX;
		
		float distToPrecisionY = cameraPosition.x % _precision;
		_yOffset = cameraPosition.x - distToPrecisionY;
				
		// update vertical lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineX" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float xPos = _xOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( cameraSize * 2 + _yOffset, distance, xPos ) );
			comp.SetPosition ( 1, new Vector3 ( -cameraSize * 2 + _yOffset, distance, xPos ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
		
		// update horizontal lines
		for ( int i = 0; i < _linesNum; i++ ) {
			GameObject line = GameObject.Find ( "LineY" + i );
			LineRenderer comp = (LineRenderer)line.GetComponent("LineRenderer");
			float yPos = _yOffset + ( ( (float)i * _precision ) - ( _linesNum * 0.5f * _precision ) );
			comp.SetPosition ( 0, new Vector3 ( yPos, distance, -cameraSize * 2 + _xOffset ) );
			comp.SetPosition ( 1, new Vector3 ( yPos, distance, cameraSize * 2 + _xOffset ) );
			comp.SetWidth ( lineWidth, lineWidth );
		}
	}
	#endregion
}
