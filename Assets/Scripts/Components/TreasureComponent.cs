using UnityEngine;
using System.Collections;

public class TreasureComponent : MonoBehaviour
{
    #region Public fields
    public GameObject cofferTop;
    public AudioClip treasureAudioClip;
    #endregion

    #region Protected fields
    protected int score = 0;
    protected bool hit = false;
    protected Color AlphaZero = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    protected PikiInterface interfaces;
    protected GameObject highScoreChild;
    protected PikiObject pObject;

    protected AudioSource treasureAudio;
    
    #endregion

    #region Unity callbacks
    void Start ()
    {
        GameObject bb = GetBBObject();
        bb.renderer.material.color = AlphaZero;

        highScoreChild = gameObject.transform.FindChild("coffer/Treasure2").gameObject;
        highScoreChild.SetActiveRecursively(false);
        
        interfaces = GameObject.FindGameObjectWithTag("Interface").GetComponent<PikiInterface>();

        treasureAudio = gameObject.AddComponent<AudioSource>();
        treasureAudio.volume = 1.0f;
        treasureAudio.playOnAwake = false;
        treasureAudio.loop = false;

        //Debug.Log("TESORO DI FASCIA: " + IsHighScore() + " PUNTEGGIO: " + Score);

        pObject = new PikiObject("", XMPPBridge.TREASURE);
    }
    #endregion

    #region Public members
    public int Score
    {
        get { return score; }
        set { score = value; }
    }
    public bool Hit
    {
        get { return hit; }
        set { hit = value; }
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

    public bool IsHighScore()
    {
        return this.score >= StaticObjectsManager.MIN_HIGH_SCORE * 10;
    }
    #endregion

    #region Message
    public void Hitted()
    {
        if (!hit)
        {
            cofferTop.animation.Play();
            hit = true;
            MyGame.Instance.Score += score;
            MyGame.Instance.Interface.UpdateScore();

            if (MyGame.Instance.currentPlayMode == DataContainer.PlayMode.Launch)
            {
                DataContainer.Instance.TreasureHit++;

                highScoreChild.SetActiveRecursively(IsHighScore());
                PikiInterface.FlyierData fData = new PikiInterface.FlyierData((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f), this.score.ToString());
                interfaces.SendMessage("CreateFlyier", fData);
                treasureAudio.PlayOneShot(treasureAudioClip);

                pObject.Id = "Treasure hit";
                pObject.ClearProperties();
                pObject.SetLandmarkPropertiesFromState();
                pObject.Properties.Add("TREASURE_SCORE", score.ToString());
                pObject.Properties.Add("TREASURE_CATEGORY", IsHighScore() ? "HIGH" : "LOW");
                XMPPBridge.Instance.SendTreasureHitLandmark(pObject);

                if (DataContainer.Instance.TreasureHit >= DataContainer.Instance.TreasureCount)
                {
                    pObject.Id = "All treasures hit";
                    pObject.ClearProperties();
                    pObject.SetLandmarkPropertiesFromState();
                    pObject.Properties.Add("TREASURES_COUNT", DataContainer.Instance.TreasureCount.ToString());
                    XMPPBridge.Instance.SendAllTreasuresHitLandmark(pObject);
                }

                MyGame.Instance.SaveBinaryData("", true);
            }
        }
    }
    #endregion
}
