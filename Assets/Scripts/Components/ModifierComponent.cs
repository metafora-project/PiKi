using UnityEngine;
using System.Collections;

public class ModifierComponent : MonoBehaviour
{
    #region Internal Data Structure
    public enum StateOperation
    {
        SELECTING = 0,
        MOVING
    }

    public enum MovingArea
    {
        GROUND = 0,
        SKY
    }
    #endregion

    #region Public Fields
    public MovingArea movingArea;
    public float heightPivot;
    public float bbWidth;
    public MovableObject mvObj;
    public StateOperation state = StateOperation.SELECTING;
    #endregion

    #region Protected Fields
    protected float minHeight = float.MinValue;
    protected float maxHeight = float.MaxValue;
    protected bool isGreen = false;
    protected bool showUI = false;
    protected bool bbJustInitialized = false;
    protected bool isJustCreated = false;
    protected Vector2 previousMousePosition = Vector2.zero;
    protected Vector3 movingOffset = Vector3.zero;
    protected Plane actionPlane;
    protected BBComponent bbComponent;
    protected PikiInterface interfaces;
    #endregion

    #region Protected Members
    protected void UpdateSelecting(Vector2 pos)
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousMousePosition = pos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if ((pos - previousMousePosition).magnitude < 2.0)
            {
                RaycastHit info;
                Ray ray = Camera.main.ScreenPointToRay(pos);
                if (gameObject.collider.Raycast(ray, out info, 300.0f))
                {
                    ObjectManager.Instance.SelectedObject = gameObject;
                    SelectObject();
                }
            }
        }
    }

    protected void UpdateMoving(Vector2 pos)
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(pos);
        bool picked = actionPlane.Raycast(ray, out distance);
        Vector3 mousePointerPos = Vector3.zero;
        if (picked)
            mousePointerPos = ray.GetPoint(distance) + movingOffset;

        if (mousePointerPos.x > Configuration.Objects.GROUND_LEFT && mousePointerPos.y > minHeight && mousePointerPos.y < maxHeight)
        {
            if (movingArea == MovingArea.GROUND)
            {
                mousePointerPos.y = -heightPivot;
            }

            if (bbComponent.CheckValidity())
            {
                isGreen = true;
                bbComponent.ShowGreen();
            }
            else
            {
                isGreen = false;
                bbComponent.ShowRed();
            }
        }
        else
        {
            isGreen = false;
            bbComponent.ShowRed();
        }

        transform.position = mousePointerPos;
        if (null != mvObj)
            mvObj.Position = transform.position;

        if (!bbJustInitialized) //skip the first pass
        {
            if (Input.GetMouseButtonUp(0))
            {
                FinalizeMoving(pos);
            }
        }
        bbJustInitialized = false;
    }

    protected Vector3 GetPositionOnScreen()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        return screenPos;
    }

    protected bool isSelectable()
    {
        Debug.Log("tag: " + gameObject.tag + ", " + DataContainer.Instance.playMode);
        bool launchObject = (gameObject.tag == "Block" || gameObject.tag == "Trampoline");

        bool launchFlag = launchObject && DataContainer.Instance.playMode == DataContainer.PlayMode.Launch;
        bool defenceFlag = !launchObject && DataContainer.Instance.playMode == DataContainer.PlayMode.Defence;

        return launchFlag || defenceFlag;
    }
    #endregion

    #region Public Members
    public void InitializeMoving(Vector2 pos)
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(pos);
        bool picked = actionPlane.Raycast(ray, out distance);
        if (picked)
            movingOffset = transform.position - ray.GetPoint(distance);

        state = StateOperation.MOVING;
        bbComponent.ShowGreen();

        bbJustInitialized = true;
    }

    public void FinalizeMoving(Vector2 pos)
    {
        if (isGreen)
        {
            Debug.Log("FinalizeMoving: " + gameObject.name);
            state = StateOperation.SELECTING;
            bbComponent.Hide();
            ObjectManager.Instance.SelectedObject = null;
            UnselectObject();

            MyGame.Instance.isSceneSaved = false;
            MyGame.Instance.SaveBinaryData("", true);


            if (gameObject.name.Contains("flag") || gameObject.name.Contains("block"))                 //Stone Sound
                ObjectManager.Instance.objectsSounds.PlayOneShot(ObjectManager.Instance.objectsSoundsClips[0]);
            else if (gameObject.name.Contains("trampoline") || gameObject.name.Contains("boxes"))        //WoodSound
                ObjectManager.Instance.objectsSounds.PlayOneShot(ObjectManager.Instance.objectsSoundsClips[1]);
            else
                ObjectManager.Instance.objectsSounds.PlayOneShot(ObjectManager.Instance.objectsSoundsClips[2]);                           //Palm Sound

            if (isJustCreated)
            {
                PikiObject _pObject = new PikiObject("2", XMPPBridge.OBSTACLE); 
                _pObject.ClearProperties();
                _pObject.SetPropertiesFromState();
                _pObject.SetProperty("OBJECT_TAG", (gameObject.tag.Equals("Flag") ? "Tower" : gameObject.tag));
                XMPPBridge.Instance.SendDenfenceObjectAddedIndicator(_pObject);
            }
            isJustCreated = false;
        }
    }

    public GameObject GetBBObject()
    {
        int childNum = transform.GetChildCount();
        if (childNum > 0)
        {
            for (int i = 0; i < childNum; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name == "BBox")
                    return child;
            }
        }
        return null;
    }
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        actionPlane = MyGame.Instance.actionPlane;
        bbComponent = gameObject.GetComponentInChildren<BBComponent>();
        bbComponent.SetWidth(bbWidth);
        //bbComponent.Hide();

        if (movingArea == MovingArea.GROUND)
        {
            minHeight = Configuration.Objects.GROUND_MIN;
            maxHeight = Configuration.Objects.GROUND_MAX;
        }
        else if (movingArea == MovingArea.SKY)
        {
            minHeight = Configuration.Objects.SKY_MIN;
            maxHeight = Configuration.Objects.SKY_MAX;
        }
        minHeight -= heightPivot;
        maxHeight -= heightPivot;

        //ObjectManager.Instance.DecrementObjectCount(gameObject.tag);
        interfaces = GameObject.Find("Interface").GetComponent<PikiInterface>();
	}

    void Start()
    {
        isJustCreated = true;
    }

	void Update()
    {
        Vector2 pos = Input.mousePosition;

        if (state == StateOperation.SELECTING)
            UpdateSelecting(pos);
        else if (state == StateOperation.MOVING)
            UpdateMoving(pos);
        
        if (showUI)
            interfaces.SendMessage("UpdateObjectOptionsPopup", GetPositionOnScreen());

        bbComponent.gameObject.transform.position = gameObject.transform.position;
    }
    #endregion

    #region Messages
    public void SelectObject()
    {
        if (isSelectable())
        {
            bool launchObject = (gameObject.tag == "Block" || gameObject.tag == "Trampoline");
            if (launchObject)
            {
                MaterialManager.Instance.CurrentMaterial = (gameObject.tag == "Block") ? MaterialManager.BLOCK : MaterialManager.TRAMPOLINE;
            }
            Debug.Log("SelectObject: " + gameObject.name);
            interfaces.SendMessage("ShowObjectOptionsPopup", GetPositionOnScreen());
            showUI = true;
        }
    }

    public void UnselectObject()
    {
        Debug.Log("UnselectObject: " + gameObject.name);
        interfaces.SendMessage("HideObjectOptionsPopup");
        showUI = false;
    }

    public void OnToolsButton()
    {

    }

    public void OnMoveButton()
    {
        Debug.Log("OnMoveButton: " + gameObject.name);
        if (state == StateOperation.SELECTING)
            InitializeMoving(Input.mousePosition);
    }

    public void OnRemoveButton()
    {
        ObjectManager.Instance.IncrementObjectCount(gameObject.tag);
        ObjectManager.Instance.RemoveSelected();
    }
    #endregion
}
