using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

public class MyGame : Game
{
    #region Singleton
    public static new MyGame Instance
    {
        get
        {
            return Game.Instance as MyGame;
        }
    }
    #endregion

    #region Public Fields
    public GUISkin pikiSkin;
    public Texture2D blockImage;
    public Texture2D trampolineImage;
    public Texture2D hoopImage;
    public Texture2D removeImage;
    public Texture2D saveImage;
    public Texture2D cameraImage;
    public Texture2D shareImage;

    public GameObject characterBoy;
    public GameObject characterGirl;

    public AudioSource clickSound;

    public string version;
    public bool _isTeacher;
    public bool isSceneSaved = true;

    public Plane actionPlane;

    public DataContainer.PlayMode currentPlayMode;

    public string lastDocId = string.Empty;
    public int distance = 0;
    #endregion

    #region Protected Fields
    protected FiniteStateMachine fsm;
    protected WWW _www;
    protected GameObject _player;
    protected CannonComponent _cannonComp;
    protected PikiInterface _interface;

    protected Dictionary<string, string> configParams;
    protected string server_url;
    protected string server_url_test;
    protected string _filename;
    protected string _output;
    protected float _angle;
    protected float _power;
    protected int _materialTabId;
    protected bool _isShooting;
    protected bool _grid;
    protected bool _showMaterialRules;
    protected bool _orthoCametaLocked;
    protected bool _showFilename;
    protected bool _showConfirmSave;
    protected bool _showConfirmShare;
    protected bool _uploadingFile;
    protected bool _showPermission;

    protected int _bestDistance;
    public int _score = 0;
    protected int _bestScore;

    protected string _userId;
    protected string _groupId;
    protected string _challengeId;
    protected string feedbackMessage = "HIF test";
    protected bool showLifMessage = false;
    protected bool showHifMessage = false;
    protected float lifTimer = -1.0f;
    #endregion    

    #region Indicators
    public float prevCannonPower = 0.0f;
    public float prevCannonAngle = 0.0f;
    public int cameraCounter = 0;
    public int saveCounter = 0;

    public float PrevCannonPower
    {
        get { return prevCannonPower; }
        set { prevCannonPower = value; }
    }
    public float PrevCannonAngle
    {
        get { return prevCannonAngle; }
        set { prevCannonAngle = value; }
    }
    public int CameraCounter
    {
        get { return cameraCounter; }
        set { cameraCounter = value; }
    }
    public int SaveCounter
    {
        get { return saveCounter; }
        set { saveCounter = value; }
    }
    #endregion

    #region Get/Set Modifiers
    public Dictionary<string, string> ConfigParams
    {
        get { return configParams; }
        set { configParams = value; }
    }
    public GameObject Player
    {
        get { return _player; }
        set { _player = value; }
    }
    public bool IsGridEnabled
    {
        get { return _grid; }
        set { _grid = value; }
    }
    public bool IsShooting
    {
        get { return _isShooting; }
    }
    public FiniteStateMachine FSM
    {
        get { return fsm; }
    }
    public bool OrthoCametaLocked
    {
        get { return _orthoCametaLocked; }
        set { _orthoCametaLocked = value; }
    }
    public string UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }
    public string GroupId
    {
        get { return _groupId; }
        set { _groupId = value; }
    }
    public string ChallengeId
    {
        get { return _challengeId; }
        set { _challengeId = value; }
    }
    public bool IsTeacher
    {
        get { return _isTeacher; }
        set { _isTeacher = value; }
    }
    public int Distance
    {
        get { return distance; }
        set
        {
            distance = value;
            if (distance > _bestDistance)
                _bestDistance = distance;
        }
    }
    public int BestDistance
    {
        get { return _bestDistance; }
        set { _bestDistance = value; }
    }
    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            if (_score > _bestScore)
                _bestScore = _score;
        }
    }
    public int BestScore
    {
        get { return _bestScore; }
        set { _bestScore = value; }
    }
    public string Output
    {
        set { _output = value + "\n" + _output; }
    }
    public bool IsInputEnabled
    {
        get { return !showHifMessage; }
    }
    public PikiInterface Interface
    {
        get { return _interface; }
    }
    #endregion
	
	#region Override functions
    protected override void OnInitializationBefore()    //Awake
	{
		base.OnInitializationBefore();
        fsm = null;
		_isShooting = false;
		_grid = false;
        _showMaterialRules = false;
        _materialTabId = 0;
        _showFilename = false;
        _showConfirmSave = false;
        _showConfirmShare = false;
        _uploadingFile = false;
        _filename = "";
        _showPermission = false;
        _player = null;
        _isTeacher = true;
        _score = 0;
        _bestScore = 0;
        distance = 0;
        version = "0.54";

        actionPlane = new Plane(Vector3.back, Vector3.zero);

        LoadConfigFile();
	}

	protected override void OnInitializationAfter()     //Start
	{
        base.OnInitializationAfter();

        _interface = GameObject.Find("Interface").GetComponent<PikiInterface>();
        _interface.SendMessage("StartInterface");
        
        CameraStates.InitializeCameras();
        fsm = gameObject.AddComponent<FiniteStateMachine>();
        fsm.State = GameplayStates.LoadingStateState;

        //Singleton initialization
        new MaterialManager();
        new DataContainer();
#if UNITY_EDITOR
        LoadConfiguration();
#endif

		GameObject click = GameObject.Find ( "clickSound" );
		clickSound = (AudioSource)(click.GetComponent ( "AudioSource" ));
        clickSound.volume = 0.8f;
	}
   
	protected override void OnUpdateBefore()
	{
		base.OnUpdateBefore();		
		fsm.ForceUpdate();

        if (Input.GetKeyDown(KeyCode.F12))
            docIdConsole = !docIdConsole;
	}
    
	protected override void OnUpdateAfter()
	{
		base.OnUpdateAfter();
	}
    
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		GameCamera.Instance.FSM.ForceUpdate();
	}
    
	public override void OnGamePaused()
	{
		base.OnGamePaused();
	}
    
	public override void OnGameResumed()
	{
		base.OnGameResumed();
	}

    bool docIdConsole = false;
	protected override void OnGUI()
	{
		base.OnGUI();
        GUI.skin = pikiSkin;

        GUI.Label(new Rect(250, 40, 780, 90), "Version: " + version + "." + ((XMPPBridge.Instance.IsTestServer)?"test":"prod"));
        /*if (IsTeacher)
        {
            GUI.Box(new Rect(320, 10, 120, 20), "");
            GUI.Label(new Rect(338, 10, 780, 90), "Teacher Mode");
        }*/
        //IsTeacher = GUI.Toggle(new Rect(10, 90, 100, 26), IsTeacher, "Teacher Mode");

        OnHIFMessage();
        OnLIFMessage();

        if (docIdConsole)
            GUI.TextField(new Rect(10, 500, 300, 20), lastDocId);

        /*
        if (FSM.State == GameplayStates.LoadingStateState)
            this.OnLoadingGUIState();

        if (FSM.State == GameplayStates.OffgameStateState)
            this.OnOffgameGUIState();

        //if (FSM.State == GameplayStates.PrepareLaunchState || FSM.State == GameplayStates.LaunchState || FSM.State == GameplayStates.EndLaunchState)
		//	this.OnLaunchGUIState();
		
		if ( FSM.State == GameplayStates.EditModeState )
			this.OnEditGUIState();

#if UNITY_EDITOR
        //GUI.Label(new Rect(10, 400, 780, 200), _output);
#endif
        */
	}
	#endregion

    #region GUI States
    private void OnLoadingGUIState()
    {
        GUI.Box(new Rect(Screen.width * 0.5f - 250.0f, 100.0f, 500.0f, 80.0f), "");
        GUI.Label(new Rect(Screen.width * 0.5f - 250.0f, 100.0f, 500.0f, 80.0f), "Loading Scenario, please wait...", "Big");
    }

    private void OnOffgameGUIState()
    {/*
        GUI.Box(new Rect(Screen.width * 0.5f - 120.0f, 70.0f, 240.0f, 60.0f), "");
        GUI.Label(new Rect(Screen.width * 0.5f - 250.0f, 70.0f, 500.0f, 60.0f), "Choose your character", "Big");
     */
    }

	private void OnLaunchGUIState()
	{
        float baseX = Screen.width - 260.0f;
        float baseY = 250.0f;
        float boxHeigth = 110.0f;

        GUI.Box(new Rect(baseX, baseY, 250.0f, boxHeigth), "");
        GUI.Label(new Rect(baseX + 10.0f, baseY + 5.0f, 100.0f, 30.0f), "Angle:");
        GUI.Label(new Rect(baseX + 10.0f, baseY + 30.0f, 100.0f, 30.0f), "Velocity:");
		
		if ( FSM.State == GameplayStates.LaunchState || FSM.State == GameplayStates.EndLaunchState ) GUI.enabled = false;

        if ((!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.CannonAngle) == 0) || showHifMessage)
            GUI.enabled = false;
        _angle = GUI.HorizontalSlider(new Rect(baseX + 75.0f, baseY + 10.0f, 100, 10), _angle, Configuration.Cannon.MIN_ANGLE, Configuration.Cannon.MAX_ANGLE);
        GUI.enabled = true;

        if (FSM.State == GameplayStates.LaunchState || FSM.State == GameplayStates.EndLaunchState) GUI.enabled = false;

        if ((!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.CannonPower) == 0) || showHifMessage)
            GUI.enabled = false;
        _power = GUI.HorizontalSlider(new Rect(baseX + 75.0f, baseY + 35.0f, 100, 10), _power, Configuration.Cannon.MIN_POWER, Configuration.Cannon.MAX_POWER);
		
        GUI.enabled = true;

        GUI.Label(new Rect(baseX + 190.0f, baseY + 5.0f, 100.0f, 30.0f), Mathf.FloorToInt(_angle) + "°");
        GUI.Label(new Rect(baseX + 190.0f, baseY + 30.0f, 100.0f, 30.0f), Mathf.FloorToInt(_power) + " m/s");

        GUI.Label(new Rect(baseX + 10.0f, baseY + 55.0f, 100.0f, 30.0f), "Distance:");
        GUI.Label(new Rect(baseX + 80, baseY + 55.0f, 100.0f, 30.0f), distance.ToString() + " m");
        GUI.Label(new Rect(baseX + 140, baseY + 55.0f, 100.0f, 30.0f), "Best:");
        GUI.Label(new Rect(baseX + 190, baseY + 55.0f, 100.0f, 30.0f), _bestDistance.ToString() + "m");

        GUI.Label(new Rect(baseX + 10, baseY + 80.0f, 100.0f, 30.0f), "Score:");
        GUI.Label(new Rect(baseX + 80, baseY + 80.0f, 100.0f, 30.0f), _score.ToString());
        GUI.Label(new Rect(baseX + 140, baseY + 80.0f, 100.0f, 30.0f), "Best:");
        GUI.Label(new Rect(baseX + 190, baseY + 80.0f, 100.0f, 30.0f), _bestScore.ToString() );
		
		//if ( FSM.State == GameplayStates.LaunchState ) GUI.enabled = false;
		
		if ( GUI.changed )
		{
			_cannonComp.CannonAngle = Mathf.Floor(_angle);
			_cannonComp.CannonPower = Mathf.Floor(_power);
		}

        GUI.enabled = !showHifMessage;

		if ( FSM.State == GameplayStates.PrepareLaunchState ) {

            //if (GUI.Button(new Rect(Screen.width - 85.0f, boxHeigth + 15, 75, 25), "FIRE!"))
            //{
            //    OnLoad();
            //    clickSound.Play();
            //}
			
		} else {

            //if (GUI.Button(new Rect(Screen.width - 85.0f, boxHeigth + 15, 75, 25), "Fire Again"))
            //{
            //    _cannonComp.DestroyBall();
            //    FSM.State = GameplayStates.PrepareLaunchState;
            //    clickSound.Play();
            //}
			
		}

        GUI.enabled = (FSM.State != GameplayStates.LaunchState && !showHifMessage);

        //if (GUI.Button(new Rect(10, 10, 150, 50), "Edit Mode", "BigButton"))
        //{
        //    FSM.State = GameplayStates.EditModeState;
        //    clickSound.Play();
        //}

        GUI.enabled = true;
	}

	private void OnEditGUIState()
    {	
        //GUI.Box ( new Rect ( 10.0f, Screen.height - 140, 250.0f, 60.0f ), "Controls" );
        //GUI.Label ( new Rect ( 14.0f, Screen.height - 120, 242.0f, 30.0f ), "Hold LB and Drag the Mouse to PAN" );
        //GUI.Label(new Rect(14.0f, Screen.height - 104, 242.0f, 30.0f), "Hold RB and Drag the Mouse to ZOOM");

        //GUI.Box(new Rect(Screen.width - 380, Screen.height - 80, 153.0f, 20.0f), "");
        //if (_showPermission || _showMaterialRules || GameCamera.Instance.FSM.State == CameraStates.SideCameraState || showHifMessage)
        //    GUI.enabled = false;
        //GUI.Label(new Rect(Screen.width - 372, Screen.height - 80, 48, 48), "SIDE");
        //if (GUI.Button(new Rect(Screen.width - 380, Screen.height - 58, 48, 48), cameraImage))
        //{
        //    GameCamera.Instance.FSM.State = CameraStates.SideCameraState;
        //    clickSound.Play();
        //}
        //GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || GameCamera.Instance.FSM.State == CameraStates.BackCameraState || showHifMessage)
        //    GUI.enabled = false;
        //GUI.Label(new Rect(Screen.width - 322, Screen.height - 80, 48, 48), "BACK");
        //if (GUI.Button(new Rect(Screen.width - 328, Screen.height - 58, 48, 48), cameraImage))
        //{
        //    GameCamera.Instance.FSM.State = CameraStates.BackCameraState;
        //    clickSound.Play();
        //}
        //GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || GameCamera.Instance.FSM.State == CameraStates.TopCameraState || showHifMessage)
        //    GUI.enabled = false;
        //GUI.Label(new Rect(Screen.width - 266, Screen.height - 80, 48, 48), "TOP");
        //if (GUI.Button(new Rect(Screen.width - 276, Screen.height - 58, 48, 48), cameraImage))
        //{
        //    GameCamera.Instance.FSM.State = CameraStates.TopCameraState;
        //    clickSound.Play();
        //}

        //GUI.enabled = !showHifMessage;

        //if (GUI.Button(new Rect(10, 10, 150, 50), "Launch Mode", "BigButton"))
        //{
        //    _cannonComp.DestroyBall ( );
        //    FSM.State = GameplayStates.PrepareLaunchState;
        //    clickSound.Play();
        //}

        //_grid = GUI.Toggle(new Rect(10, 65, 100, 26), _grid, "Grid enabled");

        string btnTxt;
        if (IsTeacher)
        {
            if (_showMaterialRules || _showFilename || showHifMessage)
                GUI.enabled = false;
            btnTxt = "Show Locks";
            if (_showPermission)
                btnTxt = "Hide Locks";

            if (GUI.Button(new Rect(Screen.width - 220, 70, 100, 26), btnTxt))
            {
                _showPermission = !_showPermission;
                MyGame.Instance.OrthoCametaLocked = false;
            }
        }
        GUI.enabled = true;

        if (_showPermission || _showFilename || (!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialShow) == 0) || showHifMessage)
            GUI.enabled = false;
        btnTxt = "Show Rules";
        if (_showMaterialRules)
            btnTxt = "Hide Rules";

        if (GUI.Button(new Rect(Screen.width - 110, 70, 100, 26), btnTxt))
        {
            _showMaterialRules = !_showMaterialRules;
            MyGame.Instance.OrthoCametaLocked = false;
        }

        GUI.enabled = true;
        if (_showFilename)
        {
            int width = 310, heigth = 140;
            GUI.Window(100, new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - heigth * 0.5f, width, heigth), FilenameWindow, "Save...");
        }

        if (_showConfirmSave)
        {
            int width = 310, heigth = 140;
            GUI.Window(100, new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - heigth * 0.5f, width, heigth), ConfirmWindowSave, "Save...");
        }

        if (_showConfirmShare)
        {
            int width = 310, heigth = 140;
            GUI.Window(100, new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - heigth * 0.5f, width, heigth), ConfirmWindowShare, "Share...");
        }
        
        if (_showMaterialRules)
            GUI.Window(_materialTabId, new Rect(460.0f, 40.0f, 330.0f, 496.0f), MaterialWindow, "Material Rules [" + MaterialManager.Instance.CurrentMaterial + "]");
        
        if (_showPermission)
            GUI.Window(50, new Rect(540.0f, 40.0f, 250.0f, 150.0f), PermissionWindow, "Locks Settings");

        if (_showPermission || _showMaterialRules || _showFilename || showHifMessage)
            GUI.enabled = false;

        //SAVE BUTTON
        //if (GUI.Button(new Rect(Screen.width - 442, Screen.height - 58, 48, 48), saveImage))
        //{
        //    _showConfirmSave = true;
        //    _showFilename = true;
        //    clickSound.Play();
        //}

        GUI.enabled = !((XMPPBridge.Instance.DocId == null) || XMPPBridge.Instance.DocId.Equals("none"));

        if (_showPermission || _showMaterialRules || _showFilename || showHifMessage)
            GUI.enabled = false;

        //if (GUI.Button(new Rect(Screen.width - 494, Screen.height - 58, 48, 48), shareImage))
        //{
        //    _showConfirmShare = true;
        //    _showFilename = true;
        //    clickSound.Play();
        //}

        GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || (!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.BlockAddRemove) == 0) || showHifMessage)
        //    GUI.enabled = false;
        //if (GUI.Button(new Rect(Screen.width - 194, Screen.height - 120, 48, 48), blockImage))
        //{
        //    Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //    worldPosition.z = 0.0f;
        //    ObjectManager.Instance.PlaceObject(ObjectManager.BLOCK, worldPosition);
        //    //ObjectManager.Instance.PlaceObject(ObjectManager.BLOCK, new Vector3(GameCamera.Instance.camera.transform.position.x, -0.5f, 0.0f));
        //    clickSound.Play();
        //}
        //GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || (!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.TrampolineAddRemove) == 0) || showHifMessage)
        //    GUI.enabled = false;
        //if (GUI.Button(new Rect(Screen.width - 142, Screen.height - 120, 48, 48), trampolineImage))
        //{
        //    ObjectManager.Instance.PlaceObject(ObjectManager.TRAMPOLINE, new Vector3(GameCamera.Instance.camera.transform.position.x, -0.5f, 0.0f));
        //    clickSound.Play();
        //}
        //GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || (!IsTeacher && (DataContainer.Instance.Permissions & DataContainer.Authorization.HoopAddRemove) == 0) || showHifMessage)
        //    GUI.enabled = false;
        //if (GUI.Button(new Rect(Screen.width - 90, Screen.height - 120, 48, 48), hoopImage))
        //{
        //    Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //    worldPosition.z = 0.0f;
        //    ObjectManager.Instance.PlaceObject(ObjectManager.HOOP, worldPosition);
        //    //ObjectManager.Instance.PlaceObject(ObjectManager.HOOP, new Vector3(GameCamera.Instance.camera.transform.position.x, 5.0f, 0.0f));
        //    clickSound.Play();
        //}
        //GUI.enabled = true;

        //if (_showPermission || _showMaterialRules || _showFilename || (!IsTeacher && (DataContainer.Instance.Permissions & (DataContainer.Authorization.BlockAddRemove | DataContainer.Authorization.TrampolineAddRemove | DataContainer.Authorization.HoopAddRemove)) == 0) || showHifMessage)
        //    GUI.enabled = false;
        //if (GUI.Button(new Rect(Screen.width - 58, Screen.height - 58, 48, 48), removeImage))
        //{
        //    if (ObjectManager.Instance.RemoveSelected())
        //        clickSound.Play();
        //}

        /* CHEAT TMP */
        //if (GUI.Button(new Rect(400, 40, 48, 48), "<-"))
        //{
        //    ObjectManager.Instance.ClearSelected();
        //    clickSound.Play();
        //}

        //int objCount = ObjectManager.Instance.GetObjectCount("Flag");
        //if (GUI.Button(new Rect(450, 40, 48, 48), "FL " + objCount))
        //{
        //    if (objCount > 0)
        //    {
        //        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //        worldPosition.z = 0.0f;
        //        ObjectManager.Instance.PlaceObject(ObjectManager.FLAG, worldPosition);
        //        clickSound.Play();
        //    }
        //}

        //objCount = ObjectManager.Instance.GetObjectCount("Palm");
        //if (GUI.Button(new Rect(500, 40, 48, 48), "PA " + objCount))
        //{
        //    if (objCount > 0)
        //    {
        //        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //        worldPosition.z = 0.0f;
        //        ObjectManager.Instance.PlaceObject(ObjectManager.PALM, worldPosition);
        //        clickSound.Play();
        //    }
        //}

        //objCount = ObjectManager.Instance.GetObjectCount("Boxes");
        //if (GUI.Button(new Rect(550, 40, 48, 48), "BO " + objCount))
        //{
        //    if (objCount > 0)
        //    {
        //        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //        worldPosition.z = 0.0f;
        //        ObjectManager.Instance.PlaceObject(ObjectManager.BOXES, worldPosition);
        //        clickSound.Play();
        //    }
        //}

        //objCount = ObjectManager.Instance.GetObjectCount("Cloud");
        //if (GUI.Button(new Rect(600, 40, 48, 48), "CL " + objCount))
        //{
        //    if (objCount > 0)
        //    {
        //        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //        worldPosition.z = 0.0f;
        //        ObjectManager.Instance.PlaceObject(ObjectManager.CLOUD, worldPosition);
        //        clickSound.Play();
        //    }
        //}

        //if (GUI.Button(new Rect(650, 40, 48, 48), "BL"))
        //{
        //    Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //    worldPosition.z = 0.0f;
        //    ObjectManager.Instance.PlaceObject(ObjectManager.BLOCK, worldPosition);
        //    clickSound.Play();
        //}

        //if (GUI.Button(new Rect(700, 40, 48, 48), "TR"))
        //{
        //    Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        //    worldPosition.z = 0.0f;
        //    ObjectManager.Instance.PlaceObject(ObjectManager.TRAMPOLINE, worldPosition);
        //    clickSound.Play();
        //}


        GUI.enabled = true;

        if (!MyGame.Instance.OrthoCametaLocked)
            MyGame.Instance.OrthoCametaLocked = _showMaterialRules || _showFilename || _showPermission || showHifMessage;
    }

    #region GUI Functions
    public void GUISave()
    {
        _showConfirmSave = true;
        _showFilename = true;
        Debug.Log("GUISave");
    }

    public void GUIShare()
    {
        _showConfirmShare = true;
        _showFilename = true;
    }

    private void OnHIFMessage()
    {
        float width = 225.0f;
        float height = 170.0f;
        float x = Screen.width * 0.5f - width * 0.5f;
        float y = Screen.height * 0.5f - height * 0.5f;

        GUI.enabled = true;
        if (showHifMessage)
        {
            //window
            GUI.Box(new Rect(x, y, width, height), "", "FeedbackWindows");

            //image
            Vector2 imgOffset = new Vector2(20.0f, 40.0f);
            GUI.Box(new Rect(x + imgOffset.x, y + imgOffset.y, 43, 84), "", "FeedbackImage");

            //text
            Vector2 txtOffset = new Vector2(70.0f, 40.0f);
            GUI.TextArea(new Rect(x + txtOffset.x, y + txtOffset.y, width - (txtOffset.x + 4), height - (txtOffset.y + 40)), feedbackMessage, "FeedbackHIFText");

            //btn
            Vector2 btnOffset = new Vector2(137.0f, 137.0f);
            if (GUI.Button(new Rect(x + btnOffset.x, y + btnOffset.y, 75.0f, 20.0f), "OK", "FeedbackBtn"))
            {
                showHifMessage = false;
                Debug.Log("showHifMessage: " + showHifMessage);
            }

            GUI.enabled = false;
        }
    }

    private void OnLIFMessage()
    {
        float width = 200.0f;
        float height = 100.0f;
        float x = Screen.width - width - 4.0f;
        float y = Screen.height - height - 4.0f;
        if (showLifMessage)
        {
            //window
            GUI.Box(new Rect(x, y, width, height), "", "FeedbackWindows");

            //text
            Vector2 txtOffset = new Vector2(4.0f, 30.0f);
            GUI.TextArea(new Rect(x + txtOffset.x, y + txtOffset.y, width - (txtOffset.x + 4), height - (txtOffset.y + 4)), feedbackMessage, "FeedbackLIFText");

            //btn
            Vector2 btnOffset = new Vector2(180.0f, 6.0f);
            if (GUI.Button(new Rect(x + btnOffset.x, y + btnOffset.y, 14.0f, 14.0f), "X", "FeedbackCloseBtn"))
            {
                showLifMessage = false;
                lifTimer = -1.0f;
            }

            if (lifTimer < 0)
            {
                lifTimer = Time.time;
            }
            else
            {
                if (Time.time - lifTimer > 30.0)
                {
                    lifTimer = -1.0f;
                    showLifMessage = false;
                }
            }
        }
    }

    void PermissionWindow(int windowId)
    {
        int baseY = 20;
        int rowHeight = 20;
        int counter = 0;

        Dictionary<string, DataContainer.Authorization> permissionMap = new Dictionary<string, DataContainer.Authorization>();
        permissionMap.Add("Velocity", DataContainer.Authorization.CannonPower);
        permissionMap.Add("Angle", DataContainer.Authorization.CannonAngle);
        permissionMap.Add("Objects Add/Remove", DataContainer.Authorization.BlockAddRemove | DataContainer.Authorization.TrampolineAddRemove | DataContainer.Authorization.HoopAddRemove);
        permissionMap.Add("Objects Move", DataContainer.Authorization.BlockMove | DataContainer.Authorization.TrampolineMove | DataContainer.Authorization.HoopMove);
        permissionMap.Add("Rules Show", DataContainer.Authorization.MaterialShow);
        permissionMap.Add("Rules Add/Remove", DataContainer.Authorization.MaterialAddRemove);

        foreach (KeyValuePair<string, DataContainer.Authorization> item in permissionMap)
        {
            if (!GUI.Toggle(new Rect(10, baseY + counter * rowHeight, 200, 20), (DataContainer.Instance.Permissions & item.Value) <= 0, item.Key))
            {
                if ((DataContainer.Instance.Permissions & item.Value) == 0)
                {
                    DataContainer.Instance.Permissions = DataContainer.Instance.Permissions | item.Value;
                }
            }
            else
            {
                if ((DataContainer.Instance.Permissions & item.Value) > 0)
                {
                    DataContainer.Instance.Permissions = DataContainer.Instance.Permissions & ~item.Value;
                }
            }
            counter++;
        }
    }

    bool factorFlag = false;
    void MaterialWindow(int windowId)
    {
        if (GUI.Button(new Rect(7, 20, 155, 20), "Vertical Component")) { _materialTabId = 0; }
        if (GUI.Button(new Rect(168, 20, 155, 20), "Horizontal Component")) { _materialTabId = 1; }

        int baseY = 50;
        int rowHeight = 26;
        string currentMaterial = MaterialManager.Instance.CurrentMaterial;

        string title = "Vertical Rules";
        if (windowId == 1)
            title = "Horizontal Rules";

        ArrayList block = MaterialManager.Instance.VerticalRules[currentMaterial];
        if (windowId == 1)
            block = MaterialManager.Instance.HorizontalRules[currentMaterial];

        GUI.Label(new Rect(4, baseY, 200, 20), title);
        GUI.enabled = (block.Count < 16 && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0));
        GUI.Label(new Rect(260, baseY, 200, 20), "Add rule");
        if (GUI.Button(new Rect(315, baseY, 20, 20), "+", "label"))
        {
            if (windowId == 0)
                MaterialManager.Instance.AddVerticalRule(currentMaterial, new MaterialRule());
            else if (windowId == 1)
                MaterialManager.Instance.AddHorizontalRule(currentMaterial, new MaterialRule());
        }
        GUI.enabled = true;
        
        for (int i = 0; i < block.Count; i++)
        {
            int ruleIdx = block.Count - 1 - i;
            MaterialRule rule = block[ruleIdx] as MaterialRule;

            #region TextField Management
            if (!GUI.GetNameOfFocusedControl().Equals("factor" + ruleIdx))
            {
                rule.FactorString = rule.FactorValue.ToString();
                if (rule.FactorValue == Mathf.Infinity)
                    rule.FactorString = "inf";
            }
            else
            {
                if (Event.current.Equals(Event.KeyboardEvent("return")))
                {
                    if (rule.FactorString.Equals("inf"))
                        rule.FactorValue = Mathf.Infinity;
                    else
                    {
                        float val = rule.FactorValue;
                        if (float.TryParse(rule.FactorString, out val))
                            rule.FactorValue = val;
                    }
                    GUI.FocusControl("min" + ruleIdx);
                    factorFlag = true;
                }
            }

            if (!GUI.GetNameOfFocusedControl().Equals("max" + ruleIdx))
            {
                rule.MaxString = rule.MaxValue.ToString();
                if (rule.MaxValue == Mathf.Infinity)
                    rule.MaxString = "inf";
            }
            else
            {
                if (Event.current.Equals(Event.KeyboardEvent("return")))
                {
                    if (rule.MaxString.Equals("inf"))
                        rule.MaxValue = Mathf.Infinity;
                    else
                    {
                        float val = rule.MaxValue;
                        if (float.TryParse(rule.MaxString, out val))
                            rule.MaxValue = val;
                    }
                    GUI.FocusControl("factor" + ruleIdx);
                }
            }

            if (!GUI.GetNameOfFocusedControl().Equals("min" + ruleIdx))
            {
                rule.MinString = rule.MinValue.ToString();
                if (rule.MinValue == Mathf.Infinity)
                    rule.MinString = "inf";
            }
            else
            {
                if (factorFlag)
                {
                    factorFlag = false;
                }
                else
                {
                    if (Event.current.Equals(Event.KeyboardEvent("return")))
                    {
                        if (rule.MinString.Equals("inf"))
                            rule.MinValue = Mathf.Infinity;
                        else
                        {
                            float val = rule.MinValue;
                            if (float.TryParse(rule.MinString, out val))
                                rule.MinValue = val;
                        }
                        GUI.FocusControl("max" + ruleIdx);
                    }
                }
            }
            #endregion

            GUI.Label(new Rect(4, baseY + rowHeight * (i + 1), 16, 20), "IF(");
            GUI.SetNextControlName("min" + ruleIdx);
            GUI.enabled = ((i != block.Count - 1) && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0));
            rule.MinString = GUI.TextField(new Rect(22, baseY + rowHeight * (i + 1), 42, 20), rule.MinString, 5);
            GUI.enabled = true;
            GUI.SetNextControlName("");
            GUI.Label(new Rect(64, baseY + rowHeight * (i + 1), 60, 20), "< Vin <=");
            GUI.SetNextControlName("max" + ruleIdx);
            GUI.enabled = ((i != block.Count - 1) && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0));
            rule.MaxString = GUI.TextField(new Rect(118, baseY + rowHeight * (i + 1), 42, 20), rule.MaxString, 5);
            GUI.enabled = true;
            GUI.SetNextControlName("");
            GUI.Label(new Rect(160, baseY + rowHeight * (i + 1), 100, 20), ") THEN Vout =");
            GUI.SetNextControlName("factor" + ruleIdx);
            GUI.enabled = ((i != block.Count - 1) && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0));
            rule.FactorString = GUI.TextField(new Rect(248, baseY + rowHeight * (i + 1), 42, 20), rule.FactorString, 5);
            if ((i != block.Count - 1) && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0))
            {
                if (GUI.Button(new Rect(293, baseY + rowHeight * (i + 1), 20, 20), "OK", "SmallButton"))
                {
                    if (rule.FactorString.Equals("inf"))
                        rule.FactorValue = Mathf.Infinity;
                    else
                    {
                        float val = rule.FactorValue;
                        if (float.TryParse(rule.FactorString, out val))
                            rule.FactorValue = val;
                    }
                    rule.FactorString = rule.FactorValue.ToString();

                    if (rule.MaxString.Equals("inf"))
                        rule.MaxValue = Mathf.Infinity;
                    else
                    {
                        float val = rule.MaxValue;
                        if (float.TryParse(rule.MaxString, out val))
                            rule.MaxValue = val;
                    }
                    rule.MaxString = rule.MaxValue.ToString();

                    if (rule.MinString.Equals("inf"))
                        rule.MinValue = Mathf.Infinity;
                    else
                    {
                        float val = rule.MinValue;
                        if (float.TryParse(rule.MinString, out val))
                            rule.MinValue = val;
                    }
                    rule.MinString = rule.MinValue.ToString();
                }
            }
            if ((i != block.Count - 1) && (IsTeacher || (DataContainer.Instance.Permissions & DataContainer.Authorization.MaterialAddRemove) > 0))
            {
                if (GUI.Button(new Rect(317, baseY + rowHeight * (i + 1), 20, 20), "-", "label"))
                {
                    if (windowId == 0)
                        MaterialManager.Instance.RemoveVerticalRule(currentMaterial, rule);
                    else if (windowId == 1)
                        MaterialManager.Instance.RemoveHorizontalRule(currentMaterial, rule);
                }
            }
        }
    }

    void ConfirmWindowSave(int windowId)
    {
        if (!_uploadingFile)
        {
            GUI.Label(new Rect(10, 30, 270, 100), "Save current state?");
        }
        else
        {
            GUI.Label(new Rect(10, 30, 270, 100), "Saving to server... ");
        }
        if (GUI.Button(new Rect(40, 100, 100, 30), "Save"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            saveCounter++;
//#if !UNITY_EDITOR
            SaveBinaryData("");
//#endif
            _uploadingFile = true;
        }
        if (GUI.Button(new Rect(170, 100, 100, 30), "Cancel"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            _showFilename = false;
            _showConfirmSave = false;
            _uploadingFile = false;
        }
    }

    void ConfirmWindowShare(int windowId)
    {
        GUI.Label(new Rect(10, 30, 270, 100), "Share current state to LASAD?");

        if (GUI.Button(new Rect(40, 100, 100, 30), "Share"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            _showFilename = false;
            _showConfirmShare = false;
            XMPPBridge.Instance.SendReferableObject("LASAD");
        }
        if (GUI.Button(new Rect(170, 100, 100, 30), "Cancel"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            _showFilename = false;
            _showConfirmShare = false;
        }
    }
    
    void FilenameWindow(int windowId)
    {
        if (!_uploadingFile)
        {
            GUI.Label(new Rect(20, 30, 270, 30), "File name:");
            _filename = GUI.TextField(new Rect(20, 50, 270, 30), _filename, 30);
        }
        else
        {
            GUI.Label(new Rect(10, 30, 270, 100), "Saving to server... " + _www.error);
        }
        if (GUI.Button(new Rect(40, 100, 100, 30), "Save"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            _filename = _filename.Trim();
            _filename = _filename.Replace(" ", "_");
            if (!_filename.Equals(""))
                SaveBinaryData(_filename);
        }
        if (GUI.Button(new Rect(170, 100, 100, 30), "Cancel"))
        {
            MyGame.Instance.OrthoCametaLocked = false;
            _filename = _filename.Trim();
            _filename = _filename.Replace(" ", "_");
            _showFilename = false;
            _uploadingFile = false;
        }
    }
    #endregion
    #endregion

    #region Public Methods
    public void InitializeFromDataContainer()
    {
        Debug.Log("InitializeFromDataContainer " + DataContainer.Instance.OwnerGroup.ToString() + ", " + GroupId.ToString());
        _angle = DataContainer.Instance.CannonAngle;
        _power = DataContainer.Instance.cannonPower;
        _cannonComp = (CannonComponent)GameObject.Find("CannonGroup").GetComponent("CannonComponent");
        _cannonComp.CannonAngle = _angle;
        _cannonComp.CannonPower = _power;

        bool canProcede = true;
        if (DataContainer.Instance.sceneState == DataContainer.SceneState.Opened)
        {
            canProcede = (DataContainer.Instance.OwnerGroup.Equals(string.Empty) || DataContainer.Instance.OwnerGroup.Equals(GroupId));
            Debug.Log("canProcede " + canProcede);

            if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            {
                if (DataContainer.Instance.Player == PlayerController.CharacterType.MALE)
                    GameObject.Find("character_boy").GetComponent<PlayerController>().PlaceCharacter();
                else
                    GameObject.Find("character_girl").GetComponent<PlayerController>().PlaceCharacter();
            }
            else
            {
                GameObject.Find("character_boy").SendMessage("HideCharacter");
                GameObject.Find("character_girl").SendMessage("HideCharacter");
            }
        }

        if (canProcede)
        {
            if (fsm.State != GameplayStates.OffgameStateState)
                fsm.State = GameplayStates.OffgameStateState;
        }
        else
        {
            Debug.Log("You can't open this resource card! ");
            Interface.SetLoadingMessage("You can't open this resource card!");
        }
    }

    public void OnShowHIF(string message)
    {
        feedbackMessage = message;
        showHifMessage = true;
    }

    public void OnShowLIF(string message)
    {
        feedbackMessage = message;
        showLifMessage = true;
    }

    public void OnLoad()
    {
        _cannonComp.Load();
        //Score = 0;
        Distance = 0;
        if (null != Player)
            Player.GetComponent<PlayerController>().FSM.State = PlayerController.FireStateState;
    }

    public void OnShoot()
    {
        _isShooting = true;
        _cannonComp.Shoot();
    }

    public void OnBallStop()
    {
        _isShooting = false;
        FSM.State = GameplayStates.EndLaunchState;
    }

    public void OnHoopPassed()
    {
        Score += 200;
    }
    #endregion

    #region Load/Save
    public void LoadConfigFile()
    {
        configParams = new Dictionary<string, string>();
#if UNITY_EDITOR
        try
        {
            XmlTextReader reader = new XmlTextReader("config.xml");
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "property")
                    {
                        string key = reader.GetAttribute("key");
                        string value = reader.GetAttribute("value");
                        configParams.Add(key, value);
                    }
                }
            }
            server_url = configParams.ContainsKey("server_uploadurl") ? configParams["server_uploadurl"] : "http://metafora.ku-eichstaett.de/metaforaservicemodul/metaforaservicemodul/fileupload";
            server_url_test = configParams.ContainsKey("server_uploadurl_test") ? configParams["server_uploadurl_test"] : "http://metafora.ku-eichstaett.de/servicemodulnewxmpp/metaforaservicemodul/fileupload";
        }
        catch (FileNotFoundException e)
        {
            Debug.Log("" + e);
            _output += "" + e;
        }
#else
        _www = new WWW("config.xml");
        StartCoroutine(WaitConfigData());
#endif
    }

    public void LoadConfiguration()
    {
        MyGame.Instance.Output = "LoadConfiguration";
        UserId = configParams.ContainsKey("metafora_userid") ? configParams["metafora_userid"] : "none";
        GroupId = configParams.ContainsKey("metafora_groupid") ? configParams["metafora_groupid"] : "none";
        ChallengeId = configParams.ContainsKey("metafora_challengeid") ? configParams["metafora_challengeid"] : "none";
#if UNITY_EDITOR
        DataContainer.Instance.SetDefault();
        LoadBinaryData("none");
        //LoadBinaryData("7fe5db1bc941cab030051c2a7c3d5a47");
#else
        string srcValue = Application.srcValue;
        string[] address = srcValue.Split('?');
        if (address.Length > 1)
        {
            if (address.Length > 2)
            {
                for (int i = 2; i < address.Length; i++)
                {
                    address[1] += "?" + address[i];
                }
            }

            string[] splt = address[1].Split('&');
            if (splt.Length > 0)
            {
                DataContainer.Instance.SetDefault();

                foreach (string couple in splt)
                {
                    string[] keyValue = couple.Split('=');
                    if (keyValue.Length > 2)
                    {
                        for(int i = 2; i < keyValue.Length; i++)
                        {
                            keyValue[1] += "=" + keyValue[i];
                        }
                    }
                    if (keyValue.Length > 1)
                    {
                        string key = keyValue[0].ToLower();
                        string value = keyValue[1];

                        value = BackendManager.URLDecode(value);

                        //Debug.Log("Reading keys..");

                        if (key.Equals("userid"))
                        {
                            _output += "userid\n";
                            string[] usernames = value.Split(';');
                            _output += "value: " + usernames[0] + ", " + usernames.Length + "\n";
                            UserId = usernames[0];
                            XMPPBridge.Instance.UserID = UserId;
                            XMPPBridge.Instance.OtherUsers = usernames;
                        }
                        else if (key.Equals("groupid"))
                        {
                            GroupId = value;
                            XMPPBridge.Instance.GroupID = GroupId;
                        }
                        else if (key.Equals("challengeid"))
                        {
                            ChallengeId = value;
                            XMPPBridge.Instance.ChallengeID = ChallengeId;
                        }
                        else if (key.Equals("challengename"))
                        {
                            XMPPBridge.Instance.ChallengeName = value;
                        }
                        else if (key.Equals("role"))
                        {
                            IsTeacher = value.Equals("teacher");
                            if (IsTeacher)
                                XMPPBridge.Instance.RoleID = "teacher";
                            else
                                XMPPBridge.Instance.RoleID = "student";
                        }
                        else if (key.Equals("token"))
                            XMPPBridge.Instance.Token = value;
                        else if (key.Equals("testserver"))
                        {
                            XMPPBridge.Instance.IsTestServer = value.Equals("true");
                            Debug.Log("XMPPBridge.Instance.IsTestServer = " + XMPPBridge.Instance.IsTestServer);
                        }
                        else if (key.Equals("ptnodeid"))
                        {
                            XMPPBridge.Instance.PtNodeId = value;
                            Debug.Log("XMPPBridge.Instance.PtNodeId = " + XMPPBridge.Instance.PtNodeId);
                        }
                        else if (key.Equals("ptmap"))
                        {
                            XMPPBridge.Instance.PtMap = value;
                            Debug.Log("XMPPBridge.Instance.PtMap = " + XMPPBridge.Instance.PtMap);
                        }
                        else if (key.Equals("docid"))
                        {
                            XMPPBridge.Instance.DocId = value;
                            if (configParams.ContainsKey("startup_state_file") && configParams.ContainsKey("startup_force_local_file"))
                            {
                                if (configParams["startup_force_local_file"].Equals("true"))
                                {
                                    LoadBinaryDataFromFile(configParams["startup_state_file"]);
                                }
                                else
                                {
                                    LoadBinaryData(value);
                                }
                            }
                            else
                            {
                                LoadBinaryData(value);
                            }
                            MyGame.Instance.Output = "DocId read";
                        }
                    }
                }
            }
            else
            {
                DataContainer.Instance.SetDefault();
                LoadBinaryData("none");
            }
        }
        else
        {
            DataContainer.Instance.SetDefault();
            LoadBinaryData("none");
        }

        //_output = "PtMap: " + XMPPBridge.Instance.PtMap + "\n" + _output;
        /*
        _output += "IsTestServer: " + IsTestServer + "\n";
        _output += "PtNodeId: " + PtNodeId + "\n";
        */
        
#endif
        MyGame.Instance.Output = "XMPP: ForcedStart";
        XMPPBridge.Instance.ForcedStart();
    }

    public byte[] SerializeData()
    {
        DataContainer.Instance.Save();
        MemoryStream memStream = new MemoryStream();
        BinaryFormatter binFormatter = new BinaryFormatter();
        binFormatter.Serialize(memStream, DataContainer.Instance);
        return memStream.ToArray();
    }

    public void DeserializeData(byte[] data)
    {
        BinaryFormatter binFormatter = new BinaryFormatter();
        MemoryStream memStream = new MemoryStream(data);
        DataContainer.Instance = (DataContainer)(binFormatter.Deserialize(memStream));
        DataContainer.Instance.Load();

        PrevCannonPower = DataContainer.Instance.CannonPower;
        PrevCannonAngle = DataContainer.Instance.CannonAngle;
        CameraCounter = 0;
        SaveCounter = 0;
    }

    public void SaveBinaryData(string filename, bool automatic = false)
    {
        string finalFilename = "piki_" + filename + ".dat";
        byte[] data = SerializeData();
//#if UNITY_EDITOR
        finalFilename = "piki_" + XMPPBridge.DateTimeToMillisec(System.DateTime.UtcNow).ToString() + ".dat";
        
        WWWForm form = new WWWForm();
        form.AddBinaryData("fileUp", data, finalFilename);
        
        string url = (XMPPBridge.Instance.IsTestServer) ? server_url_test : server_url;

        if (!_uploadingFile)
        {
            _uploadingFile = true;
            _www = new WWW(url, form);
            StartCoroutine(WaitSavingData(automatic));
            if (!automatic)
                BackendManager.Instance.NumCalls++;
        }
/*#else
        FileStream stream = new FileStream(finalFilename, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(data);
        writer.Close();
        stream.Close();
        Debug.Log("Sending file to Metafora...");
        _showFilename = false;
        _showConfirmSave = false;
        _uploadingFile = false;
#endif*/
    }

    public void LoadBinaryData()
    {
        try
        {
            Debug.Log("contains: " + configParams.Count);
            string filename = configParams.ContainsKey("startup_state_file") ? configParams["startup_state_file"] : "piki_test.dat";
            FileStream stream = new FileStream(filename, FileMode.Open);
            DeserializeData(ReadFully(stream));
            stream.Close();
        }
        catch (FileNotFoundException e)
        {
            Debug.Log("MyGame.LoadBinaryData: " + e);
        }
        InitializeFromDataContainer();
    }

    public void LoadBinaryData(string state_id)
    {
        if (state_id.Equals("none"))
        {
            InitializeFromDataContainer();
            Interface.ProcedeAfterLoading();
        }
        else
        {
            string url = (XMPPBridge.Instance.IsTestServer) ? server_url_test : server_url;
            _www = new WWW(url + "?id=" + state_id);
            StartCoroutine(WaitBinaryData());
            BackendManager.Instance.NumCalls++;
        }
    }

    public void LoadBinaryDataFromFile(string filename)
    {
        _www = new WWW(filename);
        StartCoroutine(WaitBinaryData());
    }

    private byte[] ReadFully(FileStream stream)
    {
        byte[] buffer = new byte[32768];
        using (MemoryStream ms = new MemoryStream())
        {
            while (true)
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return ms.ToArray();
                ms.Write(buffer, 0, read);
            }
        }
    }
    #endregion

    #region Coroutines
    public IEnumerator WaitBinaryData()
    {
        yield return _www;
        if (_www.error != null)
            _output = _www.url + " - " + _www.error + "\n" + _output;
        else
        {
            DeserializeData(_www.bytes);
        }
        BackendManager.Instance.NumCalls--;

        InitializeFromDataContainer();
        Interface.ProcedeAfterLoading();
    }

    public IEnumerator WaitConfigData()
    {
        yield return _www;
        if (_www.error != null)
        {
            _output += _www.url + "\n" + _www.error;
        }
        else
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(new StringReader(_www.text));
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "property")
                        {
                            string key = reader.GetAttribute("key");
                            string value = reader.GetAttribute("value");
                            configParams.Add(key, value);
                        }
                    }
                }
                server_url = configParams.ContainsKey("server_uploadurl") ? configParams["server_uploadurl"] : "NO!!";
                server_url_test = configParams.ContainsKey("server_uploadurl_test") ? configParams["server_uploadurl_test"] : "NO!!";
            }
            catch (FileNotFoundException e)
            {
                Debug.Log("" + e);
                _output += "" + e;
            }
            LoadConfiguration();
        }
    }

    public bool isPublishing = false;
    public IEnumerator WaitSavingData(bool autosave)
    {
        yield return _www;
        if (_www.error != null)
        {
            Debug.Log("Error: " + _www.error);
            _output = "Error: " + _www.error;
        }
        else
        {
            _output = "Uploading OK";
            string[] splt = _www.text.Split(':');
            XMPPBridge.Instance.DocId = splt[splt.Length - 1].Trim();
            Debug.Log("Uploading OK " + XMPPBridge.Instance.DocId);
            lastDocId = XMPPBridge.Instance.DocId;

            if (isPublishing)
            {
                isPublishing = false;
                BackendManager.Instance.PublishScene(XMPPBridge.Instance.DocId, XMPPBridge.Instance.GroupID);
            }
            else
            {
                XMPPBridge.Instance.SendNodeUrlModification(autosave);
                isSceneSaved = true;
            }
        }
        //_showFilename = false;
        _uploadingFile = false;
        //_showConfirmSave = false;
        BackendManager.Instance.NumCalls--;
        //MyGame.Instance.OrthoCametaLocked = false;
    }
    #endregion

    #region Audio
    public bool isAudioOn = true;

    public void UpdateAudioState(bool isOn)
    {
        isAudioOn = isOn;
        AudioListener.volume = (isOn) ? 100.0f : 0.0f;
    }
    #endregion
}
