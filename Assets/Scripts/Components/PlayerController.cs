using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region Internal Data Structure
    public enum CharacterType
    {
        MALE = 0,
        FEMALE
    };
    #endregion

    #region Public Fields
    public CharacterType type;
    public GameObject otherPlayer;
    //public GameObject initialPlaceholder;
    public GameObject gamePlaceholder;
    #endregion

    #region Protected Fields
    protected FiniteStateMachine fsm;
    #endregion

    #region Get/Set Modifiers
    public FiniteStateMachine FSM
    {
        get
        {
            return fsm;
        }
    }
    #endregion

    #region Player States
    public static IdleState IdleStateState = new IdleState();
    public static SelectedState SelectedStateState = new SelectedState();
    public static FireState FireStateState = new FireState();

    #region IdleState State
    public class IdleState : FiniteStateMachine.StateBase
    {
        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            self.GameObject.animation.Play("stay");
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
    }
    #endregion

    #region SelectedState State
    public class SelectedState : FiniteStateMachine.StateBase
    {
        protected bool _fading;
        protected PlayerController playerController;
        protected PikiInterface interfaces;

        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            playerController = self.GameObject.GetComponent<PlayerController>();
            interfaces = GameObject.Find("Interface").GetComponent<PikiInterface>();
            self.GameObject.animation["selected"].speed = (playerController.type == CharacterType.MALE) ? 1.3f : 2.0f;
            self.GameObject.animation.Play("selected");
            _fading = false;
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
            if (MyGame.Instance.FSM.State == GameplayStates.OffgameStateState)
            {
                if (!_fading && !self.GameObject.animation.IsPlaying("selected"))
                {
                    GameCamera.Instance.Fader.FadeOut(CameraStates.FadeTimer);
                    _fading = true;
                }

                if (_fading)
                {
                    if (!GameCamera.Instance.Fader.IsFading)
                    {
                        playerController.PlaceCharacter();
                        MyGame.Instance.FSM.State = GameplayStates.InitialStateState;
                        interfaces.State = PikiInterface.IngameLaunchPage;
                    }
                }
            }
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
    }
    #endregion

    #region FireState State
    public class FireState : FiniteStateMachine.StateBase
    {
        protected PlayerController playerController;

        public override void OnEnter(FiniteStateMachine.ObjectBase self, float time)
        {
            playerController = self.GameObject.GetComponent<PlayerController>();
            self.GameObject.animation["fire"].speed = 2.0f;
            self.GameObject.animation.Play("fire");
        }

        public override void OnExec(FiniteStateMachine.ObjectBase self, float time)
        {
            if (!self.GameObject.animation.IsPlaying("fire"))
                playerController.FSM.State = PlayerController.IdleStateState;
        }

        public override void OnExit(FiniteStateMachine.ObjectBase self, float time)
        {
        }
    }
    #endregion
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        fsm = gameObject.AddComponent<FiniteStateMachine>();
        fsm.State = IdleStateState;
    }

    void Start()
    {
        //transform.position = initialPlaceholder.transform.position;
        if (DataContainer.Instance.sceneState == DataContainer.SceneState.Opened && DataContainer.Instance.playMode == DataContainer.PlayMode.Launch)
        {
            if (type == DataContainer.Instance.Player)
            {
                MyGame.Instance.Player = gameObject;
                PlaceCharacter();
            }
        }
    }

    void Update()
    {
        fsm.ForceUpdate();
    }

    void OnMouseUp()
    {
        if (null == MyGame.Instance.Player && MyGame.Instance.FSM.State == GameplayStates.OffgameStateState && MyGame.Instance.IsInputEnabled && MyGame.Instance.Interface.State == PikiInterface.ChooseCharacterPage)
        {
            MyGame.Instance.Player = gameObject;
            Debug.Log("Click on " + type);
            fsm.State = SelectedStateState;
            DataContainer.Instance.Player = type;
            DataContainer.Instance.OwnerGroup = MyGame.Instance.GroupId;
            MyGame.Instance.SaveBinaryData("", true);
            DataContainer.Instance.sceneState = DataContainer.SceneState.Opened;
        }
    }
    #endregion

    #region Public Members
    public void PlaceCharacter()
    {
        MyGame.Instance.Player = gameObject;
        Destroy(otherPlayer);
        fsm.State = IdleStateState;
        transform.position = gamePlaceholder.transform.position;
        transform.rotation = gamePlaceholder.transform.rotation;
    }

    public void HideCharacter()
    {
        fsm.State = IdleStateState;
        Vector3 newPos = new Vector3(0.0f, -100.0f, 0.0f);
        transform.position = newPos;
    }
    #endregion
}
