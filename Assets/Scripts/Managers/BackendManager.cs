using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CodeTitans.JSon;

public class BackendManager : MonoBehaviour
{
    #region Singleton
    private static BackendManager _instance = null;
    private static bool _isValid = false;
    public static BackendManager Instance
    {
        get
        {
            if (null == _instance)
            {
                UnityEngine.Object[] instances = FindObjectsOfType(typeof(BackendManager));
                DebugUtils.Assert(1 == instances.Length, "Singleton of type {0} have {1} instances!", typeof(BackendManager), instances.Length);
                _instance = instances[0] as BackendManager;
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
    const string Key = "192?@83eRn74sd65";
    const string UrlBase = "http://test.silentbaystudios.com/metafora_piki/services/";
    const string PublishSceneUrl = UrlBase + "publishScene.php";
    const string GetPublishedScenesUrl = UrlBase + "getPublishedScenes.php";
    const string CheckSceneNameUrl = UrlBase + "checkSceneName.php";
    const string GetPublishedScenesByPageUrl = UrlBase + "getPublishedScenesByPage.php";
    const string GetScenesCountUrl = UrlBase + "getScenesCount.php";
    const string RateSceneUrl = UrlBase + "rateScene.php";
    const string GetSceneByIdUrl = UrlBase + "getSceneById.php";
    const string VisitSceneUrl = UrlBase + "visitScene.php";
    const string WinSceneUrl = UrlBase + "winScene.php";
    const string GetPublishedScenesByPageAndFilterUrl = UrlBase + "getPublishedScenesByPageAndFilter.php";
    #endregion

    #region Static Members
    public static string URLEncode(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return string.Empty;

        string encoded = WWW.EscapeURL(txt, Encoding.UTF8);
        encoded = encoded.Replace("+", "%20");
        return encoded;
    }

    public static string URLDecode(string txt)
    {
        if (string.IsNullOrEmpty(txt))
            return string.Empty;

        string decoded = txt.Replace("%20", "+");
        decoded = WWW.UnEscapeURL(decoded, Encoding.UTF8);
        return decoded;
    }
    #endregion

    #region Internal Data Structures
    public struct RequestData
    {
        public string url;
        public WWWForm postData;

        public RequestData(string kUrl, WWWForm kPostData)
        {
            url = kUrl;
            postData = kPostData;
        }
    }

    public class PublishedScene
    {
        public int id;
        public string name;
        public string filename;
        public string docid;
        public string owner;
        public string publish_date;
        public int match_played;
        public int match_won;
        public int rating;

        public static PublishedScene Parse(IJSonObject jsonObj)
        {
            PublishedScene scene = new PublishedScene();
            scene.id = jsonObj["id"].Int32Value;
            scene.name = URLDecode(jsonObj["name"].StringValue);
            scene.filename = jsonObj["filename"].StringValue;
            scene.docid = jsonObj["docid"].StringValue;
            scene.owner = URLDecode(jsonObj["owner"].StringValue);
            scene.publish_date = jsonObj["publish_date"].StringValue;
            scene.match_played = jsonObj["match_played"].Int32Value;
            scene.match_won = jsonObj["match_won"].Int32Value;
            scene.rating = jsonObj["rating"].Int32Value;

            return scene;
        }
    }

    /*public class BoolValue
    {
        public static bool Parse(IJSonObject jsonObj)
        {
            return jsonObj.BooleanValue;
        }
    }*/
    #endregion

    #region Protected Fields
    protected int numCalls = 0;

    public int NumCalls
    {
        get { return numCalls; }
        set { numCalls = Mathf.Max(0, value); }
    }
    #endregion

    #region Protected Fields
    protected bool loadingShown = false;
    protected bool isConsumingRequest = false;
    protected string currentScenName = string.Empty;
    protected List<RequestData> requestList = new List<RequestData>();
    protected WWW currentRequest;
    protected List<PublishedScene> publishedScenes = new List<PublishedScene>();
    #endregion

    #region Protected Members
    protected string CalculateMD5Hash(string input)
    {
        MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString();
    }

    protected string Sign(string concatenatedParams)
    {
        return CalculateMD5Hash(concatenatedParams + Key).ToLower();
    }

    protected void AddRequest(string url, Dictionary<string, string> parameters)
    {
        string signSource = "";
        if (null != parameters)
        {
            foreach (KeyValuePair<string, string> param in parameters)
                signSource += param.Value;
        }

        WWWForm postData = new WWWForm();
        if (null != parameters)
        {
            foreach (KeyValuePair<string, string> param in parameters)
                postData.AddField(param.Key, param.Value);
        }
        postData.AddField("sign", Sign(signSource));
        postData.AddField("nocache", Random.Range(int.MinValue, int.MaxValue));
        Debug.Log(signSource + ", [" + Sign(signSource) + "]");

        requestList.Add(new RequestData(url, postData));
    }

    protected void ConsumeRequest()
    {
        if (requestList.Count > 0 && !isConsumingRequest)
        {
            isConsumingRequest = true;
            RequestData request = requestList[0];
            currentRequest = new WWW(request.url, request.postData);
            StartCoroutine(WaitResponse());
            requestList.RemoveAt(0);
            Debug.Log("Starting cousuming " + request.url);
            NumCalls++;
        }
    }

    protected void HandleServerError(WWW request)
    {
        MyGame.Instance.Interface.ShowConnectionError();
        Debug.Log("HandleServerError: " + request.error);
        NumCalls--;
    }

    protected void HandleServerResponse(WWW request)
    {
        JSonReader reader = new JSonReader();
        IJSonObject json = reader.ReadAsJSonObject(request.text);

        bool result = false;
        if (json.Contains("result"))
            result = json["result"].BooleanValue;

        if (json.Contains("obj"))
        {
            if (result)
            {
                if (request.url.Equals(PublishSceneUrl))
                    HandlePublishScene(json["obj"]);
                else if (request.url.Equals(GetPublishedScenesUrl))
                    HandleGetPublishedScenes(json["obj"]);
                else if (request.url.Equals(CheckSceneNameUrl))
                    HandleCheckSceneName(json["obj"]);
                else if (request.url.Equals(GetPublishedScenesByPageUrl))
                    HandleGetPublishedScenes(json["obj"]);
                else if (request.url.Equals(GetScenesCountUrl))
                    HandleGetScenesCount(json["obj"]);
                else if (request.url.Equals(RateSceneUrl))
                    HandleRateScene(json["obj"]);
                else if (request.url.Equals(GetSceneByIdUrl))
                    HandleGetSceneById(json["obj"]);
                else if (request.url.Equals(GetPublishedScenesByPageAndFilterUrl))
                    HandleGetPublishedScenes(json["obj"]);
            }
            else
            {
                Debug.Log("ERROR: " + json["obj"]["description"]);
            }
        }

        NumCalls--;
    }

    protected void HandleGetSceneById(IJSonObject jsonObj)
    {
        IEnumerable<IJSonObject> sceneArray;
        sceneArray = jsonObj.ArrayItems;
        foreach (IJSonObject scene in sceneArray)
            MyGame.Instance.Interface.SelectedScene = PublishedScene.Parse(scene);
    }

    protected void HandleRateScene(IJSonObject jsonObj)
    {
        bool result = jsonObj.BooleanValue;
        if (result)
            DataContainer.Instance.sceneState = DataContainer.SceneState.Rated;
        else
            DataContainer.Instance.sceneState = DataContainer.SceneState.Finished;
        Debug.Log("HandleRateScene, result: " + result);
        MyGame.Instance.SaveBinaryData("");
    }

    protected void HandlePublishScene(IJSonObject jsonObj)
    {
        MyGame.Instance.Interface.ShowHidePublishPopup(false);
        MyGame.Instance.Interface.State = PikiInterface.DefenceRewardPage;

        PikiObject pObject = new PikiObject("", XMPPBridge.SCENE);
        pObject.Id = "Scene published";
        pObject.ClearProperties();
        pObject.SetLandmarkPropertiesFromState();
        pObject.Properties.Add("SCENE_NAME", currentScenName);
        XMPPBridge.Instance.SendPublishSceneLandmark(pObject);
    }

    protected void HandleCheckSceneName(IJSonObject jsonObj)
    {
        //bool result = BoolValue.Parse(jsonObj);
        bool result = jsonObj.BooleanValue;
        if (result)
        {
            DataContainer.Instance.playMode = DataContainer.PlayMode.None;
            DataContainer.Instance.sceneState = DataContainer.SceneState.Published;
            DataContainer.Instance.Creator = DataContainer.Instance.OwnerGroup;
            DataContainer.Instance.OwnerGroup = string.Empty;
            DataContainer.Instance.CannonAngle = Configuration.Cannon.MIN_ANGLE;
            DataContainer.Instance.CannonPower = Configuration.Cannon.MIN_POWER;
            MyGame.Instance.saveCounter++;
            MyGame.Instance.SaveBinaryData("");
        }
        else
        {
            MyGame.Instance.Interface.ShowPublishPopupMessage("The scene's name is not available");
        }
    }

    protected void HandleGetPublishedScenes(IJSonObject jsonObj)
    {
        IEnumerable<IJSonObject> sceneArray;
        sceneArray = jsonObj.ArrayItems;
        publishedScenes.Clear();
        foreach (IJSonObject scene in sceneArray)
        {
            publishedScenes.Add(PublishedScene.Parse(scene));
        }
        if (MyGame.Instance.Interface.State == PikiInterface.HighscorePage)
            MyGame.Instance.Interface.CreateHighscoreList(publishedScenes);
        else
            MyGame.Instance.Interface.CreateSceneList(publishedScenes);
    }

    protected void HandleGetScenesCount(IJSonObject jsonObj)
    {
        int count = jsonObj.Int32Value;
        if (MyGame.Instance.Interface.State == PikiInterface.HighscorePage)
            MyGame.Instance.Interface.SetTotalPageHighscoreNum(count);
        else
            MyGame.Instance.Interface.SetTotalPageNum(count);
    }
    #endregion

    #region Public Members
    public void PublishScene(string docId, string owner)
    {
        Dictionary<string, string> parameters = new Dictionary<string,string>();
        parameters.Add("name", currentScenName);
        parameters.Add("filename", "piki_" + currentScenName);
        parameters.Add("docid", docId);
        parameters.Add("owner", URLEncode(owner));
        parameters.Add("istest", XMPPBridge.Instance.IsTestServer.ToString());

        AddRequest(PublishSceneUrl, parameters);
        Debug.Log("Added " + PublishSceneUrl);
    }

    public void GetPublishedScenes(int page, int size)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("page", page.ToString());
        parameters.Add("size", size.ToString());
        parameters.Add("istest", XMPPBridge.Instance.IsTestServer.ToString());

        AddRequest(GetPublishedScenesByPageUrl, parameters);
        Debug.Log("Added " + GetPublishedScenesByPageUrl);
    }

    public void GetPublishedScenes(int page, int size, string filter, string order)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("page", page.ToString());
        parameters.Add("size", size.ToString());
        parameters.Add("filter", filter);
        parameters.Add("order", order);
        parameters.Add("istest", XMPPBridge.Instance.IsTestServer.ToString());

        AddRequest(GetPublishedScenesByPageAndFilterUrl, parameters);
        Debug.Log("Added " + GetPublishedScenesByPageAndFilterUrl);
    }

    public void GetSceneById(int id)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("id", id.ToString());

        AddRequest(GetSceneByIdUrl, parameters);
        Debug.Log("Added " + GetSceneByIdUrl);
    }
    
    public void GetPublishedScene()
    {
        AddRequest(GetPublishedScenesUrl, null);
        Debug.Log("Added " + GetPublishedScenesUrl);
    }

    public void CheckSceneName(string name)
    {
        currentScenName = URLEncode(name.Trim());
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("name", currentScenName);
        parameters.Add("istest", XMPPBridge.Instance.IsTestServer.ToString());

        AddRequest(CheckSceneNameUrl, parameters);
        Debug.Log("Added " + CheckSceneNameUrl);
    }
    
    public void GetScenesCount()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("istest", XMPPBridge.Instance.IsTestServer.ToString());

        AddRequest(GetScenesCountUrl, parameters);
        Debug.Log("Added " + GetScenesCountUrl);
    }

    public void RateScene(int id, int rating)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("id", id.ToString());
        parameters.Add("rating", rating.ToString());

        AddRequest(RateSceneUrl, parameters);
        Debug.Log("Added " + RateSceneUrl);
    }

    public void VisitScene(int id)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("id", id.ToString());

        AddRequest(VisitSceneUrl, parameters);
        Debug.Log("Added " + VisitSceneUrl);
    }

    public void WinScene(int id)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("id", id.ToString());

        AddRequest(WinSceneUrl, parameters);
        Debug.Log("Added " + WinSceneUrl);
    }   
    #endregion

    #region Coroutines
    IEnumerator WaitResponse()
    {
        yield return currentRequest;

        if (currentRequest.error != null)
        {
            HandleServerError(currentRequest);
        }
        else
        {
            HandleServerResponse(currentRequest);
        }

        isConsumingRequest = false;
    }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        //GetPublishedScene();
    }

    void Update()
    {
        ConsumeRequest();

        if (NumCalls > 0 && !loadingShown)
        {
            loadingShown = true;
            MyGame.Instance.Interface.ShowLoadingPopup();
        }
        else if (NumCalls == 0 && loadingShown)
        {
            loadingShown = false;
            MyGame.Instance.Interface.HideLoadingPopup();
        }
    }
    #endregion
}
