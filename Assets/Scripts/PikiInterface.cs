using System;
using System.Collections;
using System.Collections.Generic;
using pumpkin.display;
using pumpkin.events;
using pumpkin.text;
using SBS.Core;
using UnityEngine;


public class PikiInterface : FiniteStateMachine2
{
    #region Static Members
    static public int Void = -2;
    static public int Loading = -1;
    static public int SplashPage = 0;
    static public int ChooseModePage = 1;
    static public int ChooseCharacterPage = 2;
    static public int IngameEditDefencePage = 3;
    static public int IngameEditLaunchPage = 4;
    static public int IngameLaunchPage = 5;
    static public int LaunchRewardPage = 6;
    static public int DefenceRewardPage = 7;
    static public int NegativeRewardPage = 8;
    static public int ChooseScenePage = 9;
    static public int HighscorePage = 10;
    #endregion

    #region public Members

    public Texture[] cursors;
    public AudioClip posJingle;
    public AudioClip negJingle;

    #endregion

    #region Protected Members
    protected int cursorIndex = 0;
    protected Stage stage = null;
    protected MyGame game = null;
    protected FiniteStateMachine fsm = null;
    protected CannonComponent _cannonComp;
    protected BackendManager.PublishedScene selectedScene = null;

    protected AudioSource interfaceAudio;
    #endregion

    #region Get/Set
    public BackendManager.PublishedScene SelectedScene
    {
        get { return selectedScene; }
        set { selectedScene = value; }
    }

    public int CursorIndex
    {
        get
        {
            return cursorIndex;
        }
    }

    #endregion

    #region public Classes

    public class SceneData
    {
        int sceneId;
        string sceneName;
        string groupName;
        string data;
        int played;
        int won;
        int rating;

        public int SceneId
        {
            get { return sceneId; }
            set { sceneId = value; }
        }

        public string SceneName
        {
            get { return sceneName; }
            set { sceneName = value; }
        }

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }

        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        public int Played
        {
            get { return played; }
            set { played = value; }
        }

        public int Won
        {
            get { return won; }
            set { won = value; }
        }

        public int Rating
        {
            get { return rating; }
            set { rating = value; }
        }
    }

    #endregion

    #region Enums
    public enum CounterType
    {
        flag,
        palm,
        crates,
        clouds,
        block,
        trampoline
    }

    #endregion

    #region Unity Callbacks

    void OnGUI()
    {
#if !UNITY_EDITOR
        Screen.showCursor = false;
#endif
        if (cursorIndex >= 0)
            GUI.DrawTexture(new Rect(Input.mousePosition.x - 5, Screen.height - Input.mousePosition.y, 32, 32), cursors[cursorIndex]);
    }

    #endregion

    void StartInterface()
    {
        MovieClipOverlayCameraBehaviour.overlayCameraName = "UICamera";
        stage = MovieClipOverlayCameraBehaviour.instance.stage;

        interfaceAudio = gameObject.AddComponent<AudioSource>();
        interfaceAudio.volume = 1.0f;
        interfaceAudio.loop = false;
        interfaceAudio.playOnAwake = false;

        game = GameObject.Find("MasterGameObject").GetComponent<MyGame>();
        fsm = game.FSM;

        FiniteStateMachine2.FSMObject2.Function voidFunc = (self, time) => { };
        fsmObject.AddState(Void, voidFunc, voidFunc, voidFunc);
        fsmObject.AddState(Loading, OnLoadingEnter, OnLoadingExec, OnLoadingExit);
        fsmObject.AddState(SplashPage, OnSplashEnter, OnSplashExec, OnSplashExit);
        fsmObject.AddState(IngameEditDefencePage, OnIngameEditDefenceEnter, OnIngameEditDefenceExec, OnIngameEditDefenceExit);
        fsmObject.AddState(IngameEditLaunchPage, OnIngameEditLaunchEnter, OnIngameEditLaunchExec, OnIngameEditLaunchExit);
        fsmObject.AddState(IngameLaunchPage, OnIngameLaunchEnter, OnIngameLaunchExec, OnIngameLaunchExit);
        fsmObject.AddState(ChooseModePage, OnChooseModeEnter, OnChooseModeExec, OnChooseModeExit);
        fsmObject.AddState(ChooseScenePage, OnChooseSceneEnter, OnChooseSceneExec, OnChooseSceneExit);
        fsmObject.AddState(HighscorePage, OnHighscoreEnter, OnHighscoreExec, OnHighscoreExit);
        fsmObject.AddState(ChooseCharacterPage, OnChooseCharacterEnter, OnChooseCharacterExec, OnChooseCharacterExit);
        fsmObject.AddState(LaunchRewardPage, OnLaunchRewardEnter, OnLaunchRewardExec, OnLaunchRewardExit);
        fsmObject.AddState(DefenceRewardPage, OnDefenceRewardEnter, OnDefenceRewardExec, OnDefenceRewardExit);
        fsmObject.AddState(NegativeRewardPage, OnNegativeRewardEnter, OnNegativeRewardExec, OnNegativeRewardExit);

        //this.State = ChooseScenePage;
    }

    #region Tutorial
    protected bool haveToShowTutorial = false;
    protected MovieClip tutorialPopup = null;
    protected MovieClip btTutorialContinue = null;
    
    public void ShowTutorial()
    {
        if (haveToShowTutorial)
        {
            StartCoroutine(TutorialCoroutine());
        }
    }

    public void HideTutorial()
    {
        stage.removeChild(tutorialPopup);
        tutorialPopup = null;
        btTutorialContinue = null;
        MyGame.Instance.OrthoCametaLocked = false;
    }

    public IEnumerator TutorialCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("SHOW TUTORIAL");
        haveToShowTutorial = false;

        tutorialPopup = new MovieClip("Flash/piki.swf:mcPopupTutorialClass");
        stage.addChild(tutorialPopup);
        tutorialPopup.x = 400.0f;
        tutorialPopup.y = 300.0f;

        if (DataContainer.Instance.playMode == DataContainer.PlayMode.Defence)
            tutorialPopup.gotoAndStop(1);
        else
            tutorialPopup.gotoAndStop(2);

        btTutorialContinue = tutorialPopup.getChildByName<MovieClip>("btContinue");
        SetupButton(btTutorialContinue, OnTutorialContinue, "Continue");

        MyGame.Instance.OrthoCametaLocked = true;
    }

    void OnTutorialContinue(CEvent evt)
    {
        HideTutorial();
    }
    #endregion

    #region States
    #region Loading State
    protected TextField tfLoadingText;
    void OnLoadingEnter(FSMObject2 self, float time)
    {
        Debug.Log("LOADING Enter");

        MovieClipOverlayCameraBehaviour.overlayCameraName = "UICamera";
        stage = MovieClipOverlayCameraBehaviour.instance.stage;

        mcChooseModePage = new MovieClip("Flash/piki.swf:mcChooseModePageClass");
        mcChooseModePage.gotoAndStop("Loading");
        tfLoadingText = mcChooseModePage.getChildByName<MovieClip>("mcLoadingText").getChildByName<TextField>("tfLabel");
        tfLoadingText.text = "Loading...";
        stage.addChild(mcChooseModePage);
    }

    void OnLoadingExec(FSMObject2 self, float time)
    { }

    void OnLoadingExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcChooseModePage);
        mcChooseModePage = null;
        tfLoadingText = null;
    }

    public void SetLoadingMessage(string message)
    {
        if (null != tfLoadingText)
            tfLoadingText.text = message;
    }
    #endregion

    #region Splash State
    protected MovieClip btStart;
    void OnSplashEnter(FSMObject2 self, float time)
    {
        Debug.Log("STARTPAGE Enter");

        mcChooseModePage = new MovieClip("Flash/piki.swf:mcChooseModePageClass");
        mcChooseModePage.gotoAndStop("Start");
        stage.addChild(mcChooseModePage);
        btStart = mcChooseModePage.getChildByName<MovieClip>("btStart");
        SetupButton(btStart, OnModeClick, "Start");
    }

    void OnSplashExec(FSMObject2 self, float time)
    {

        //mcChooseModePage.gotoAndStop("Start");
    }

    void OnSplashExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcChooseModePage);
        mcChooseModePage = null;
        btStart = null;        
    }
    #endregion

    #region ChooseMode State
    protected MovieClip mcChooseModePage = null;
    protected MovieClip btLaunch = null;
    protected MovieClip btDefence = null;
    protected TextField tfModeText = null;

    void OnChooseModeEnter(FSMObject2 self, float time)
    {
        Debug.Log("CHOOSEMODE Enter");

        mcChooseModePage = new MovieClip("Flash/piki.swf:mcChooseModePageClass");
        mcChooseModePage.gotoAndStop("ChooseMode");
        stage.addChild(mcChooseModePage);
        tfModeText = mcChooseModePage.getChildByName<MovieClip>("mcGameMode").getChildByName<TextField>("tfLabel");
        btLaunch = mcChooseModePage.getChildByName<MovieClip>("btLaunch");
        btDefence = mcChooseModePage.getChildByName<MovieClip>("btDefence");
        SetupButton(btLaunch, OnModeClick, "Launch");
        SetupButton(btDefence, OnModeClick, "Defence");
        tfModeText.text = "Choose game mode...";
    }

    void OnChooseModeExec(FSMObject2 self, float time)
    { }

    void OnChooseModeExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcChooseModePage);
        mcChooseModePage = null;
        btLaunch = null;
        btDefence = null;
        tfModeText = null;
    }
    #endregion

    #region ChooseScene State
    protected float sceneListPosX = 84.0f;
    protected float sceneListPosY = 180;
    protected float indexScenesSpace = 30.0f;

    protected MovieClip mcChooseScenePage = null;
    protected MovieClip btContinue = null;
    protected MovieClip btLeft = null;
    protected MovieClip btRight = null;
    protected TextField tfPageLabel = null;

    protected MovieClip btName = null;
    protected MovieClip btOwner = null;
    protected MovieClip btDate = null;
    protected MovieClip btPlayed = null;
    protected MovieClip btWon = null;
    protected MovieClip btRating = null;

    //protected SceneData[] totScenesArr = null;
    protected Dictionary<MovieClip, BackendManager.PublishedScene> sceneByMC = new Dictionary<MovieClip, BackendManager.PublishedScene>();
    protected MovieClip[] scenesRows = null;
    protected int sceneArrIndex = 0;
    protected int currentPage = 0;
    protected int totalPageNum = 0;

    protected const int RowsPerPage = 10;
    protected string filter = "rating";
    protected string order = "DESC";

    protected bool connectionError = false;
    protected TextField tfErrorMessage = null;

    void OnChooseSceneEnter(FSMObject2 self, float time)
    {
        mcChooseScenePage = new MovieClip("Flash/piki.swf:mcChooseScenePageClass");
        stage.addChild(mcChooseScenePage);

        InitNavBar(mcChooseScenePage);

        btContinue = mcChooseScenePage.getChildByName<MovieClip>("btContinue");
        btLeft = mcChooseScenePage.getChildByName<MovieClip>("btLeft");
        btRight = mcChooseScenePage.getChildByName<MovieClip>("btRight");
        SetupButton(btSoundOn, OnChooseSceneButtonsClick, "", "soundon");
        SetupButton(btContinue, OnChooseSceneButtonsClick, "Continue", "");
        GhostButton(btContinue, "Continue");
        //SetupButton(btLeft, OnChooseSceneButtonsClick, "", "");
        GhostButton(btLeft);
        SetupButton(btRight, OnChooseSceneButtonsClick, "", "");

        currentPage = 1;
        tfPageLabel = mcChooseScenePage.getChildByName<TextField>("tfPageLabel");
        UpdatePageString(currentPage.ToString(), totalPageNum.ToString());

        //TEMP
        scenesRows = new MovieClip[RowsPerPage];
        //CreateFakeList();
        //totScenesArr = FakeList.ToArray();
        //CreateSceneList(new List<SceneData>());

        BackendManager.Instance.GetScenesCount();
        BackendManager.Instance.GetPublishedScenes(1, RowsPerPage, filter, order);

        btName = mcChooseScenePage.getChildByName<MovieClip>("btName");
        btOwner = mcChooseScenePage.getChildByName<MovieClip>("btOwner");
        btDate = mcChooseScenePage.getChildByName<MovieClip>("btDate");
        btPlayed = mcChooseScenePage.getChildByName<MovieClip>("btPlayed");
        btWon = mcChooseScenePage.getChildByName<MovieClip>("btWon");
        btRating = mcChooseScenePage.getChildByName<MovieClip>("btRating");
        UpdateSceneListHeader();

        connectionError = false;
        tfErrorMessage = mcChooseScenePage.getChildByName<TextField>("tfErrorMessage");
        tfErrorMessage.visible = false;
    }

    void OnChooseSceneExec(FSMObject2 self, float time)
    { }

    void OnChooseSceneExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcChooseScenePage);
        mcChooseScenePage = null;
        btSoundOn = null;
        btContinue = null;
        btLeft = null;
        btRight = null;
        btName = null;
        btOwner = null;
        btDate = null;
        btPlayed = null;
        btWon = null;
        btRating = null;

        tfErrorMessage = null;
    }

    public void ShowConnectionError()
    {
        if (State == ChooseScenePage)
        {
            SetupButton(btContinue, OnChooseSceneButtonsClick, "Back", "");
        }
        else
            btHighscoreContinue.getChildByName<TextField>("tfLabel").text = "Back";
        connectionError = true;
        tfErrorMessage.visible = true;
        tfErrorMessage.text = "An error occured while retrieving the list of scenes. Please check your internet connection and launch the game again.";
    }

    public void HideConnectionError()
    {
        tfErrorMessage.visible = false;
    }

    void UpdateSceneListHeader()
    {
        SetupButton(btName, OnSceneListHeadeClick);
        SetupButton(btOwner, OnSceneListHeadeClick);
        SetupButton(btDate, OnSceneListHeadeClick);
        SetupButton(btPlayed, OnSceneListHeadeClick);
        SetupButton(btWon, OnSceneListHeadeClick);
        SetupButton(btRating, OnSceneListHeadeClick);

        btName.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        btOwner.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        btDate.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        btPlayed.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        btWon.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        btRating.getChildByName<MovieClip>("mcArrow").gotoAndStop("none");
        
        switch (filter)
        {
            case "name":
                btName.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
            case "owner":
                btOwner.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
            case "publish_date":
                btDate.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
            case "match_played":
                btPlayed.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
            case "match_won":
                btWon.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
            case "rating":
                btRating.getChildByName<MovieClip>("mcArrow").gotoAndStop(order);
                break;
        }
    }

    void OnSceneListHeadeClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        string localFilter = string.Empty;
        switch (mc.name)
        {
            case "btName":
                localFilter = "name";
                break;
            case "btOwner":
                localFilter = "owner";
                break;
            case "btDate":
                localFilter = "publish_date";
                break;
            case "btPlayed":
                localFilter = "match_played";
                break;
            case "btWon":
                localFilter = "match_won";
                break;
            case "btRating":
                localFilter = "rating";
                break;
        }

        if (filter.Equals(localFilter))
        {
            if (order.Equals("ASC"))
                order = "DESC";
            else
                order = "ASC";
        }
        else
        {
            filter = localFilter;
            order = "ASC";
        }

        currentPage = 1;
        hsCurrentPage = 1;
        BackendManager.Instance.GetPublishedScenes(currentPage, RowsPerPage, filter, order);
        UpdateSceneListHeader();
    }

    void OnChooseSceneButtonsClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        switch (mc.name)
        {
            case "btContinue":
                if (connectionError)
                {
                    this.State = ChooseModePage;
                }
                else
                {
                    if (null == selectedScene)
                        return;
                    MyGame.Instance.LoadBinaryData(selectedScene.docid);
                    BackendManager.Instance.VisitScene(selectedScene.id);
                }
                break;

            case "btRight":
                if (currentPage < totalPageNum)
                {
                    currentPage++;
                    selectedScene = null;
                    BackendManager.Instance.GetPublishedScenes(currentPage, RowsPerPage, filter, order);
                    if (currentPage == totalPageNum)
                        GhostButton(btRight);

                    if (btLeft.currentLabel == "gh")
                        SetupButton(btLeft, OnChooseSceneButtonsClick);

                    GhostButton(btContinue, "Continue");

                }
                break;
            case "btLeft":
                if (currentPage > 1)
                {
                    currentPage--;
                    selectedScene = null;
                    BackendManager.Instance.GetPublishedScenes(currentPage, RowsPerPage, filter, order);
                    if (currentPage == 1)
                        GhostButton(btLeft);

                    if (btRight.currentLabel == "gh")
                        SetupButton(btRight, OnChooseSceneButtonsClick);

                    GhostButton(btContinue, "Continue");
                }
                break;
            case "btSoundOn":
                MyGame.Instance.UpdateAudioState(!MyGame.Instance.isAudioOn);
                MovieClip mcIcon = mc.getChildByName<MovieClip>("mcIcon");
                mcIcon.gotoAndStop(!MyGame.Instance.isAudioOn ? "soundoff" : "soundon");
                break;
        }
        
    }

    void OnScenesClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        for (int i = 0; i < scenesRows.Length; i++)
        {
            if (null != scenesRows[i])
            {
                scenesRows[i].gotoAndStop(1);
                if (scenesRows[i] == (evt.currentTarget as MovieClip))
                {
                    scenesRows[i].gotoAndStop(2);
                    selectedScene = sceneByMC[mc];
                }
                else
                    scenesRows[i].gotoAndStop(1);
            }
        }
        if (btContinue.currentLabel == "gh")
            SetupButton(btContinue, OnChooseSceneButtonsClick, "Continue");
    }

    protected void UpdatePageString(string page, string total)
    {
        tfPageLabel.text = page + "/" + total;
    }

    public void SetTotalPageNum(int value)
    {
        totalPageNum = Mathf.FloorToInt(value / RowsPerPage) + 1;
        UpdatePageString(currentPage.ToString(), totalPageNum.ToString());
    }

    public void ProcedeAfterLoading()
    {
        if (this.State == ChooseScenePage)
        {
            this.State = ChooseCharacterPage;
            GameCamera.Instance.gameObject.animation.Play();
            return;
        }

        if (DataContainer.Instance.sceneState == DataContainer.SceneState.Published)
        {
            //ShowFinishPopup("Well Done!");
            this.State = DefenceRewardPage;
        }
        else if (DataContainer.Instance.sceneState == DataContainer.SceneState.Finished || DataContainer.Instance.sceneState == DataContainer.SceneState.Rated)
        {
            Debug.Log("ProcedeAfterLoading: " + DataContainer.Instance.sceneState);
            CheckRewardInterface(true);
        }
        else
        {
            if (DataContainer.Instance.playMode == DataContainer.PlayMode.None)
                this.State = ChooseModePage;
        }
    }

    public void CreateSceneList(List<BackendManager.PublishedScene> scenes)
    {
        foreach (KeyValuePair<MovieClip, BackendManager.PublishedScene> item in sceneByMC)
            mcChooseScenePage.removeChild(item.Key);
        sceneByMC.Clear();

        int count = Math.Min(scenes.Count, RowsPerPage);
        for (int i = 0; i < count; i++)
        {
            int rank = ((currentPage - 1) * RowsPerPage) + (i + 1);
            scenesRows[i] = AddSceneRow(mcChooseScenePage, scenes[i], sceneListPosX, sceneListPosY + indexScenesSpace * i, rank);
            sceneByMC.Add(scenesRows[i], scenes[i]);
        }
        UpdatePageString(currentPage.ToString(), totalPageNum.ToString());
    }

    MovieClip AddSceneRow(MovieClip parent, BackendManager.PublishedScene pScene, float sX, float sY, int rank)
    {
        MovieClip scene = new MovieClip("Flash/piki.swf:mcSceneRowClass");
        parent.addChild(scene);
        scene.x = sX;
        scene.y = sY;
        scene.getChildByName<TextField>("tfNumber").text = rank.ToString();
        scene.getChildByName<TextField>("tfName").text = pScene.name;
        scene.getChildByName<TextField>("tfOwner").text = pScene.owner;
        scene.getChildByName<TextField>("tfDate").text = pScene.publish_date;
        scene.getChildByName<TextField>("tfPlayed").text = pScene.match_played.ToString();
        scene.getChildByName<TextField>("tfWon").text = pScene.match_won.ToString();
        scene.getChildByName<MovieClip>("mcRate").gotoAndStop(pScene.rating + 1);

        scene.addEventListener(MouseEvent.CLICK, OnScenesClick);
        return scene;
    }
    
    #endregion

    #region Highscore State
    protected MovieClip mcHighscorePage = null;
    protected MovieClip btHighscoreContinue = null;
    protected MovieClip btHighscoreLeft = null;
    protected MovieClip btHighscoreRight = null;
    protected TextField tfHighscorePageLabel = null;

    protected int hsCurrentPage = 0;
    protected int hsTotalPageNum = 0;
    protected int prevState = IngameLaunchPage;
    protected bool highscoreInitialized = false;

    void OnHighscoreEnter(FSMObject2 self, float time)
    {
        highscoreInitialized = false;
        mcHighscorePage = new MovieClip("Flash/piki.swf:mcChooseScenePageClass");
        stage.addChild(mcHighscorePage);

        InitNavBar(mcHighscorePage);

        btHighscoreContinue = mcHighscorePage.getChildByName<MovieClip>("btContinue");
        btHighscoreLeft = mcHighscorePage.getChildByName<MovieClip>("btLeft");
        btHighscoreRight = mcHighscorePage.getChildByName<MovieClip>("btRight");
        SetupButton(btSoundOn, OnHighscoreButtonsClick, "", "soundon");
        SetupButton(btHighscoreContinue, OnHighscoreButtonsClick, "Back", "");
        //SetupButton(btHighscoreLeft, OnHighscoreButtonsClick, "", "");
        GhostButton(btHighscoreLeft);
        SetupButton(btHighscoreRight, OnHighscoreButtonsClick, "", "");

        hsCurrentPage = 1;
        tfHighscorePageLabel = mcHighscorePage.getChildByName<TextField>("tfPageLabel");
        UpdateHighscorePageString(hsCurrentPage.ToString(), hsTotalPageNum.ToString());


        scenesRows = new MovieClip[RowsPerPage];
        BackendManager.Instance.GetScenesCount();
        BackendManager.Instance.GetPublishedScenes(1, RowsPerPage);

        btName = mcHighscorePage.getChildByName<MovieClip>("btName");
        btOwner = mcHighscorePage.getChildByName<MovieClip>("btOwner");
        btDate = mcHighscorePage.getChildByName<MovieClip>("btDate");
        btPlayed = mcHighscorePage.getChildByName<MovieClip>("btPlayed");
        btWon = mcHighscorePage.getChildByName<MovieClip>("btWon");
        btRating = mcHighscorePage.getChildByName<MovieClip>("btRating");
        UpdateSceneListHeader();

        connectionError = false;
        tfErrorMessage = mcHighscorePage.getChildByName<TextField>("tfErrorMessage");
        tfErrorMessage.visible = false;      
    }

    void OnHighscoreExec(FSMObject2 self, float time)
    { }

    void OnHighscoreExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcHighscorePage);
        mcHighscorePage = null;
        btSoundOn = null;
        btHighscoreContinue = null;
        btHighscoreLeft = null;
        btHighscoreRight = null;

        tfErrorMessage = null;
    }

    protected void UpdateHighscorePageString(string page, string total)
    {
        tfHighscorePageLabel.text = page + "/" + total;
    }

    void OnHighscoreButtonsClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        switch (mc.name)
        {
            case "btContinue":
                    State = prevState;
                break;

            case "btRight":
                if (hsCurrentPage < hsTotalPageNum)
                {
                    hsCurrentPage++;
                    selectedScene = null;
                    BackendManager.Instance.GetPublishedScenes(hsCurrentPage, RowsPerPage, filter, order);

                    if (currentPage == hsTotalPageNum)
                        GhostButton(btHighscoreRight);

                    if (btHighscoreLeft.currentLabel == "gh")
                        SetupButton(btHighscoreLeft, OnHighscoreButtonsClick);

                    GhostButton(btHighscoreContinue, "Back");
                    highscoreInitialized = false;
                }
                break;
            case "btLeft":
                if (hsCurrentPage > 1)
                {
                    hsCurrentPage--;
                    selectedScene = null;
                    BackendManager.Instance.GetPublishedScenes(hsCurrentPage, RowsPerPage, filter, order);

                    if (currentPage == 1)
                        GhostButton(btHighscoreLeft);

                    if (btHighscoreRight.currentLabel == "gh")
                        SetupButton(btHighscoreRight, OnHighscoreButtonsClick);

                    GhostButton(btHighscoreContinue, "Back");
                    highscoreInitialized = false;
                }
                break;
            case "btSoundOn":
                MyGame.Instance.UpdateAudioState(!MyGame.Instance.isAudioOn);
                MovieClip mcIcon = mc.getChildByName<MovieClip>("mcIcon");
                mcIcon.gotoAndStop(!MyGame.Instance.isAudioOn ? "soundoff" : "soundon");
                break;
                
        }

    }

    public void SetTotalPageHighscoreNum(int value)
    {
        hsTotalPageNum = Mathf.FloorToInt(value / RowsPerPage) + 1;
        UpdateHighscorePageString(hsCurrentPage.ToString(), hsTotalPageNum.ToString());
    }

    public void CreateHighscoreList(List<BackendManager.PublishedScene> scenes)
    {
        foreach (KeyValuePair<MovieClip, BackendManager.PublishedScene> item in sceneByMC)
            mcHighscorePage.removeChild(item.Key);
        sceneByMC.Clear();

        int count = Math.Min(scenes.Count, RowsPerPage);
        for (int i = 0; i < count; i++)
        {
            int rank = ((hsCurrentPage - 1) * RowsPerPage) + (i + 1);
            scenesRows[i] = AddHighscoreRow(mcHighscorePage, scenes[i], sceneListPosX, sceneListPosY + indexScenesSpace * i, rank);
            sceneByMC.Add(scenesRows[i], scenes[i]);
        }
        UpdateHighscorePageString(hsCurrentPage.ToString(), hsTotalPageNum.ToString());
    }

    MovieClip AddHighscoreRow(MovieClip parent, BackendManager.PublishedScene pScene, float sX, float sY, int rank)
    {
        MovieClip scene = new MovieClip("Flash/piki.swf:mcSceneRowClass");
        parent.addChild(scene);
        scene.x = sX;
        scene.y = sY;
        scene.getChildByName<TextField>("tfNumber").text = rank.ToString();
        scene.getChildByName<TextField>("tfName").text = pScene.name;
        scene.getChildByName<TextField>("tfOwner").text = pScene.owner;
        scene.getChildByName<TextField>("tfDate").text = pScene.publish_date;
        scene.getChildByName<TextField>("tfPlayed").text = pScene.match_played.ToString();
        scene.getChildByName<TextField>("tfWon").text = pScene.match_won.ToString();
        scene.getChildByName<MovieClip>("mcRate").gotoAndStop(pScene.rating + 1);
        return scene;
    }
    #endregion

    #region ChooseCharacter State
    protected TextField tfCharacterText;
    void OnChooseCharacterEnter(FSMObject2 self, float time)
    {
        Debug.Log("CHOOSECHARACTER Enter");

        mcChooseModePage = new MovieClip("Flash/piki.swf:mcChooseModePageClass");
        mcChooseModePage.gotoAndStop("ChooseCharacter");
        stage.addChild(mcChooseModePage);
        tfCharacterText = mcChooseModePage.getChildByName<MovieClip>("mcChoosingText").getChildByName<TextField>("tfLabel");
        tfCharacterText.text = "Choose character...";

        DataContainer.Instance.SelectedSceneId = selectedScene.id;
        DataContainer.Instance.sceneState = DataContainer.SceneState.Opened;
    }

    void OnChooseCharacterExec(FSMObject2 self, float time)
    { }

    void OnChooseCharacterExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcChooseModePage);
        mcChooseModePage = null;
        tfCharacterText = null;
    }
    #endregion

    #region IngameEditDefence States
    #region Ingame Members
    protected MovieClip mcEditMode = null;
    protected MovieClip mcPopupRules = null;
    protected MovieClip mcPopupTutorial = null;
    protected MovieClip btGoToCannon = null;
    protected MovieClip btPublish = null;
    protected MovieClip mcPopupHelp = null;

    protected MovieClip mcPopupPublish = null;
    protected MovieClip btPublishOk = null;
    protected MovieClip btPublishCancel = null;
    protected MovieClip mcPopupShare = null;
    protected MovieClip btShareOk = null;
    protected MovieClip btShareCancel = null;

    protected MovieClip btCameraSide = null;
    protected MovieClip btCameraBack = null;
    protected MovieClip btCameraTop = null;

    protected MovieClip btHelp = null;
    protected MovieClip btSoundOn = null;
    protected MovieClip btSoundOff = null;
    protected MovieClip btHiscore = null;
    protected MovieClip btLasad = null;
    protected MovieClip btSave = null;

    protected MovieClip btPointer = null;
    protected MovieClip btFlag = null;
    protected MovieClip btPalm = null;
    protected MovieClip btCrates = null;
    protected MovieClip btBlock = null;
    protected MovieClip btTrampoline = null;

    protected MovieClip mcFlagNumber = null;
    protected MovieClip mcPalmNumber = null;
    protected MovieClip mcCratesNumber = null;
    protected MovieClip mcCloudsNumber = null;
    protected MovieClip mcBlockNumber = null;
    protected MovieClip mcTrampolineNumber = null;

    protected TextField tfScoreLabel = null;
    protected TextField tfScoreValue = null;
    protected TextField tfStatusLabel = null;

    protected MovieClip mcSvolaz = null;
    #endregion

    void OnIngameEditDefenceEnter(FSMObject2 self, float time)
    {
        Debug.Log("INGAMEEDITDEFENCE Enter");
        prevState = IngameEditDefencePage;

        mcEditMode = new MovieClip("Flash/piki.swf:mcEditModeClass");
        mcEditMode.gotoAndStop("Defence");
        stage.addChild(mcEditMode);
        
        btGoToCannon = mcEditMode.getChildByName<MovieClip>("btGoToCannon");
        SetupButton(btGoToCannon, OnSwitchLaunchEditButton, "Test Scene");

        InitCamerasButtons(mcEditMode);
        InitNavBar(mcEditMode); 
        InitManagementButtons(mcEditMode);

        btPointer = mcEditMode.getChildByName<MovieClip>("btPointer");
        btFlag = mcEditMode.getChildByName<MovieClip>("btFlag");
        btPalm = mcEditMode.getChildByName<MovieClip>("btPalm");
        btCrates = mcEditMode.getChildByName<MovieClip>("btCrates");
        btPublish = mcEditMode.getChildByName<MovieClip>("btPublish");
        SetupButton(btPointer, OnEditClick, "", "pointer");
        SetupButton(btFlag, OnEditClick, "", "flag");
        SetupButton(btPalm, OnEditClick, "", "palm");
        SetupButton(btCrates, OnEditClick, "", "crates");
        SetupButton(btPublish, OnBtPublishClick);

        mcFlagNumber = btFlag.getChildByName<MovieClip>("mcNumber");
        mcPalmNumber = btPalm.getChildByName<MovieClip>("mcNumber");
        mcCratesNumber = btCrates.getChildByName<MovieClip>("mcNumber");

        tfStatusLabel = mcEditMode.getChildByName<TextField>("tfStatusLabel");
        tfScoreLabel = mcEditMode.getChildByName<TextField>("tfScoreLabel");
        tfScoreValue = mcEditMode.getChildByName<TextField>("tfScoreValue");
        tfScoreLabel.text = "Score";
        tfScoreValue.text = "0";

        tfScoreLabel.visible = false;
        tfScoreValue.visible = false;

        tfStatusLabel.text = string.Empty;
        
        mcPopupPublish = mcEditMode.getChildByName<MovieClip>("mcPopupPublish");
        btPublishOk = mcPopupPublish.getChildByName<MovieClip>("btOk");
        btPublishCancel = mcPopupPublish.getChildByName<MovieClip>("btCancel");
        SetupButton(btPublishOk, OnBtOkPublishPopupClick);
        SetupButton(btPublishCancel, OnBtCancelPublishPopupClick);
        ShowHidePublishPopup(false);

        mcPopupShare = mcEditMode.getChildByName<MovieClip>("mcPopupShare");
        btShareOk = mcPopupShare.getChildByName<MovieClip>("btOk");
        btShareCancel = mcPopupShare.getChildByName<MovieClip>("btCancel");
        SetupButton(btShareCancel, OnBtCancelSavePopupClick);
        ShowHideSavePopup(false);

        mcCheckGrid = mcEditMode.getChildByName<MovieClip>("mcCheckGrid");
        mcCheckGrid.addEventListener(MouseEvent.CLICK, OnCheckGridClick);
        mcCheckGrid.gotoAndStop("uncheck");

        UpdateCounter(CounterType.flag, DataContainer.Instance.FlagsNum);
        UpdateCounter(CounterType.crates, DataContainer.Instance.BoxesNum);
        UpdateCounter(CounterType.palm, DataContainer.Instance.PalmsNum);
        UpdateCounter(CounterType.clouds, DataContainer.Instance.CloudsNum);

        mcPopupHelp = mcEditMode.getChildByName<MovieClip>("mcPopupHelp");
        btPublishOk = mcPopupHelp.getChildByName<MovieClip>("btOk");
        SetupButton(btPublishOk, OnBtHelpCallback);
        mcPopupHelp.visible = false;

        ShowTutorial();

    }

    void OnIngameEditDefenceExec(FSMObject2 self, float time)
    {
        UpdateStatusLabel();
    }

    void OnIngameEditDefenceExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcEditMode);
        mcEditMode = null;
        btPointer = null;
        btFlag = null;
        btPalm = null;
        btCrates = null;
        btPublish = null;
        mcPopupHelp = null;
        btPublishOk = null;
    }

    #region IngameEdit Functions
    public void UpdateStatusLabel()
    {
        if (null != tfStatusLabel)
        {
            if (MyGame.Instance.isSceneSaved)
                tfStatusLabel.text = "Scene saved";
            else
                tfStatusLabel.text = "Saving...";
        }
    }
    
    public void UpdateCounter(CounterType cType, int cNumber)
    {
        if (State != IngameEditDefencePage)
            return;

        switch (cType)
        {
            case CounterType.flag:
                mcFlagNumber.gotoAndStop(1);
                mcFlagNumber.getChildByName<TextField>("tfLabel").text = cNumber.ToString();
                break;
            case CounterType.palm:
                mcPalmNumber.gotoAndStop(1);
                mcPalmNumber.getChildByName<TextField>("tfLabel").text = cNumber.ToString();
                break;
            case CounterType.crates:
                mcCratesNumber.gotoAndStop(1);
                mcCratesNumber.getChildByName<TextField>("tfLabel").text = cNumber.ToString();
                break;
        }
    }

    void ShowPopupRules()
    {
        mcPopupRules.visible = true;
    }
    #endregion

    void RemoveClickListener(MovieClip m)
    {
        m.removeAllEventListeners(MouseEvent.CLICK);
    }

    void OnBtOkSharePopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        ShowHideSavePopup(false);

        XMPPBridge.Instance.SendReferableObject("LASAD");
    }

    void OnBtOkSavePopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        ShowHideSavePopup(false);

        MyGame.Instance.saveCounter++;
        MyGame.Instance.isSceneSaved = false;
        MyGame.Instance.SaveBinaryData("");
    }

    void OnBtCancelSavePopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        ShowHideSavePopup(false);
    }

    void ShowHideSavePopup(bool vis)
    {
        mcPopupShare.visible = vis;
        MyGame.Instance.OrthoCametaLocked = vis;
        if (!vis)
            RemoveClickListener(btShareOk);
    }

    void OnBtPublishClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        ShowHidePublishPopup(true);
    }

    void OnBtOkPublishPopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        string sceneName =  mcPopupPublish.getChildByName<TextField>("tfInputSceneName").text;
        if (sceneName.Length == 0)
        {
            ShowPublishPopupMessage("You must specify the scene's name");
        }
        else
        {
            MyGame.Instance.isPublishing = true;
            BackendManager.Instance.CheckSceneName(sceneName);
        }    
    }

    void OnBtCancelPublishPopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        ShowHidePublishPopup(false);
    }

    public void ShowHidePublishPopup(bool vis)
    {
        MyGame.Instance.OrthoCametaLocked = vis;
        mcPopupPublish.getChildByName<TextField>("tfInputSceneName").text = "";
        mcPopupPublish.visible = vis;
        mcPopupPublish.gotoAndStop(2);
        mcPopupPublish.getChildByName<TextField>("tfMessage").text = string.Empty;
    }

    public void ShowPublishPopupMessage(string message)
    {
        mcPopupPublish.getChildByName<TextField>("tfMessage").text = message;
    }

    void OnBtHelpCallback(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        mcPopupHelp.visible = false;
    }

    public void ShowFinishPopup(string _text)
    {
        interfaceAudio.PlayOneShot(_text == "Game Over" ? negJingle : posJingle);
        mcSvolaz = new MovieClip("Flash/piki.swf:mcSvolazClass");
        mcSvolaz.gotoAndStop(1);

        mcSvolaz.getChildByName<MovieClip>("mcSvolText").getChildByName<TextField>("tfLabel").text = _text;
        mcSvolaz.x = Screen.width * 0.5f;
        mcSvolaz.y = 300;
        mcSvolaz.play();
        stage.addChild(mcSvolaz);
        mcSvolaz.addFrameScript("end", OnHideFinishPopup);
    }

    void OnHideFinishPopup(CEvent evt)
    {
        (evt.currentTarget as MovieClip).visible = false;

        this.State = NextState;

        /*
        if ((this.State == IngameLaunchPage || this.State == IngameEditLaunchPage) && ((evt.currentTarget as MovieClip).getChildByName<MovieClip>("mcSvolText").getChildByName<TextField>("tfLabel").text == "Well Done!"))
            this.State = LaunchRewardPage;
        else if (this.State == IngameEditDefencePage)
            this.State = DefenceRewardPage;
        else
            this.State = NegativeRewardPage;
        */        
        
        stage.removeChild((evt.currentTarget as MovieClip));
    }

    
    #endregion

    #region IngameEditLaunch State
    protected MovieClip mcCheckGrid = null;
    protected MovieClip btRulesPopupOk = null;
    protected MovieClip parentSelected = null;
    int selectedRuleTextfield;

    void OnIngameEditLaunchEnter(FSMObject2 self, float time)
    {
        Debug.Log("INGAMEEDITLAUNCH Enter");
        prevState = IngameEditLaunchPage;

        mcEditMode = new MovieClip("Flash/piki.swf:mcEditModeClass");
        mcEditMode.gotoAndStop(2);
        stage.addChild(mcEditMode);

        mcPopupRules = mcEditMode.getChildByName<MovieClip>("mcPopupRules");
        InitMaterialRulesPopup();
        ShowMaterialRulesPopup(MaterialManager.TRAMPOLINE); //HideMaterialRulesPopup();

        btGoToCannon = mcEditMode.getChildByName<MovieClip>("btGoToCannon");
        SetupButton(btGoToCannon, OnSwitchLaunchEditButton, "Go to Cannon");

        InitCamerasButtons(mcEditMode);
        InitNavBar(mcEditMode);
        InitManagementButtons(mcEditMode);
        
        btPointer = mcEditMode.getChildByName<MovieClip>("btPointer");
        btBlock = mcEditMode.getChildByName<MovieClip>("btBlock");
        btTrampoline = mcEditMode.getChildByName<MovieClip>("btTrampoline");
        SetupButton(btPointer, OnEditClick, "", "pointer");
        SetupButton(btBlock, OnEditClick, "", "block");
        SetupButton(btTrampoline, OnEditClick, "", "trampoline");

        mcBlockNumber = btBlock.getChildByName<MovieClip>("mcNumber");
        mcBlockNumber.gotoAndStop(2);
        mcTrampolineNumber = btTrampoline.getChildByName<MovieClip>("mcNumber");
        mcTrampolineNumber.gotoAndStop(2);

        tfStatusLabel = mcEditMode.getChildByName<TextField>("tfStatusLabel");
        tfScoreLabel = mcEditMode.getChildByName<TextField>("tfScoreLabel");
        tfScoreValue = mcEditMode.getChildByName<TextField>("tfScoreValue");
        tfScoreLabel.text = "Score";
        tfScoreValue.text = MyGame.Instance.Score.ToString();

        tfStatusLabel.text = string.Empty;

        mcPopupShare = mcEditMode.getChildByName<MovieClip>("mcPopupShare");
        btShareOk = mcPopupShare.getChildByName<MovieClip>("btOk");
        btShareCancel = mcPopupShare.getChildByName<MovieClip>("btCancel");
        SetupButton(btShareCancel, OnBtCancelSavePopupClick);
        ShowHideSavePopup(false);

        mcCheckGrid = mcEditMode.getChildByName<MovieClip>("mcCheckGrid");
        mcCheckGrid.addEventListener(MouseEvent.CLICK, OnCheckGridClick);
        mcCheckGrid.gotoAndStop("uncheck");
        
        HideMaterialRulesPopup();

        mcPopupHelp = mcEditMode.getChildByName<MovieClip>("mcPopupHelp");
        btPublishOk = mcPopupHelp.getChildByName<MovieClip>("btOk");
        SetupButton(btPublishOk, OnBtHelpCallback);
        mcPopupHelp.visible = false;
        
        ShowTutorial();
    }

    void OnIngameEditLaunchExec(FSMObject2 self, float time)
    {
        UpdateStatusLabel();
        //if (mcPopupRules.visible && Input.GetKeyDown(KeyCode.Return))
        //    UpdateTextfieldsValues();

        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (parentSelected != null)
            {
                parentSelected.getChildByName<MovieClip>("mcInput" + selectedRuleTextfield).gotoAndStop("normal");
                UpdateTextfieldsValues(parentSelected, selectedRuleTextfield);
            }
        }
    }

    void OnIngameEditLaunchExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcEditMode);
        mcEditMode = null;
        mcPopupHelp = null;
        mcPopupHelp = null;
    }

    #region EditModeCallbacks
    void OnCheckGridClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        if (mc.currentLabel == "check")
        {
            mc.gotoAndStop("uncheck");
            MyGame.Instance.IsGridEnabled = false;
        }
        else
        {
            mc.gotoAndStop("check");
            MyGame.Instance.IsGridEnabled = true;
        }

    }
    #endregion

    #region Material Popup
    bool isVerticalShown = true;
    MovieClip btVertComp;
    MovieClip btHorizComp;
    MovieClip mcRulePlus;
    TextField tfPopupRulesLabel;
    Dictionary<MovieClip, MaterialRule> shownRules = new Dictionary<MovieClip, MaterialRule>();

    void SetupMaterialRule(MovieClip parent, string name, int x, int y, bool isRemovable, MaterialRule rule)
    {
        MovieClip mcRow = new MovieClip("Flash/piki.swf:mcRuleRowClass");
        mcRow.name = name;
        mcRow.x = x;
        mcRow.y = y;
        parent.addChild(mcRow);

        MovieClip mcRuleDelete = mcRow.getChildByName<MovieClip>("mcRuleDelete");
        MovieClip mcRuleOK = mcRow.getChildByName<MovieClip>("mcRuleOK");
        TextField tfRuleInput1 = mcRow.getChildByName<TextField>("tfRuleInput1");
        TextField tfRuleInput2 = mcRow.getChildByName<TextField>("tfRuleInput2");
        TextField tfRuleInput3 = mcRow.getChildByName<TextField>("tfRuleInput3");

        tfRuleInput1.text = (rule.MinValue == Mathf.Infinity) ? "inf" : rule.MinValue.ToString();
        tfRuleInput2.text = (rule.MaxValue == Mathf.Infinity) ? "inf" : rule.MaxValue.ToString();
        tfRuleInput3.text = (rule.FactorValue == Mathf.Infinity) ? "inf" : rule.FactorValue.ToString();

        tfRuleInput1.addEventListener(MouseEvent.CLICK, OnRuleValueClick);
        tfRuleInput2.addEventListener(MouseEvent.CLICK, OnRuleValueClick);
        tfRuleInput3.addEventListener(MouseEvent.CLICK, OnRuleValueClick);

        mcRow.getChildByName<MovieClip>("mcInput1").gotoAndStop("normal");
        mcRow.getChildByName<MovieClip>("mcInput2").gotoAndStop("normal");
        mcRow.getChildByName<MovieClip>("mcInput3").gotoAndStop("normal");

        SetupButton(mcRuleOK, OnMaterialRuleClick);
        if (isRemovable)
        {
            SetupButton(mcRuleDelete, OnMaterialRuleClick);
        }
        else
        {
            mcRuleDelete.visible = false;
        }

        shownRules.Add(mcRow, rule);
    }

    void InitMaterialRulesPopup()
    {
        MovieClip mcRef = mcPopupRules;
        btVertComp = mcRef.getChildByName<MovieClip>("btVertComp");
        btHorizComp = mcRef.getChildByName<MovieClip>("btHorizComp");
        mcRulePlus = mcRef.getChildByName<MovieClip>("mcRulePlus");
        btRulesPopupOk = mcRef.getChildByName<MovieClip>("btOk");
        SetupButton(btRulesPopupOk, OnBtRulesOkClick);
        tfPopupRulesLabel = mcRef.getChildByName<TextField>("tfPopupRulesLabel");

        tfPopupRulesLabel.text = "Material Rules " + MaterialManager.Instance.CurrentMaterial;

        SetupButton(btVertComp, OnMaterialRuleClick);
        SetupButton(btHorizComp, OnMaterialRuleClick);
        SetupButton(mcRulePlus, OnMaterialRuleClick);

        LoadRules();
    }

    void LoadRules()
    {
        MovieClip mcRef = mcPopupRules;
        tfPopupRulesLabel.text = "Material Rules " + MaterialManager.Instance.CurrentMaterial;

        foreach (KeyValuePair<MovieClip, MaterialRule> mc in shownRules)
            mcRef.removeChild(mc.Key);
        
        shownRules.Clear();

        int yBase = -110;
        int yHeight = 30;

        string currentMaterial = MaterialManager.Instance.CurrentMaterial;
        ArrayList ruleList = MaterialManager.Instance.VerticalRules[MaterialManager.Instance.CurrentMaterial];
        if (!isVerticalShown)
            ruleList = MaterialManager.Instance.HorizontalRules[MaterialManager.Instance.CurrentMaterial];

        for (int i = 0; i < ruleList.Count; i++)
        {
            SetupMaterialRule(mcRef, "mcRule" + i, 3, yBase + i * yHeight, i != 0, ruleList[i] as MaterialRule);
        }

        if (ruleList.Count >= 10)
            mcRulePlus.visible = false;
        else
            mcRulePlus.visible = true;

        TextField tfVertRulesLabel = mcRef.getChildByName<TextField>("tfVertRulesLabel");
        tfVertRulesLabel.text = (isVerticalShown) ? "Vertical Rules" : "Horizontal Rules";
    }

    void OnBtRulesOkClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        HideMaterialRulesPopup();
    }

    void ShowMaterialRulesPopup(string material)
    {
        MaterialManager.Instance.CurrentMaterial = material;
        mcPopupRules.visible = true;
        MyGame.Instance.OrthoCametaLocked = true;
        LoadRules();
    }

    void HideMaterialRulesPopup()
    {
        mcPopupRules.visible = false;
        MyGame.Instance.OrthoCametaLocked = false;

        MyGame.Instance.isSceneSaved = false;
        MyGame.Instance.SaveBinaryData("", true);
    }

    void SelectTextfieldCell(MovieClip mcRef, int id)
    {
        //float value;
        TextField tf = mcRef.getChildByName<TextField>("tfRuleInput" + id);
        MovieClip bg = mcRef.getChildByName<MovieClip>("mcInput" + id);

        mcRef.getChildByName<MovieClip>("mcInput1").gotoAndStop("normal");
        mcRef.getChildByName<MovieClip>("mcInput2").gotoAndStop("normal");
        mcRef.getChildByName<MovieClip>("mcInput3").gotoAndStop("normal");

        bg.gotoAndStop("selected");
    }


    void UpdateTextfieldsValues(MovieClip mcRef, int selected)
    {
        for (int i = 1; i <= 3; i++)
        {
            float value;
            TextField tf = mcRef.getChildByName<TextField>("tfRuleInput" + i);
            MovieClip bg = mcRef.getChildByName<MovieClip>("mcInput" + i);

            if (float.TryParse(tf.text, out value))
            {
                switch (i)
                {
                    case 1: shownRules[mcRef].MinValue = value; break;
                    case 2: shownRules[mcRef].MaxValue = value; break;
                    case 3: shownRules[mcRef].FactorValue = value; break;
                }
            }
            else
            {
                if (tf.text.Equals("inf"))
                {
                    switch (i)
                    {
                        case 1: shownRules[mcRef].MinValue = Mathf.Infinity; break;
                        case 2: shownRules[mcRef].MaxValue = Mathf.Infinity; break;
                        case 3: shownRules[mcRef].FactorValue = Mathf.Infinity; break;
                    }
                }
                else
                {
                    if (i != selected)
                        bg.gotoAndStop("wrong");
                    else
                        bg.gotoAndStop("wrong_selected");
                }
            }
        }
    }

    void OnRuleValueClick(CEvent evt)
    {
        TextField tf = evt.currentTarget as TextField;
        MovieClip parent = tf.parent as MovieClip;
        int selected = 0;

        switch (tf.name)
        {
            case "tfRuleInput1":
                selected = 1;
                break;
            case "tfRuleInput2":
                selected = 2;
                break;
            case "tfRuleInput3":
                selected = 3;
                break;
        }

        selectedRuleTextfield = selected;
        parentSelected = parent;

        SelectTextfieldCell(parent, selected);
        UpdateTextfieldsValues(parent, selected);
    }

    void OnMaterialRuleClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        MovieClip parent = mc.parent as MovieClip;

        mc.gotoAndStop("dn");

        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0.0f;

        MyGame.Instance.clickSound.Play();

        switch (mc.name)
        {
            case "btVertComp":
                Debug.Log("btVertComp");
                isVerticalShown = true;
                break;
            case "btHorizComp":
                Debug.Log("btHorizComp");
                isVerticalShown = false;
                break;
            case "mcRulePlus":
                if (isVerticalShown)
                    MaterialManager.Instance.AddVerticalRule(MaterialManager.Instance.CurrentMaterial, new MaterialRule());
                else
                    MaterialManager.Instance.AddHorizontalRule(MaterialManager.Instance.CurrentMaterial, new MaterialRule());
                break;
            case "mcRuleOK":
                UpdateTextfieldsValues(parent, 0);
                Debug.Log("mcRuleOK " + mc.parent.name);
                break;
            case "mcRuleDelete":
                Debug.Log("mcRuleDelete " + mc.parent.name);
                if (isVerticalShown)
                    MaterialManager.Instance.RemoveVerticalRule(MaterialManager.Instance.CurrentMaterial, shownRules[mc.parent as MovieClip]);
                else
                    MaterialManager.Instance.RemoveHorizontalRule(MaterialManager.Instance.CurrentMaterial, shownRules[mc.parent as MovieClip]);
                break;
        }
        LoadRules();
    }
    #endregion

    #region ObjectOptionsPopup
    protected MovieClip mcEditObjectOptions;
    protected MovieClip mcEditOptions;
    protected MovieClip mcEditMove;
    protected MovieClip mcEditDelete;

    void ShowObjectOptionsPopup(Vector3 screenPosition)
    {
        mcEditObjectOptions = new MovieClip("Flash/piki.swf:mcEditObjectOptionsClass");
        mcEditObjectOptions.x = screenPosition.x;
        mcEditObjectOptions.y = (Screen.height - screenPosition.y);
        stage.addChildAt(mcEditObjectOptions, 0);

        mcEditOptions = mcEditObjectOptions.getChildByName<MovieClip>("mcEditOptions");
        mcEditMove = mcEditObjectOptions.getChildByName<MovieClip>("mcEditMove");
        mcEditDelete = mcEditObjectOptions.getChildByName<MovieClip>("mcEditDelete");
        SetupButton(mcEditOptions, OnEditObjectPopupClick);
        SetupButton(mcEditMove, MouseEvent.MOUSE_UP, OnEditObjectPopupClick);
        SetupButton(mcEditDelete, OnEditObjectPopupClick);

        mcEditOptions.visible = DataContainer.Instance.playMode == DataContainer.PlayMode.Launch;
    }

    void UpdateObjectOptionsPopup(Vector3 screenPosition)
    {
        if (mcEditObjectOptions != null)
        {
            mcEditObjectOptions.x = screenPosition.x;
            mcEditObjectOptions.y = (Screen.height - screenPosition.y);
        }
    }

    void HideObjectOptionsPopup()
    {
        if (mcEditObjectOptions != null)
            stage.removeChild(mcEditObjectOptions);
        mcEditObjectOptions = null;
        mcEditMove = null;
        mcEditOptions = null;
        mcEditDelete = null;
    }

    void OnEditObjectPopupClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");

        switch (mc.name)
        { 
            case "mcEditOptions":
                ShowMaterialRulesPopup(MaterialManager.Instance.CurrentMaterial); //TODO EDIT OPTIONS OF THE OBJECT
                break;
            case "mcEditMove":
                ObjectManager.Instance.SelectedObject.SendMessage("OnMoveButton");
                break;
            case "mcEditDelete":
                string tag = ObjectManager.Instance.SelectedObject.tag;
                ObjectManager.Instance.IncrementObjectCount(tag);
                ObjectManager.Instance.RemoveSelected();
                MyGame.Instance.SaveBinaryData("", true);
                HideObjectOptionsPopup();
                break;
        }
    }
    #endregion
    #endregion

    #region IngameLaunch Page
    protected MovieClip mcLaunchMode = null;
    protected MovieClip btEditScene = null;
    protected MovieClip mcCannonPopup = null;
    protected MovieClip btFire = null;
    protected MovieClip btFireAgain = null;
    protected MovieClip mcSliderAngle = null;
    protected MovieClip mcSliderVelocity = null;
    protected MovieClip mcFireNumber = null;
    protected TextField tfDistance = null;
    protected TextField tfAngle = null;
    protected TextField tfVelocity = null;
    protected TextField tfScore = null;

    protected MovieClip mcBackground = null;
    protected MovieClip mcSliderBlock = null;
    protected MovieClip mcPowerBg = null;
    protected MovieClip mcAngleBg = null;

    protected float _angle;
    protected float _power;
    protected bool angleSliderActive = false;
    protected bool velocitySliderActive = false;
    protected float startInputX;

    void OnIngameLaunchEnter(FSMObject2 self, float time)
    {
        Debug.Log("INGAMELAUNCH Enter");
        prevState = IngameLaunchPage;

        mcLaunchMode = new MovieClip("Flash/piki.swf:mcLaunchModeClass");
        stage.addChild(mcLaunchMode);
        InitNavBar(mcLaunchMode);

        mcBackground = mcLaunchMode.getChildByName<MovieClip>("mcTraspBG");
        mcBackground.addEventListener(MouseEvent.CLICK, OnLaunchValuesClick);
        
        btEditScene = mcLaunchMode.getChildByName<MovieClip>("btEditScene");
        SetupButton(btEditScene, OnSwitchLaunchEditButton, "Edit Mode");

        mcCannonPopup = mcLaunchMode.getChildByName<MovieClip>("mcCannonPopup");
        btFireAgain = mcCannonPopup.getChildByName<MovieClip>("btFireAgain");
        btFire = mcCannonPopup.getChildByName<MovieClip>("btFire");
        mcFireNumber = mcCannonPopup.getChildByName<MovieClip>("mcFireNumber");
        SetupButton(btFire, OnBtnFireClick);
        SetupButton(btFireAgain, OnBtnFireAgainClick);
        btFireAgain.visible = false;

        mcAngleBg = mcCannonPopup.getChildByName<MovieClip>("mcAngleBg");
        mcPowerBg = mcCannonPopup.getChildByName<MovieClip>("mcPowerBg");
        mcAngleBg.gotoAndStop("normal");
        mcPowerBg.gotoAndStop("normal");

        mcSliderAngle = mcCannonPopup.getChildByName<MovieClip>("mcSliderAngle");
        mcSliderVelocity = mcCannonPopup.getChildByName<MovieClip>("mcSliderVelocity");

        tfDistance = mcCannonPopup.getChildByName<TextField>("tfDistance");
        tfAngle = mcCannonPopup.getChildByName<TextField>("tfAngle");
        tfVelocity = mcCannonPopup.getChildByName<TextField>("tfVelocity");

        tfAngle.text = Configuration.Cannon.MIN_ANGLE.ToString();
        tfVelocity.text = Configuration.Cannon.MIN_POWER.ToString();

        tfAngle.addEventListener(MouseEvent.CLICK, OnLaunchValuesChanged);
        tfVelocity.addEventListener(MouseEvent.CLICK, OnLaunchValuesChanged);
        

        SetupSlider(mcSliderAngle);
        SetupSlider(mcSliderVelocity);

        if (stage.hasEventListener(MouseEvent.MOUSE_UP))
            stage.removeAllEventListeners(MouseEvent.MOUSE_UP);
        stage.addEventListener(MouseEvent.MOUSE_UP, OnStageMouseButtonUp);

        tfScoreLabel = mcLaunchMode.getChildByName<TextField>("tfScoreLabel");
        tfScoreValue = mcLaunchMode.getChildByName<TextField>("tfScoreValue");
        tfScoreLabel.text = "Score";
        tfScoreValue.text = MyGame.Instance.Score.ToString();

        _cannonComp = (CannonComponent)GameObject.Find("CannonGroup").GetComponent("CannonComponent");
        _angle = DataContainer.Instance.CannonAngle;
        _power = DataContainer.Instance.cannonPower;

        int shotnum = DataContainer.Instance.ShotsNum;
        if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            shotnum = DataContainer.Instance.ShotsNumAttack;
        UpdateFireCounter(shotnum);

        mcSliderBlock = mcCannonPopup.getChildByName<MovieClip>("mcLaunchBlock");
        BlockLaunchInteraction(false);

       
        tfScoreLabel.visible = tfScoreValue.visible = MyGame.Instance.currentPlayMode != DataContainer.PlayMode.Defence;

        SetDistance(MyGame.Instance.distance);
        ShowTutorial();
    }    

    void OnIngameLaunchExec(FSMObject2 self, float time)
    { 
        //Configuration.Cannon.MIN_ANGLE, Configuration.Cannon.MAX_ANGLE
        if (angleSliderActive)
        {
            //Debug.Log("X DELLO SLIDER ANGLE: " + mcSliderAngle.localToGlobal(new Vector2(0, 0)) + " / X DEL MOUSE: " + Input.mousePosition.x + " / startINPUTX: " + startInputX + " Differenza : " + (Input.mousePosition.x - startInputX));
            mcSliderAngle.x += (Input.mousePosition.x - startInputX);
            tfAngle.text = (Mathf.FloorToInt(_angle)).ToString() ;
            _cannonComp.CannonAngle = Mathf.Floor(_angle);
        }
        if (velocitySliderActive)
        {
            //Debug.Log("X DELLO SLIDER ANGLE: " + mcSliderAngle.localToGlobal(new Vector2(0, 0)) + " / X DEL MOUSE: " + Input.mousePosition.x);
            tfVelocity.text = (Mathf.FloorToInt(_power)).ToString();
            _cannonComp.CannonPower = Mathf.Floor(_power);
        }

        if (Input.GetMouseButtonUp(0))
        {
            angleSliderActive = velocitySliderActive = false;
            mcSliderVelocity.gotoAndStop("up");
            mcSliderAngle.gotoAndStop("up");
        }

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && (mcAngleBg.currentLabel == "selected" || mcPowerBg.currentLabel == "selected"))
        {
            UpdateLaunchTextfieldsValues(mcBackground);
            SelectLaunchTextfieldCell(mcBackground);
            OnStageMouseButtonUp(null);
        }
        //tfScore.text = MyGame.Instance.Score.ToString();
    }

    void OnIngameLaunchExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcLaunchMode);
        mcLaunchMode = null;
        btEditScene = null;
        mcCannonPopup = null;
        btFire = null;
        btFireAgain = null;
        mcSliderAngle = null;
        mcSliderVelocity = null;
        mcFireNumber = null;
        tfDistance = null;
        tfAngle = null;
        tfVelocity = null;
        tfScore = null;
        mcBackground = null;

        if (stage.hasEventListener(MouseEvent.MOUSE_UP))
            stage.removeAllEventListeners(MouseEvent.MOUSE_UP);
    }

    public void SetDistance(int value)
    {
        if (null != tfDistance)
        {
            tfDistance.text = value.ToString() + " m";
        }
    }

    void UpdateFireCounter(int fireNumber)
    {
        mcFireNumber.gotoAndStop(1);
        mcFireNumber.getChildByName<TextField>("tfLabel").text = fireNumber.ToString();
    }

    public void BlockLaunchInteraction(bool flag)
    {
        if (null != mcSliderBlock)
            mcSliderBlock.visible = flag;
    }

    protected int minSliderX = -24;
    protected int maxSliderX = 61;

    void SetupSlider(MovieClip mc)
    {
        mc.gotoAndStop("up");

        if (mc.name.Equals("mcSliderAngle"))
        {
            mc.x = GetSliderXByValues(DataContainer.Instance.CannonAngle, Configuration.Cannon.MIN_ANGLE, Configuration.Cannon.MAX_ANGLE);
            tfAngle.text = DataContainer.Instance.CannonAngle.ToString();
        }
        else if (mc.name.Equals("mcSliderVelocity"))
        {
            mc.x = GetSliderXByValues(DataContainer.Instance.CannonPower, Configuration.Cannon.MIN_POWER, Configuration.Cannon.MAX_POWER);
            tfVelocity.text = DataContainer.Instance.CannonPower.ToString();// +" m/s";
        }

        if (!mc.hasEventListener(MouseEvent.MOUSE_DOWN))
            mc.addEventListener(MouseEvent.MOUSE_DOWN, OnSliderMouseButtonDown);
    }

    void ReleaseMove()
    {
        if (stage.hasEventListener(MouseEvent.MOUSE_MOVE))
            stage.removeAllEventListeners(MouseEvent.MOUSE_MOVE);
    }

    MovieClip currentSlider = null;
    void OnSliderMouseButtonDown(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        currentSlider = mc;
        Debug.Log("OnSliderMouseButtonDown" + mc.localToGlobal(new Vector2(mc.x, mc.y)));

        if (!stage.hasEventListener(MouseEvent.MOUSE_MOVE))
            stage.addEventListener(MouseEvent.MOUSE_MOVE, OnStageMouseButtonMove);        
    }

    void OnSliderMouseButtonMove(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;

        mc.x = mc.parent.mouseX;


        if (mc.x < minSliderX)
        {
            mc.x = minSliderX;
            ReleaseMove();
        }

        if (mc.x > maxSliderX)
        {
            mc.x = maxSliderX;
            ReleaseMove();
        }

        float sliderFactor = (mc.x - minSliderX) / (maxSliderX - minSliderX);
        switch (mc.name)
        {
            case "mcSliderAngle":
                float angle = Configuration.Cannon.MIN_ANGLE + sliderFactor * (Configuration.Cannon.MAX_ANGLE - Configuration.Cannon.MIN_ANGLE);
                _cannonComp.CannonAngle = Mathf.Floor(angle);
                tfAngle.text = _cannonComp.CannonAngle.ToString();
                break;
            case "mcSliderVelocity":
                float power = Configuration.Cannon.MIN_POWER + sliderFactor * (Configuration.Cannon.MAX_POWER - Configuration.Cannon.MIN_POWER);
                _cannonComp.CannonPower = Mathf.Floor(power);
                tfVelocity.text = _cannonComp.CannonPower + " m/s";
                break;
        }
    }

    void OnStageMouseButtonMove(CEvent evt)
    {
        Debug.Log("Stage: " + stage.mouseX + ", " + stage.mouseY);
        Vector2 mousePos = new Vector2(stage.mouseX, stage.y);
        Vector2 localPos = new Vector2(stage.mouseX - 597 - 48, stage.y); //currentSlider.globalToLocal(mousePos);

        if (localPos.x < minSliderX)
        {
            localPos.x = minSliderX;
        }

        if (localPos.x > maxSliderX)
        {
            localPos.x = maxSliderX;
        }

        if (null != currentSlider)
        {
            currentSlider.x = localPos.x;
            float sliderFactor = (currentSlider.x - minSliderX) / (maxSliderX - minSliderX);
            switch (currentSlider.name)
            {
                case "mcSliderAngle":
                    float angle = Configuration.Cannon.MIN_ANGLE + sliderFactor * (Configuration.Cannon.MAX_ANGLE - Configuration.Cannon.MIN_ANGLE);
                    _cannonComp.CannonAngle = Mathf.Floor(angle);
                    tfAngle.text = _cannonComp.CannonAngle.ToString();
                    break;
                case "mcSliderVelocity":
                    float power = Configuration.Cannon.MIN_POWER + sliderFactor * (Configuration.Cannon.MAX_POWER - Configuration.Cannon.MIN_POWER);
                    _cannonComp.CannonPower = Mathf.Floor(power);
                    tfVelocity.text = _cannonComp.CannonPower + " m/s";
                    break;
            }
        }
        else
        {
            OnStageMouseButtonUp(evt);
        }
    }

    void OnStageMouseButtonUp(CEvent evt)
    {
        currentSlider = null;
        ReleaseMove();
    }

    float GetSliderXByValues(float value, float min, float max)
    {
        float factor = (value - min) / (max - min);
        return minSliderX + factor * (maxSliderX - minSliderX);
    }

    void SelectLaunchTextfieldCell(MovieClip bg)
    {
        mcAngleBg.gotoAndStop("normal");
        mcPowerBg.gotoAndStop("normal");

        if (bg == mcAngleBg || bg == mcPowerBg)
            bg.gotoAndStop("selected");
    }

    void UpdateLaunchTextfieldsValues(MovieClip _bg, TextField _tf = null)
    {
            float value;
            TextField tf = _tf;
            MovieClip bg = _bg;

            if (float.TryParse(tfAngle.text, out value))
            {
                value = Mathf.Clamp(value, Configuration.Cannon.MIN_ANGLE, Configuration.Cannon.MAX_ANGLE);
                _cannonComp.CannonAngle = Mathf.Floor(value);
                mcSliderAngle.x = GetSliderXByValues(value, Configuration.Cannon.MIN_ANGLE, Configuration.Cannon.MAX_ANGLE);
                tfAngle.text = value.ToString();
            }
            else
            {
                if(mcAngleBg.currentLabel != "selected")//if (_tf != tfAngle)
                    mcAngleBg.gotoAndStop("wrong");
                else
                    mcAngleBg.gotoAndStop("wrong_selected");
            }

            if (float.TryParse(tfVelocity.text, out value))
            {
                value = Mathf.Clamp(value, Configuration.Cannon.MIN_POWER, Configuration.Cannon.MAX_POWER);
                _cannonComp.CannonPower = Mathf.Floor(value);
                mcSliderVelocity.x = GetSliderXByValues(value, Configuration.Cannon.MIN_POWER, Configuration.Cannon.MAX_POWER);
                tfVelocity.text = value.ToString();
            }
            else
            {
                if (_tf != tfVelocity)
                    mcPowerBg.gotoAndStop("wrong");
                else
                    mcPowerBg.gotoAndStop("wrong_selected");
            }
    }

    void OnLaunchValuesChanged(CEvent evt)
    {
        TextField tf = evt.currentTarget as TextField;
        MovieClip bg = mcAngleBg;

        switch (tf.name)
        {
            case "tfAngle":
                bg = mcAngleBg;
                break;
            case "tfVelocity":
                bg = mcPowerBg;
                break;
        }

        SelectLaunchTextfieldCell(bg);
        UpdateLaunchTextfieldsValues(bg, tf);
        //OnStageMouseButtonUp(null);
    }

    void OnLaunchValuesClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        SelectLaunchTextfieldCell(mc);
        UpdateLaunchTextfieldsValues(mc);
        //OnStageMouseButtonUp(null);
    }

    #endregion

    #region LaunchReward State
    MovieClip mcRewardPage = null;
    MovieClip[] mcRate = null;
    TextField tfTitle = null;
    TextField tfTitle2 = null;
    TextField tfTreasureLabel = null;
    TextField tfTreasureScore = null;
    TextField tfShootsLabel = null;
    TextField tfShootsValue = null;
    TextField tfShootsScore = null;
    TextField tfTotalLabel = null;
    TextField tfTotalScore = null;
    TextField tfVertRulesLabel = null;

    void OnLaunchRewardEnter(FSMObject2 self, float time)
    {
        Debug.Log("REWARD Enter");
        mcRewardPage = new MovieClip("Flash/piki.swf:mcRewardPageClass");
        mcRewardPage.gotoAndStop("PosRew");
        stage.addChild(mcRewardPage);

        InitNavBar(mcRewardPage);

        tfTitle = mcRewardPage.getChildByName<TextField>("tfTitle");
        tfTreasureLabel = mcRewardPage.getChildByName<TextField>("tfTreasureLabel");
        tfTreasureScore = mcRewardPage.getChildByName<TextField>("tfTreasureScore");
        tfShootsLabel = mcRewardPage.getChildByName<TextField>("tfShootsLabel");
        tfShootsValue = mcRewardPage.getChildByName<TextField>("tfShootsValue");
        tfShootsScore = mcRewardPage.getChildByName<TextField>("tfShootsScore");
        tfTotalLabel = mcRewardPage.getChildByName<TextField>("tfTotalLabel");
        tfTotalScore = mcRewardPage.getChildByName<TextField>("tfTotalScore");
        tfVertRulesLabel = mcRewardPage.getChildByName<TextField>("tfVertRulesLabel");
        tfTitle2 = mcRewardPage.getChildByName<TextField>("tfTitle2");

        tfTitle.text = "Reward";
        tfTreasureLabel.text = "Treasure Chests";
        tfShootsLabel.text = "Shoots Saved";
        tfTotalLabel.text = "Total Score";
        tfVertRulesLabel.text = "Rate this scene";
        tfTitle2.text = "Create a new resource card to play again";

        int score = MyGame.Instance.Score;
        int shots = DataContainer.Instance.ShotsNumAttack;
        int shotsScore = shots * 100;
        int totalScore = score + shotsScore;

        tfTreasureScore.text = score.ToString();
        tfShootsValue.text = shots.ToString();
        tfShootsScore.text = shotsScore.ToString();
        tfTotalScore.text = totalScore.ToString();
        
        mcRate = new MovieClip[5];
        for (int i = 0; i < mcRate.Length; i++)
        {
            mcRate[i] = mcRewardPage.getChildByName<MovieClip>("mcRate" + (i+1).ToString());
            mcRate[i].gotoAndStop(1);
            SetupButton(mcRate[i], OnRateClick);
        }
        btSoundOn = mcRewardPage.getChildByName<MovieClip>("btSoundOn");
        
        if (DataContainer.Instance.sceneState == DataContainer.SceneState.Rated)
            UpdateRate(mcRate[DataContainer.Instance.Rating - 1], false);

        Debug.Log("DataContainer.Instance.sceneState " + DataContainer.Instance.sceneState);
    }

    void OnLaunchRewardExec(FSMObject2 self, float time)
    {
    }

    void OnLaunchRewardExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcRewardPage);
        mcRewardPage = null;
        mcRate = null;
        tfTitle = null;
        tfTitle2 = null;
        tfTreasureLabel = null;
        tfTreasureScore = null;
        tfShootsLabel = null;
        tfShootsValue = null;
        tfShootsScore = null;
        tfTotalLabel = null;
        tfTotalScore = null;
        tfVertRulesLabel = null;

        btSoundOn = null;
    }

    #region LaunchRewardButtonsCallback
    void UpdateRate(MovieClip mc, bool sendToServer)
    {
        int rate = 0;
        bool rated = false;
        for (int i = 0; i < mcRate.Length; i++)
        {
            if (!rated)
                mcRate[i].gotoAndStop("dn");
            if (mc == mcRate[i])
            {
                rated = true;
                rate = i + 1;
            }
            mcRate[i].removeAllEventListeners(MouseEvent.MOUSE_DOWN);
            mcRate[i].removeAllEventListeners(MouseEvent.MOUSE_ENTER);
            mcRate[i].removeAllEventListeners(MouseEvent.MOUSE_LEAVE);
        }

        DataContainer.Instance.Rating = rate;

        if (sendToServer)
            BackendManager.Instance.RateScene(DataContainer.Instance.SelectedSceneId, rate);
    }

    void OnRateClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        UpdateRate(mc, true);        
    }
    #endregion
    #endregion

    #region DefenceReward
    protected TextField tfPublished = null;

    void OnDefenceRewardEnter(FSMObject2 self, float time)
    {
        Debug.Log("REWARD Enter");
        mcRewardPage = new MovieClip("Flash/piki.swf:mcRewardPageClass");
        mcRewardPage.gotoAndStop("Publish");
        stage.addChild(mcRewardPage);

        tfTitle = mcRewardPage.getChildByName<TextField>("tfTitle");
        tfPublished = mcRewardPage.getChildByName<TextField>("tfPublished");
        tfTitle2 = mcRewardPage.getChildByName<TextField>("tfTitle2");
        tfTitle.text = "Great Job!";
        tfPublished.text = "You successfully published the scene";
        tfTitle2.text = "Create a new resource card to play again";
        
        InitNavBar(mcRewardPage);
    }

    void OnDefenceRewardExec(FSMObject2 self, float time)
    { }

    void OnDefenceRewardExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcRewardPage);
        mcRewardPage = null;
        tfTitle = null;
        tfPublished = null;
        tfTitle2 = null;

        btSoundOn = null;
    }
    #endregion

    #region NegativeReward
    protected TextField tfNegRew = null;
    void OnNegativeRewardEnter(FSMObject2 self, float time)
    {
        Debug.Log("Negative REWARD Enter");
        mcRewardPage = new MovieClip("Flash/piki.swf:mcRewardPageClass");
        mcRewardPage.gotoAndStop("NegativeReward");
        stage.addChild(mcRewardPage);

        InitNavBar(mcRewardPage);

        tfTitle = mcRewardPage.getChildByName<TextField>("tfTitle");
        tfNegRew = mcRewardPage.getChildByName<TextField>("tfNegRew");
        tfVertRulesLabel = mcRewardPage.getChildByName<TextField>("tfVertRulesLabel");
        tfTitle2 = mcRewardPage.getChildByName<TextField>("tfTitle2");

        mcRate = new MovieClip[5];
        for (int i = 0; i < mcRate.Length; i++)
        {
            mcRate[i] = mcRewardPage.getChildByName<MovieClip>("mcRate" + (i + 1).ToString());
            mcRate[i].gotoAndStop(1);
            SetupButton(mcRate[i], OnRateClick);
        }

        if (DataContainer.Instance.sceneState == DataContainer.SceneState.Rated)
            UpdateRate(mcRate[DataContainer.Instance.Rating - 1], false);
    }

    void OnNegativeRewardExec(FSMObject2 self, float time)
    {
    }

    void OnNegativeRewardExit(FSMObject2 self, float time)
    {
        stage.removeChild(mcRewardPage);
        mcRewardPage = null;
        tfTitle = null;
        tfPublished = null;
        tfTitle2 = null;
        tfVertRulesLabel = null;
        btSoundOn = null;
    }
    #endregion

    protected int NextState = Void;
    public void CheckRewardInterface(bool immediatly = false)
    {
        Debug.Log("CheckRewardInterface: " + DataContainer.Instance.sceneState.ToString());
        if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
        {
            Debug.Log(DataContainer.Instance.TreasureCount.ToString() + ", " + DataContainer.Instance.TreasureHit.ToString());
            if (DataContainer.Instance.TreasureCount <= DataContainer.Instance.TreasureHit)
            {
                if (DataContainer.Instance.sceneState != DataContainer.SceneState.Finished && DataContainer.Instance.sceneState != DataContainer.SceneState.Rated)
                    BackendManager.Instance.WinScene(DataContainer.Instance.SelectedSceneId);

                if (DataContainer.Instance.sceneState != DataContainer.SceneState.Rated)
                    DataContainer.Instance.sceneState = DataContainer.SceneState.Finished;

                if (immediatly)
                {
                    State = LaunchRewardPage;
                }
                else
                {
                    NextState = LaunchRewardPage;
                    ShowFinishPopup("Well done!");
                }
                MyGame.Instance.SaveBinaryData("", true);
            }
            else if (DataContainer.Instance.ShotsNumAttack == 0)
            {
                if (DataContainer.Instance.sceneState != DataContainer.SceneState.Rated)
                    DataContainer.Instance.sceneState = DataContainer.SceneState.Finished;
                
                if (immediatly)
                {
                    State = NegativeRewardPage;
                }
                else
                {
                    ShowFinishPopup("Game Over");
                    NextState = NegativeRewardPage;
                }
                MyGame.Instance.SaveBinaryData("", true);
            }
            else
            {
                SwitchPlayMode();
            }
        }
        else
        {
            SwitchPlayMode();
        }
    }

    #endregion

    #region Buttons Callbacks
    void SetupButton(MovieClip mc, EventDispatcher.EventCallback ClickCallback, string text = "", string iconLabel = "")
    {
        mc.gotoAndStop("up");

        if (!mc.hasEventListener(MouseEvent.MOUSE_DOWN))
            mc.addEventListener(MouseEvent.MOUSE_DOWN, ClickCallback);

        if (!mc.hasEventListener(MouseEvent.MOUSE_ENTER))
            mc.addEventListener(MouseEvent.MOUSE_ENTER, OnBtnEnter);

        if (!mc.hasEventListener(MouseEvent.MOUSE_LEAVE))
            mc.addEventListener(MouseEvent.MOUSE_LEAVE, OnBtnLeave);

        if (text != "")
            mc.getChildByName<TextField>("tfLabel").text = text;

        if (iconLabel != "")
            mc.getChildByName<MovieClip>("mcIcon").gotoAndStop(iconLabel);
    }

    void SetupButton(MovieClip mc, string clickEvent, EventDispatcher.EventCallback ClickCallback, string text = "", string iconLabel = "")
    {
        mc.gotoAndStop("up");

        if (!mc.hasEventListener(clickEvent))
            mc.addEventListener(clickEvent, ClickCallback);

        if (!mc.hasEventListener(MouseEvent.MOUSE_ENTER))
            mc.addEventListener(MouseEvent.MOUSE_ENTER, OnBtnEnter);

        if (!mc.hasEventListener(MouseEvent.MOUSE_LEAVE))
            mc.addEventListener(MouseEvent.MOUSE_LEAVE, OnBtnLeave);

        if (text != "")
            mc.getChildByName<TextField>("tfLabel").text = text;

        if (iconLabel != "")
            mc.getChildByName<MovieClip>("mcIcon").gotoAndStop(iconLabel);
    }

    void GhostButton(MovieClip button, string text = "")
    {
        button.gotoAndStop("gh");
        button.removeAllEventListeners(MouseEvent.CLICK);
        button.removeAllEventListeners(MouseEvent.MOUSE_DOWN);
        button.removeAllEventListeners(MouseEvent.MOUSE_ENTER);
        button.removeAllEventListeners(MouseEvent.MOUSE_LEAVE);

        if (text != "")
            button.getChildByName<TextField>("tfLabel").text = text;
    }

    void OnBtnFireClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        int shotsNum = DataContainer.Instance.ShotsNum;
        if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            shotsNum = DataContainer.Instance.ShotsNumAttack;

        if (shotsNum > 0)
        {
            _cannonComp.DestroyBall();
            BlockLaunchInteraction(true);        
            MyGame.Instance.clickSound.Play();
            MyGame.Instance.OnLoad();

            if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            {
                DataContainer.Instance.ShotsNumAttack--;
                UpdateFireCounter(DataContainer.Instance.ShotsNumAttack);
            }
            else
            {
                DataContainer.Instance.ShotsNum--;
                UpdateFireCounter(DataContainer.Instance.ShotsNum);
            }

            MyGame.Instance.isSceneSaved = false;
            MyGame.Instance.SaveBinaryData("", true);
        }
    }

    void OnBtnFireAgainClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        
        btFire.visible = true;
        btFireAgain.visible = false;

        int shotsNum = DataContainer.Instance.ShotsNum;
        if (DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
            shotsNum = DataContainer.Instance.ShotsNumAttack;

        if (shotsNum > 0)
        {
            game.FSM.State = GameplayStates.PrepareLaunchState;
            MyGame.Instance.clickSound.Play();
        }
        _cannonComp.DestroyBall();
    }

    void OnSwitchLaunchEditButton(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        ChangePlayMode();
    }

    public void SwitchPlayMode()
    {
        StartCoroutine(SwitchPlayModeCoroutine());
    }

    IEnumerator SwitchPlayModeCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        ChangePlayMode();
    }

    public void ChangePlayMode()
    {
        if (this.State == IngameEditDefencePage || this.State == IngameEditLaunchPage)
        {
            this.State = IngameLaunchPage;
            game.FSM.State = GameplayStates.PrepareLaunchState;
        }
        else if (this.State == IngameLaunchPage && DataContainer.Instance.playMode == DataContainer.PlayMode.Defence)
        {
            this.State = IngameEditDefencePage;
            game.FSM.State = GameplayStates.EditModeState;
        }
        else if (this.State == IngameLaunchPage && DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
        {
            this.State = IngameEditLaunchPage;
            game.FSM.State = GameplayStates.EditModeState;
        }
    }

    void OnEditClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        Vector3 worldPosition = GameCamera.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0.0f;
        
        MyGame.Instance.clickSound.Play();

        string tag = string.Empty;
        switch (mc.name)
        {
            case "btPointer":
                ObjectManager.Instance.ClearSelected();
                break;
            case "btFlag":
            case "mcFlagNumber":
                tag = "Flag";
                if (ObjectManager.Instance.GetObjectCount(tag) > 0)
                    ObjectManager.Instance.PlaceObject(ObjectManager.FLAG, worldPosition);
                break;
            case "btCrates":
            case "mcCratesNumber":
                tag = "Boxes";
                if (ObjectManager.Instance.GetObjectCount(tag) > 0)
                    ObjectManager.Instance.PlaceObject(ObjectManager.BOXES, worldPosition);
                break;
            case "btPalm":
            case "mcPalmNumber":
                tag = "Palm";
                if (ObjectManager.Instance.GetObjectCount(tag) > 0)
                    ObjectManager.Instance.PlaceObject(ObjectManager.PALM, worldPosition);
                break;
            case "btBlock":
                ObjectManager.Instance.PlaceObject(ObjectManager.BLOCK, worldPosition);
                break;
            case "btTrampoline":
                ObjectManager.Instance.PlaceObject(ObjectManager.TRAMPOLINE, worldPosition);
                break;
        }

        if(tag.Length > 0)
            ObjectManager.Instance.DecrementObjectCount(tag);
    }

    void OnCamerasClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");
        MyGame.Instance.clickSound.Play();
        switch (mc.name)
        { 
            case "btCameraSide":
                GameCamera.Instance.FSM.State = CameraStates.SideCameraState;
                break;
            case "btCameraBack": 
                GameCamera.Instance.FSM.State = CameraStates.BackCameraState;
                break;
            case "btCameraTop":
                GameCamera.Instance.FSM.State = CameraStates.TopCameraState; 
                break;
        }
    }

    void OnNavBarClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        MyGame.Instance.clickSound.Play();

        switch (mc.name)
        { 
                
            case "btHelp":
                //mcPopupHelp.visible = true;
                ShowEditPopup();
                break;
            case "btSoundOn":
                MyGame.Instance.UpdateAudioState(!MyGame.Instance.isAudioOn);
                MovieClip mcIcon = mc.getChildByName<MovieClip>("mcIcon");
                mcIcon.gotoAndStop(!MyGame.Instance.isAudioOn ? "soundoff" : "soundon");
                break;
            case "btHiscore":
                State = HighscorePage;
                break;
            case "btLasad": 
                SetupButton(btShareOk, OnBtOkSharePopupClick);
                mcPopupShare.getChildByName<TextField>("tfPopupText").text = "Do you want to share your scene in Lasad?";
                ShowHideSavePopup(true);
                break;
            case "btSave":
                //MyGame.Instance.GUISave();
                SetupButton(btShareOk, OnBtOkSavePopupClick);
                mcPopupShare.getChildByName<TextField>("tfPopupText").text = "Do you want to save your scene?";
                ShowHideSavePopup(true);
                break;
        }
    }

    void OnModeClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        haveToShowTutorial = true;
        switch (mc.name)
        {
            case "btStart":
                haveToShowTutorial = false;
                game.FSM.State = GameplayStates.EditModeState;
                break;
            case "btLaunch":
                this.State = ChooseScenePage;
                //DataContainer.Instance.sceneState = DataContainer.SceneState.Opened;
                DataContainer.Instance.playMode = DataContainer.PlayMode.Launch;
                break;
            case "btDefence":

                Dictionary<string, string> config = MyGame.Instance.ConfigParams;
                float startPos = config.ContainsKey("treasure_start_pos") ? float.Parse(config["treasure_start_pos"]) : 40.0f;
                float endPos = config.ContainsKey("treasure_end_pos") ? float.Parse(config["treasure_end_pos"]) : 300.0f;
                int treasureCount = config.ContainsKey("treasure_num") ? int.Parse(config["treasure_num"]) : 4;
                float randomFactor = config.ContainsKey("treasure_random_factor") ? float.Parse(config["treasure_random_factor"]) : 1.0f;
                StaticObjectsManager.Instance.Initialize(startPos, endPos, treasureCount, randomFactor);

                DataContainer.Instance.TreasureCount = treasureCount;
                DataContainer.Instance.sceneState = DataContainer.SceneState.Opened;
                DataContainer.Instance.playMode = DataContainer.PlayMode.Defence;
                DataContainer.Instance.OwnerGroup = MyGame.Instance.GroupId;
                MyGame.Instance.SaveBinaryData("", true);
                game.FSM.State = GameplayStates.EditModeState;

                GameObject.Find("character_boy").SendMessage("HideCharacter");
                GameObject.Find("character_girl").SendMessage("HideCharacter");
                break;
        }

        MyGame.Instance.currentPlayMode = DataContainer.Instance.playMode;
    }

    void OnBtnEnter(CEvent evt)
    {
        cursorIndex = 1;
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("ov");
    }

    void OnBtnLeave(CEvent evt)
    {
        cursorIndex = 0;
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("up");
    }
    
    #endregion

    #region Common Init Functions

    void InitNavBar(MovieClip mcRoot)
    {
        btSoundOn = mcRoot.getChildByName<MovieClip>("btSoundOn");
        SetupButton(btSoundOn, OnNavBarClick, "", "soundon");

        btHiscore = mcRoot.getChildByName<MovieClip>("btHiscore");
        if(btHiscore != null)
            SetupButton(btHiscore, OnNavBarClick, "", "highscore");

        btHelp = mcRoot.getChildByName<MovieClip>("btHelp");
        if (btHelp != null)
            SetupButton(btHelp, OnNavBarClick, "", "help");
    }

    void InitManagementButtons(MovieClip mcRoot)
    {
        btLasad = mcRoot.getChildByName<MovieClip>("btLasad");
        btSave = mcRoot.getChildByName<MovieClip>("btSave");
        SetupButton(btLasad, OnNavBarClick, "", "lasad");
        SetupButton(btSave, OnNavBarClick, "", "save");
    }

    void InitCamerasButtons(MovieClip mcRoot)
    {
        btCameraSide = mcRoot.getChildByName<MovieClip>("btCameraSide");
        btCameraBack = mcRoot.getChildByName<MovieClip>("btCameraBack");
        btCameraTop = mcRoot.getChildByName<MovieClip>("btCameraTop");
        SetupButton(btCameraSide, OnCamerasClick, "", "cameraside");
        SetupButton(btCameraBack, OnCamerasClick, "", "cameraback");
        SetupButton(btCameraTop, OnCamerasClick, "", "cameratop");
    }
    #endregion

    #region Utils
    protected MovieClip mcLoading;

    public void ShowLoadingPopup()
    {
        if (null == mcLoading)
        {
            mcLoading = new MovieClip("Flash/piki.swf:mcLoadingClass");
            stage.addChild(mcLoading);
        }

        if (this.State == HighscorePage)
        {
            GhostButton(btHighscoreContinue, "Back");
            GhostButton(btHighscoreLeft);
            GhostButton(btHighscoreRight);
        }

        if (this.State == ChooseScenePage)
        {
            GhostButton(btContinue, "Continue");
            GhostButton(btLeft);
            GhostButton(btRight);
        }
    }

    public void HideLoadingPopup()
    {
        if(mcLoading != null)
            stage.removeChild(mcLoading);
        mcLoading = null;

        if (this.State == HighscorePage)
        {
            SetupButton(btHighscoreContinue, OnHighscoreButtonsClick, "Back");
            if (hsCurrentPage > 1) SetupButton(btHighscoreLeft, OnHighscoreButtonsClick);
            if (hsCurrentPage < hsTotalPageNum) SetupButton(btHighscoreRight, OnHighscoreButtonsClick);
        }

        if (this.State == ChooseScenePage)
        {
            if (currentPage > 1) SetupButton(btLeft, OnChooseSceneButtonsClick);
            if (currentPage < totalPageNum) SetupButton(btRight, OnChooseSceneButtonsClick);
        }
    }


    void WriteTextInButtonCounter(MovieClip counter, string txt)
    {
        counter.getChildByName<TextField>("tfLabel").text = txt;
    }

    public void UpdateScore()
    {
        if (null != tfScoreValue)
            tfScoreValue.text = MyGame.Instance.Score.ToString();
    }

    public class FlyierData
    {
        public int x;
        public int y;
        public string flyierText;

        public FlyierData(int _x, int _y, string _text)
        {
            x = _x;
            y = _y;
            flyierText = _text;
        }
    }

    public void CreateFlyier(FlyierData fData)
    {
        MovieClip flyier = new MovieClip("Flash/piki.swf:mcSvolazScoreClass");
        stage.addChild(flyier);
        flyier.x = fData.x;
        flyier.y = fData.y;
        flyier.getChildByName<MovieClip>("mcSvolText").getChildByName<TextField>("tfLabel").text = fData.flyierText;
        flyier.addFrameScript("end", n => { flyier.gotoAndStop("end"); stage.removeChild(flyier); });

    }
    #endregion

    #region Help Popup
    protected MovieClip mcHelpPage = null;
    protected MovieClip btHelpRight = null;
    protected MovieClip btHelpLeft = null;
    protected MovieClip btHelpBack = null;
    protected MovieClip btHelpSound = null;

    void ShowEditPopup()
    {
        mcHelpPage = new MovieClip("Flash/piki.swf:mcHelpPageClass");
        btHelpLeft = mcHelpPage.getChildByName<MovieClip>("btLeft");
        btHelpRight = mcHelpPage.getChildByName<MovieClip>("btRight");
        btHelpBack = mcHelpPage.getChildByName<MovieClip>("btBack");
        btHelpSound = mcHelpPage.getChildByName<MovieClip>("btSoundOn");
        SetupButton(btHelpLeft, OnHelpPageClick);
        SetupButton(btHelpRight, OnHelpPageClick);
        SetupButton(btHelpBack, HideEditPopup, "BACK");
        SetupButton(btHelpSound, OnNavBarClick);
        stage.addChild(mcHelpPage);

        mcHelpPage.gotoAndStop("Generic");
        mcHelpPage.gotoAndStop(this.State == IngameEditDefencePage ? "Defence" : "Launch");
        
        MovieClip mcIcon = btHelpSound.getChildByName<MovieClip>("mcIcon");
        mcIcon.gotoAndStop(!MyGame.Instance.isAudioOn ? "soundoff" : "soundon");
        
    }

    void HideEditPopup(CEvent evt)
    {
        (evt.currentTarget as MovieClip).gotoAndStop("dn");

        stage.removeChild(mcHelpPage);
        mcHelpPage = null;
        btHelpRight = null;
        btHelpLeft = null;
        btHelpBack = null;
        btHelpSound = null;
    }

    void OnHelpPageClick(CEvent evt)
    {
        MovieClip mc = evt.currentTarget as MovieClip;
        mc.gotoAndStop("dn");

        if (this.State == IngameEditDefencePage)
        {
            mcHelpPage.gotoAndStop(mcHelpPage.currentLabel != "Defence" ? "Defence" : "Generic");
            mcHelpPage.getChildByName<TextField>("tfTitle").text = "Defence Help" ;
        }

        if (this.State == IngameEditLaunchPage)
        {
            mcHelpPage.gotoAndStop(mcHelpPage.currentLabel != "Launch" ? "Launch" : "Generic");
            mcHelpPage.getChildByName<TextField>("tfTitle").text = "Launch Help";
        }
    }
    #endregion
}
