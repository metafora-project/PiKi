using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectManager:MonoBehaviour {

    #region Singleton
    private static ObjectManager _instance = null;
    private static bool _isValid = false;
    public static ObjectManager Instance
    {
        get
        {
            if (null == _instance)
            {
                UnityEngine.Object[] instances = FindObjectsOfType(typeof(ObjectManager));
                DebugUtils.Assert(1 == instances.Length, "Singleton of type {0} have {1} instances!", typeof(ObjectManager), instances.Length);
                _instance = instances[0] as ObjectManager;
                _isValid = true;
            }

            return _instance;
        }
    }

    public static bool IsValid
    {
        get
        {
            return _isValid;
        }
    }
    #endregion

    #region Constants
    public const int HOOP = 0;
    public const int BLOCK = 1;
    public const int TRAMPOLINE = 2;
    public const int FLAG = 3;
    public const int PALM = 4;
    public const int BOXES = 5;
    public const int CLOUD = 6;
    public const int TREASURE = 7;
    #endregion

    #region Public Fields
    public GameObject hoopSource;
    public GameObject blockSource;
    public GameObject trampolineSource;
    public GameObject flagSource;
    public GameObject palmSource;
    public GameObject boxesSource;
    public GameObject cloudSource;
    public GameObject _selectedObject;
    public Dictionary<string, MovableObject> objectList;
    public List<GameObject> bbList;
    public AudioSource objectsSounds;
    public AudioClip[] objectsSoundsClips;
    #endregion

    #region Protected Fields
    protected int _objCounter;
    protected PikiInterface interfaces;
    #endregion

    #region Get/Set
    public GameObject SelectedObject
    {
        get { return _selectedObject; }
        set
        {
            ClearSelected();
            _selectedObject = value;
        }
    }

    public Dictionary<string, MovableObject> ObjectList
    {
        get { return objectList; }
        set { objectList = value; }
    }
    #endregion

    #region Protected Members
    protected GameObject InstantiateGameObject(int type, Vector3 position)
    {
        GameObject obj = null;
        switch (type)
        {
            case (HOOP):
                obj = Instantiate(hoopSource, position, Quaternion.identity) as GameObject;
                obj.name = "hoop_" + _objCounter;
                break;
            case (BLOCK):
                obj = Instantiate(blockSource, position, Quaternion.identity) as GameObject;
                obj.name = "block_" + _objCounter;
                MaterialManager.Instance.CurrentMaterial = MaterialManager.BLOCK;
                break;
            case (TRAMPOLINE):
                obj = Instantiate(trampolineSource, position, Quaternion.identity) as GameObject;
                obj.name = "trampoline_" + _objCounter;
                MaterialManager.Instance.CurrentMaterial = MaterialManager.TRAMPOLINE;
                break;
            case (FLAG):
                obj = Instantiate(flagSource, position, Quaternion.identity) as GameObject;
                obj.name = "flag_" + _objCounter;
                break;
            case (PALM):
                obj = Instantiate(palmSource, position, Quaternion.identity) as GameObject;
                obj.name = "palm_" + _objCounter;
                break;
            case (BOXES):
                obj = Instantiate(boxesSource, position, Quaternion.identity) as GameObject;
                obj.name = "boxes_" + _objCounter;
                break;
            case (CLOUD):
                obj = Instantiate(cloudSource, position, Quaternion.identity) as GameObject;
                obj.name = "cloud_" + _objCounter;
                break;
        }
        obj.transform.parent = gameObject.transform;
        ModifierComponent modComp = obj.GetComponent<ModifierComponent>();
        bbList.Add(modComp.GetBBObject());
        return obj;
    }
    #endregion

    #region Public Members
    public void PlaceObject(int type, Vector3 position)
    {
        if (null != _selectedObject)
        {
            if (_selectedObject.GetComponent<ModifierComponent>().state == ModifierComponent.StateOperation.MOVING)
            {
                DecrementObjectCount(_selectedObject.tag);
                _selectedObject.GetComponent<ModifierComponent>().OnRemoveButton();
            }
        }

        GameObject obj = InstantiateGameObject(type, position);
        MovableObject mvObj = new MovableObject(type, obj);
        obj.GetComponent<ModifierComponent>().mvObj = mvObj;
        obj.GetComponent<ModifierComponent>().InitializeMoving(Input.mousePosition);
        ObjectList.Add(mvObj.obj.name, mvObj);
        SelectedObject = mvObj.obj;
        mvObj.obj.SendMessage("SelectObject");
        _objCounter++;
    }

    public void PlaceObject(MovableObject mvObj)
    {
        GameObject obj = InstantiateGameObject(mvObj.type, mvObj.Position);
        obj.GetComponent<ModifierComponent>().mvObj = mvObj;
        mvObj.obj = obj;
        Debug.Log("mvObj.obj.name: " + mvObj.obj.name);
        ObjectList.Add(mvObj.obj.name, mvObj);
        //SelectedObject = mvObj.obj;
        mvObj.obj.SendMessage("UnselectObject");
        _objCounter++;        
    }

    public void ClearSelected()
    {
        if (null != _selectedObject)
        {
            _selectedObject.SendMessage("UnselectObject");
            if (_selectedObject.GetComponent<ModifierComponent>().state == ModifierComponent.StateOperation.MOVING)
            {
                _selectedObject.GetComponent<ModifierComponent>().OnRemoveButton();
            }
        }

    }

    public bool RemoveSelected()
    {
        if (null != _selectedObject)
        {
            ModifierComponent modComp = _selectedObject.GetComponent<ModifierComponent>();
            bbList.Remove(modComp.GetBBObject());

            foreach (KeyValuePair<string, MovableObject> item in ObjectList)
            {
                if (item.Value.obj == _selectedObject)
                {
                    Destroy(item.Value.obj);
                    item.Value.obj = null;
                    ObjectList.Remove(item.Key);
                    return true;
                }

            }
        }
        return false;            
    }

    public void ClearObjects()
    {
        foreach (KeyValuePair<string, MovableObject> item in ObjectList)
        {
            Destroy(item.Value.obj);
            item.Value.obj = null;
        }
        ObjectList.Clear();
        bbList.Clear();
    }

    public bool CheckBounds(GameObject go)
    {
        foreach (GameObject item in bbList)
        {
            if (go != item)
            {
                if (go.collider.bounds.Intersects(item.collider.bounds))
                    return true;
            }
        }
        return false;
    }

    public void AddBBObject(GameObject go)
    {
        bbList.Add(go);
    }

    public int GetObjectCount(string tag)
    {
        switch (tag)
        {
            case "Boxes":
                return DataContainer.Instance.BoxesNum;
            case "Palm":
                return DataContainer.Instance.PalmsNum;
            case "Flag":
                return DataContainer.Instance.FlagsNum;
            case "Cloud":
                return DataContainer.Instance.CloudsNum;
        }
        return 0;
    }

    public void DecrementObjectCount(string tag)
    {
        switch (tag)
        {
            case "Boxes":
                DataContainer.Instance.BoxesNum--;
                interfaces.UpdateCounter(PikiInterface.CounterType.crates, DataContainer.Instance.BoxesNum);
                break;
            case "Palm":
                DataContainer.Instance.PalmsNum--;
                interfaces.UpdateCounter(PikiInterface.CounterType.palm, DataContainer.Instance.PalmsNum);
                Debug.Log("DecrementObjectCount PALM " + DataContainer.Instance.PalmsNum);
                break;
            case "Flag":
                DataContainer.Instance.FlagsNum--;
                interfaces.UpdateCounter(PikiInterface.CounterType.flag, DataContainer.Instance.FlagsNum);
                break;
            case "Cloud":
                DataContainer.Instance.CloudsNum--;
                interfaces.UpdateCounter(PikiInterface.CounterType.clouds, DataContainer.Instance.CloudsNum);
                break;
        }
    }

    public void IncrementObjectCount(string tag)
    {
        switch (tag)
        {
            case "Boxes":
                DataContainer.Instance.BoxesNum++;
                interfaces.UpdateCounter(PikiInterface.CounterType.crates, DataContainer.Instance.BoxesNum);
                break;
            case "Palm":
                DataContainer.Instance.PalmsNum++;
                interfaces.UpdateCounter(PikiInterface.CounterType.palm, DataContainer.Instance.PalmsNum);
                break;
            case "Flag":
                DataContainer.Instance.FlagsNum++;
                interfaces.UpdateCounter(PikiInterface.CounterType.flag, DataContainer.Instance.FlagsNum);
                break;
            case "Cloud":
                DataContainer.Instance.CloudsNum++;
                interfaces.UpdateCounter(PikiInterface.CounterType.clouds, DataContainer.Instance.CloudsNum);
                break;
        }
    }
    #endregion

    #region Unity CallBacks
    void Awake()
    {
        _objCounter = 0;
        objectList = new Dictionary<string, MovableObject>();

        objectsSounds.loop = false;
        objectsSounds.playOnAwake = false;
        objectsSounds.volume = 0.8f;
    }

    void Start()
    {
        interfaces = GameObject.Find("Interface").GetComponent<PikiInterface>();
    }
    #endregion
}
