using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DataContainer
{
    #region Singleton
    public static DataContainer _instance;

    public static DataContainer Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    #endregion

    #region Internal Data Structures
    public enum PlayMode
    {
        None = 0,
        Defence,
        Launch
    }

    public enum SceneState
    {
        New = 0,
        Opened,
        Published,
        Finished,
        Rated
    }
    
    [System.Flags]
    public enum Authorization
    {
        None = 0,
        CannonPower = 1 << 0,
        CannonAngle = 1 << 1,
        BlockMove = 1 << 2,
        BlockAddRemove = 1 << 3,
        TrampolineMove = 1 << 4,
        TrampolineAddRemove = 1 << 5,
        HoopMove = 1 << 6,
        HoopAddRemove = 1 << 7,
        MaterialShow = 1 << 8,
        MaterialAddRemove = 1 << 9,
        All = 0x7fffffff
    }
    #endregion

    #region Public Fields
    public float cannonPower = Configuration.Cannon.MIN_POWER;
    public float cannonAngle = Configuration.Cannon.MIN_ANGLE;
    public int shotsNum = 0;
    public int shotsNumAttack = 0;
    public int flagsNum = 0;
    public int palmsNum = 0;
    public int boxesNum = 0;
    public int cloudsNum = 0;
    public int treasureCount = 0;
    public int treasureHit = 0;
    public int rating = 0;
    public int selectedSceneId = 0;
    public string ownerGroup = string.Empty;
    public string creator = string.Empty;
    public SceneState sceneState = SceneState.New;
    public PlayMode playMode = PlayMode.None;
    public PlayerController.CharacterType player = PlayerController.CharacterType.MALE;
    public Authorization permissionSet = Authorization.All;
    public MaterialRule[] horizontalBlockRules = null;
    public MaterialRule[] verticalBlockRules = null;
    public MaterialRule[] horizontalTrampolineRules = null;
    public MaterialRule[] verticalTrampolineRules = null;
    public MovableObject[] objectList = null;
    public MovableObject[] treasureList = null;
    #endregion

    #region Get/Set Modifiers
    public float CannonPower
    {
        get
        {
            return cannonPower;
        }
        set
        {
            cannonPower = value;
        }
    }
    public float CannonAngle
    {
        get
        {
            return cannonAngle;
        }
        set
        {
            cannonAngle = value;
        }
    }
    public int ShotsNum
    {
        get
        {
            return shotsNum;
        }
        set
        {
            if (value < 0) value = 0;
            shotsNum = value;
        }
    }
    public int ShotsNumAttack
    {
        get
        {
            return shotsNumAttack;
        }
        set
        {
            if (value < 0) value = 0;
            shotsNumAttack = value;
        }
    }
    public int FlagsNum
    {
        get
        {
            return flagsNum;
        }
        set
        {
            if (value < 0) value = 0;
            flagsNum = value;
        }
    }
    public int PalmsNum
    {
        get
        {
            return palmsNum;
        }
        set
        {
            if (value < 0) value = 0;
            palmsNum = value;
        }
    }
    public int BoxesNum
    {
        get
        {
            return boxesNum;
        }
        set
        {
            if (value < 0) value = 0;
            boxesNum = value;
        }
    }
    public int CloudsNum
    {
        get
        {
            return cloudsNum;
        }
        set
        {
            if (value < 0) value = 0;
            cloudsNum = value;
        }
    }
    public int TreasureCount
    {
        get
        {
            return treasureCount;
        }
        set
        {
            treasureCount = value;
        }
    }
    public int TreasureHit
    {
        get
        {
            return treasureHit;
        }
        set
        {
            treasureHit = value;
        }
    }
    public int Rating
    {
        get
        {
            return rating;
        }
        set
        {
            rating = value;
        }
    }
    public PlayerController.CharacterType Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
        }
    }
    public int SelectedSceneId
    {
        get
        {
            return selectedSceneId;
        }
        set
        {
            selectedSceneId = value;
        }
    }
    public string OwnerGroup
    {
        get
        {
            return ownerGroup;
        }
        set
        {
            ownerGroup = value;
        }
    }
    public string Creator
    {
        get
        {
            return creator;
        }
        set
        {
            creator = value;
        }
    }
    public SceneState State
    {
        get
        {
            return sceneState;
        }
        set
        {
            sceneState = value;
        }
    }
    public Authorization Permissions
    {
        get
        {
            return permissionSet;
        }
        set
        {
            permissionSet = value;
        }
    }
    #endregion

    #region Ctor
    public DataContainer()
    {
        _instance = this;
    }
    #endregion

    #region Public Members
    public void SetDefault()
    {
        Dictionary<string, string> config = MyGame.Instance.ConfigParams;

        playMode = PlayMode.None;
        player = PlayerController.CharacterType.MALE;
        permissionSet = Authorization.All;
        cannonPower = Configuration.Cannon.MIN_POWER;
        cannonAngle = Configuration.Cannon.MIN_ANGLE;
        shotsNum = config.ContainsKey("shots_num") ? int.Parse(config["shots_num"]) : 10;
        shotsNumAttack = config.ContainsKey("shots_attack_num") ? int.Parse(config["shots_attack_num"]) : 10;
        flagsNum = config.ContainsKey("flags_num") ? int.Parse(config["flags_num"]) : 10;
        palmsNum = config.ContainsKey("palms_num") ? int.Parse(config["palms_num"]) : 10;
        boxesNum = config.ContainsKey("boxes_num") ? int.Parse(config["boxes_num"]) : 10;
        cloudsNum = config.ContainsKey("clouds_num") ? int.Parse(config["clouds_num"]) : 10;
        ownerGroup = string.Empty;
        sceneState = SceneState.New;
        treasureHit = 0;

        /*
        float startPos = config.ContainsKey("treasure_start_pos") ? float.Parse(config["treasure_start_pos"]) : 40.0f;
        float endPos = config.ContainsKey("treasure_end_pos") ? float.Parse(config["treasure_end_pos"]) : 300.0f;
        treasureCount = config.ContainsKey("treasure_num") ? int.Parse(config["treasure_num"]) : 4;
        float randomFactor = config.ContainsKey("treasure_random_factor") ? float.Parse(config["treasure_random_factor"]) : 1.0f;
        StaticObjectsManager.Instance.Initialize(startPos, endPos, treasureCount, randomFactor);
        */
        Debug.Log("++++++++++++++++++ treasureCount: " + treasureCount);
    }
    #endregion

    #region Save/Load Functionalities
    public void Save()
    {
        SaveMaterialData();
        SaveObjectsData();
        SaveTreasureData();

        Debug.Log("Save, SelectedSceneId: " + SelectedSceneId.ToString());
    }

    public void Load()
    {
        LoadMaterialData();
        LoadObjectsData();
        LoadTreasureData();

        if (playMode == PlayMode.None)
        {
            playMode = MyGame.Instance.currentPlayMode;
        }

        Debug.Log("Load, SelectedSceneId: " + SelectedSceneId.ToString());
    }

    #region Material Data
    private void SaveMaterialData()
    {
        int hBlockRulesNum = MaterialManager.Instance.HorizontalRules[MaterialManager.BLOCK].Count;
        horizontalBlockRules = new MaterialRule[hBlockRulesNum];
        for (int i = 0; i < hBlockRulesNum; i++)
            horizontalBlockRules[i] = MaterialManager.Instance.HorizontalRules[MaterialManager.BLOCK][i] as MaterialRule;

        int vBlockRulesNum = MaterialManager.Instance.VerticalRules[MaterialManager.BLOCK].Count;
        verticalBlockRules = new MaterialRule[vBlockRulesNum];
        for (int i = 0; i < vBlockRulesNum; i++)
            verticalBlockRules[i] = MaterialManager.Instance.VerticalRules[MaterialManager.BLOCK][i] as MaterialRule;

        int hTrampolineRulesNum = MaterialManager.Instance.HorizontalRules[MaterialManager.TRAMPOLINE].Count;
        horizontalTrampolineRules = new MaterialRule[hTrampolineRulesNum];
        for (int i = 0; i < hTrampolineRulesNum; i++)
            horizontalTrampolineRules[i] = MaterialManager.Instance.HorizontalRules[MaterialManager.TRAMPOLINE][i] as MaterialRule;

        int vTrampolineRulesNum = MaterialManager.Instance.VerticalRules[MaterialManager.TRAMPOLINE].Count;
        verticalTrampolineRules = new MaterialRule[vTrampolineRulesNum];
        for (int i = 0; i < vTrampolineRulesNum; i++)
            verticalTrampolineRules[i] = MaterialManager.Instance.VerticalRules[MaterialManager.TRAMPOLINE][i] as MaterialRule;
    }

    private void LoadMaterialData()
    {
        int hBlockRulesNum = horizontalBlockRules.Length;
        MaterialManager.Instance.HorizontalRules[MaterialManager.BLOCK].Clear();
        for (int i = 0; i < hBlockRulesNum; i++)
            MaterialManager.Instance.HorizontalRules[MaterialManager.BLOCK].Add(horizontalBlockRules[i]);

        int vBlockRulesNum = verticalBlockRules.Length;
        MaterialManager.Instance.VerticalRules[MaterialManager.BLOCK].Clear();
        for (int i = 0; i < vBlockRulesNum; i++)
            MaterialManager.Instance.VerticalRules[MaterialManager.BLOCK].Add(verticalBlockRules[i]);

        int hTrampolineRulesNum = horizontalTrampolineRules.Length;
        MaterialManager.Instance.HorizontalRules[MaterialManager.TRAMPOLINE].Clear();
        for (int i = 0; i < hTrampolineRulesNum; i++)
            MaterialManager.Instance.HorizontalRules[MaterialManager.TRAMPOLINE].Add(horizontalTrampolineRules[i]);

        int vTrampolineRulesNum = verticalTrampolineRules.Length;
        MaterialManager.Instance.VerticalRules[MaterialManager.TRAMPOLINE].Clear();
        for (int i = 0; i < vTrampolineRulesNum; i++)
            MaterialManager.Instance.VerticalRules[MaterialManager.TRAMPOLINE].Add(verticalTrampolineRules[i]);
    }
    #endregion

    #region Objects Data
    private void SaveObjectsData()
    {
        int objectNum = ObjectManager.Instance.ObjectList.Count;
        objectList = new MovableObject[objectNum];
        int i = 0;
        foreach (KeyValuePair<string, MovableObject> item in ObjectManager.Instance.ObjectList)
        {
            objectList[i] = item.Value;
            i++;
        }
    }

    private void LoadObjectsData()
    {
        ObjectManager.Instance.ClearObjects();
        int objectNum = objectList.Length;
        for (int i = 0; i < objectNum; i++)
        {
            //ObjectManager.Instance.ObjectList.Add(objectList[i].name, objectList[i]);
            ObjectManager.Instance.PlaceObject(objectList[i]);
        }
        ObjectManager.Instance.SelectedObject = null;
    }
    #endregion

    #region Treasure Data
    private void SaveTreasureData()
    {
        Dictionary<string, MovableObject> mvList = StaticObjectsManager.Instance.GetMovableObjects();
        int objectNum = mvList.Count;
        treasureList = new MovableObject[objectNum];
        int i = 0;
        foreach (KeyValuePair<string, MovableObject> item in mvList)
        {
            treasureList[i] = item.Value;
            i++;
        }
    }

    private void LoadTreasureData()
    {
        if (treasureList.Length > 0)
        {
            StaticObjectsManager.Instance.InitializeFromMovableObject(treasureList);
        }
    }
    #endregion
    #endregion
}
