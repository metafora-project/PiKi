using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticObjectsManager: MonoBehaviour
{
    #region Singleton
    private static StaticObjectsManager _instance = null;
    private static bool _isValid = false;
    public static StaticObjectsManager Instance
    {
        get
        {
            if (null == _instance)
            {
                UnityEngine.Object[] instances = FindObjectsOfType(typeof(StaticObjectsManager));
                DebugUtils.Assert(1 == instances.Length, "Singleton of type {0} have {1} instances!", typeof(StaticObjectsManager), instances.Length);
                _instance = instances[0] as StaticObjectsManager;
                _isValid = true;
            }

            return _instance;
        }
    }
    #endregion

    #region Static Members
    
    public static int MIN_LOW_SCORE = 250; // * 10 
    public static int MAX_LOW_SCORE = 350; // * 10
    public static int MIN_HIGH_SCORE = 450; // * 10 
    public static int MAX_HIGH_SCORE = 550; // * 10

    #endregion

    #region Public Members
    public StaticObject treasureSource;
    //public StaticObject obstacleSource;
    public int treasureNumber;
    public float initialX;
    public float finalX;
    public float randomPositionFactor;

    //public StaticObject obstacleSource;
    //public int obstaclesNumber;
    #endregion

    #region Protected Members
    public GameObject[] treasureList;
    protected float[] treasuresPos;
    //protected GameObject[] obstaclesList;
    protected float[] obstaclesPos;
    protected int obstaclePosIndex;
    #endregion

    [System.Serializable]
    public class StaticObject
    {
        public GameObject obj;
        public float objWidth;

        public StaticObject(GameObject o, float w)
        {
            obj = o;
            objWidth = w;
        }
    }

    /*public void Initialize()
    {
        float totalDistance = finalX - initialX;

        treasureList = new GameObject[treasureNumber];
        treasuresPos = new float[treasureNumber];
        PlaceTreasures(totalDistance, treasureList, treasureSource.objWidth, randomPositionFactor);
    }*/

    public void Initialize(float startPos, float endPos, int objNum, float randomFactor)
    {
        float totalDistance = endPos - startPos;

        treasureList = new GameObject[objNum];
        treasuresPos = new float[objNum];

        int[] scores = new int[objNum];
        for (int i = 0; i < objNum; i++)
        {
            int r1 = Random.Range(0, 100);
            if (r1 > 50)
            {
                scores[i] = Random.Range(MIN_LOW_SCORE, MAX_LOW_SCORE) * 10;
            }
            else
            {
                scores[i] = Random.Range(MIN_HIGH_SCORE, MAX_HIGH_SCORE) * 10;
            }
        }

        PlaceTreasures(totalDistance, treasureList, treasureSource.objWidth, randomFactor, scores);
    }

    public void InitializeFromMovableObject(MovableObject[] mvObj)
    {
        treasureList = new GameObject[mvObj.Length];
        int i = 0;
        foreach (MovableObject mv in mvObj)
        {
            treasureList[i] = PlaceObject(mv.Position, i, mv.score, mv.hit);
            i++;
        }
    }

    GameObject PlaceObject(Vector3 position, int _objCounter, int score, bool hit)
    {
        GameObject obj = null;
        obj = Instantiate(treasureSource.obj, position, Quaternion.identity) as GameObject;
        obj.transform.parent = gameObject.transform;
        obj.name = "Treasure_" + _objCounter;

        TreasureComponent trComp = obj.GetComponent<TreasureComponent>();
        trComp.Score = score;
        if (hit) trComp.Hitted();

        ObjectManager.Instance.AddBBObject(trComp.GetBBObject());
        return obj;
    }

    void PlaceTreasures(float totalDistance, GameObject[] objsList, float objsWidth, float randPosFactor, int[] scores)
    {
        float objectInterval = totalDistance / objsList.Length ;
        float halfObjWidth = objsWidth * 0.5f;

        if (objectInterval > objsWidth)
        {
            for (int i = 0; i < objsList.Length; i++)
            {
                treasuresPos[i] = (initialX + objectInterval * i + objectInterval * 0.5f) + Random.Range((-objectInterval * 0.5f + halfObjWidth) * randPosFactor, (objectInterval * 0.5f - halfObjWidth) * randPosFactor);
                objsList[i] = PlaceObject(new Vector3(treasuresPos[i], this.gameObject.transform.position.y - 1.1f, this.gameObject.transform.position.z), i, scores[i], false);
            }
        }
    }

    public Dictionary<string, MovableObject> GetMovableObjects()
    {
        Dictionary<string, MovableObject> mvList = new Dictionary<string, MovableObject>();
        if (null != treasureList)
        {
            foreach (GameObject go in treasureList)
            {
                TreasureComponent trComp = go.GetComponent<TreasureComponent>();
                mvList.Add(go.name, new MovableObject(ObjectManager.TREASURE, go, trComp.Score, trComp.Hit));
            }
        }
        return mvList;
    }

    //void CalculateObstaclePossiblePositions()
    //{
    //    int k = 0;
    //    for (int i = 1; i < treasureNumber; i++)
    //    {
    //        if (treasuresPos[i] - treasuresPos[i - 1] >= treasureSource.objWidth + obstacleSource.objWidth * 3)
    //        { 
    //            obstaclesPos[k] = 
    //        }
    //        else if (treasuresPos[i] - treasuresPos[i - 1] >= treasureSource.objWidth + obstacleSource.objWidth * 2)
    //        { }
    //        else if (treasuresPos[i] - treasuresPos[i - 1] >= treasureSource.objWidth + obstacleSource.objWidth)
    //        { 
    //        }
    //    }
    //}

    //void PlaceObstacles(float[] treasuresPositions, GameObject[] objsList, float objsWidth)
    //{
        //for (int i = 0; i < objsList.Length; i++)
        //{
        //    obstaclesPos[i] = 
        //    treasuresPos[i] = (initialX + objectInterval * i + objectInterval * 0.5f) + Random.Range((-objectInterval * 0.5f + halfObjWidth) * randPosFactor, (objectInterval * 0.5f - halfObjWidth) * randPosFactor);
        //    objsList[i] = PlaceObject(new Vector3(treasuresPos[i], this.gameObject.transform.position.y, this.gameObject.transform.position.z), i, "Treasure_");
        //}
    //}


    public static void Shuffle<T>(T[] array)
    {
        //int random = UnityEngine.Random.Range(0,1);
        for (int i = array.Length; i > 1; i--)
        {
            // Pick random element to swap.
            int j = UnityEngine.Random.Range(0, i - 1);
            // Swap.
            T tmp = array[j];
            array[j] = array[i - 1];
            array[i - 1] = tmp;
        }
    }

}
