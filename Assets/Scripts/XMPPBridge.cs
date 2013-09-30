using UnityEngine;
using System;
using System.Collections.Generic;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using XML;

public class XMPPBridge : MonoBehaviour
{
    #region Singleton
    private static XMPPBridge _instance;
    private static bool _isValid;

    public static XMPPBridge Instance
    {
        get
        {
            if (null == _instance)
            {
                UnityEngine.Object[] instances = FindObjectsOfType(typeof(XMPPBridge));
                _instance = instances[0] as XMPPBridge;
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

    public const string CANNON = "Cannon";
    public const string TREASURE = "Treasure";
    public const string SCENE = "Scene";
    public const string OBSTACLE = "Obstacle";
    public const string PIKI_BASE = "http://test.silentbaystudios.com/metafora_piki/production";
    public const string PIKI_ADDRESS = PIKI_BASE + "/piki.php";
    public const string PIKI_BASE_TEST = "http://test.silentbaystudios.com/metafora_piki/test";
    public const string PIKI_ADDRESS_TEST = PIKI_BASE_TEST + "/piki.php";

    #region Private Constants
    private string SENDER = "piki@metafora.ku-eichstaett.de";
    private string PASSWORD = "piki";
    private string LOGGER_CHANNEL = "logger@conference.metafora.ku-eichstaett.de";
    private string ANALYSIS_CHANNEL = "analysis@conference.metafora.ku-eichstaett.de";
    private string COMMAND_CHANNEL = "command@conference.metafora.ku-eichstaett.de";
    private string TOOL = "PIKI";
    #endregion

    private XmppClientConnection _xmpp;
    private Jid _jidSender;

    private string _userID;
    private string[] _otherUsers = {};
    private string _roleID;
    private string _groupID = "test_group";
    private string _challengeID;
    private string _challengeName;
    private string _docId;
    private string _token;
    private bool _isTestServer = true;
    private string _ptNodeId;
    private string _ptMap;
    private bool _newXML;
    private bool _readyToReceive = false;

    private string feedbackMessage = String.Empty;
    private bool sendLowMessage = false;
    private bool sendHighMessage = false;

    #region Getters/Setters
    public string UserID { get { return _userID; } set { _userID = value; } }
    public string[] OtherUsers { get { return _otherUsers; } set { _otherUsers = value; } }
    public string RoleID { get { return _roleID; } set { _roleID = value; } }
    public string GroupID { get { return _groupID; } set { _groupID = value; } }
    public string ChallengeID { get { return _challengeID; } set { _challengeID = value; } }
    public string ChallengeName { get { return _challengeName; } set { _challengeName = value; } }
    public string DocId { get { return _docId; } set { _docId = value; } }
    public string Token { get { return _token; } set { _token = value; } }
    public bool IsTestServer { get { return _isTestServer; } set { _isTestServer = value; } }
    public string PtNodeId { get { return _ptNodeId; } set { _ptNodeId = value; } }
    public string PtMap { get { return _ptMap; } set { _ptMap = value; } }
    #endregion

    #region Unity Callbacks
    public void ForcedStart()
    {
        Debug.Log("XMPPBridge: ForcedStart " + IsTestServer);
        string testStr = (IsTestServer) ? "_test" : "";
        SENDER = MyGame.Instance.ConfigParams.ContainsKey("xmpp_sender" + testStr) ? MyGame.Instance.ConfigParams["xmpp_sender" + testStr] : "piki@metafora.ku-eichstaett.de";
        PASSWORD = MyGame.Instance.ConfigParams.ContainsKey("xmpp_password" + testStr) ? MyGame.Instance.ConfigParams["xmpp_password" + testStr] : "piki";
        LOGGER_CHANNEL = MyGame.Instance.ConfigParams.ContainsKey("xmpp_logger_channel" + testStr) ? MyGame.Instance.ConfigParams["xmpp_logger_channel" + testStr] : "logger@conference.metafora.ku-eichstaett.de";
        ANALYSIS_CHANNEL = MyGame.Instance.ConfigParams.ContainsKey("xmpp_analysis_channel" + testStr) ? MyGame.Instance.ConfigParams["xmpp_analysis_channel" + testStr] : "analysis@conference.metafora.ku-eichstaett.de";
        COMMAND_CHANNEL = MyGame.Instance.ConfigParams.ContainsKey("xmpp_command_channel" + testStr) ? MyGame.Instance.ConfigParams["xmpp_command_channel" + testStr] : "command@conference.metafora.ku-eichstaett.de";
        TOOL = "PIKI";

        InitializeXMPP();        

        _roleID = "student";
        _newXML = true; // IsTestServer;

        MyGame.Instance.Output = (IsTestServer) ? "*** TEST ***" : "*** PRODUCTION ***";
        MyGame.Instance.Output = "SENDER: " + SENDER;
        MyGame.Instance.Output = "PASSWORD: " + PASSWORD;
        MyGame.Instance.Output = "LOGGER_CHANNEL: " + LOGGER_CHANNEL;
        MyGame.Instance.Output = "ANALYSIS_CHANNEL: " + ANALYSIS_CHANNEL;
        MyGame.Instance.Output = "COMMAND_CHANNEL: " + COMMAND_CHANNEL;
	}

    protected void InitializeXMPP()
    {
        //The resource field is ignored by agsXMPP but by default is set to null, so the server give a random unique id
        _jidSender = new Jid(SENDER);
        _jidSender.Resource = "piki_" + DateTimeToMillisec(System.DateTime.UtcNow).ToString();
        _xmpp = new XmppClientConnection(_jidSender.Server);
        _xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
        _xmpp.OnMessage += new MessageHandler(xmpp_OnMessage);
        _xmpp.OnError += new ErrorHandler(xmpp_OnError);
        _xmpp.Open(_jidSender.User, PASSWORD);
    }

    void Update()
    {
        if (sendLowMessage)
        {
            sendLowMessage = false;
            Debug.Log("LOW: " + feedbackMessage);
            MyGame.Instance.OnShowLIF(feedbackMessage);
        }

        if (sendHighMessage)
        {
            sendHighMessage = false;
            Debug.Log("HIGH: " + feedbackMessage);
            MyGame.Instance.OnShowHIF(feedbackMessage);
        }
    }

    void OnDestroy()
    {
        Debug.Log("XMPPBridge: OnDestroy");
        if (null != _xmpp)
            _xmpp.Close();
    }
    /*
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 250, 150, 26), "Send LOG"))
        {
            LogMessage(LOGGER_CHANNEL, GetExampleMessage());
        }
        if (GUI.Button(new Rect(10, 280, 150, 26), "Send COMMAND"))
        {
            LogMessage(COMMAND_CHANNEL, GetExampleMessage());
        }
    }
    */
    void LogMessage(string channel, string body)
    {
        Debug.Log("*********************************** LogMessage to " + channel);
        if (channel.Equals(COMMAND_CHANNEL))
            Debug.Log(body);

        Message msg = new Message();
        msg.Type = MessageType.groupchat;
        msg.To = new Jid(channel);
        msg.Body = body;
        _xmpp.Send(msg);
        Debug.Log("*********************************** LogMessage SENT");
    }
    #endregion

    #region XMPP callbacks
    void xmpp_OnError(object sender, Exception ex)
    {
        Debug.Log("xmpp_OnError: " + ex.ToString());
    }

    void xmpp_OnLogin(object sender)
    {
        MucManager muc = new MucManager(_xmpp);
        muc.JoinRoom(new Jid(LOGGER_CHANNEL), _jidSender.Resource, true);
        muc.JoinRoom(new Jid(COMMAND_CHANNEL), _jidSender.Resource, true);
        muc.JoinRoom(new Jid(ANALYSIS_CHANNEL), _jidSender.Resource, true);
    }

    protected bool isReceveingHistory = true;
    protected float historyTimer = -1.0f;

    void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
    {
        //Debug.Log(((msg.Type == MessageType.error) ? "ERROR " : "") + "xmppCon_OnMessage: " + msg.From.Bare + ", " + msg.Body + ", " + msg.Error);
        if (msg.From.Bare.StartsWith("command"))
        {
            //Debug.Log("state: " + msg.Chatstate.ToString() + ", " + msg.Body);
            if (!_readyToReceive)
            {
                //cheat: avoid messages from history
                _readyToReceive = true; // msg.Body.Contains("has set the subject to");
                //if (_readyToReceive)
                //    MyGame.Instance.Output = "************************************** READY TO RECEIVE";
                if (_readyToReceive)
                    Debug.Log("************************************** READY TO RECEIVE");
            }
            else
            {
                //MyGame.Instance.Output = "Received message " + msg.Body;
                string type, ip, interruption, receiver;
                bool allInfo = ParseFeedbackXML(msg.Body, out type, out ip, out feedbackMessage, out interruption, out receiver);
                Debug.Log(msg.Body);
                if (allInfo)
                {
#if UNITY_EDITOR
                    Token = "1362392351935";
#endif
                    Debug.Log("Type: " + type + ", " + type.Equals("feedback").ToString());
                    Debug.Log("Token: " + ip + ", " + Token.ToLower() + ", " + ip.Equals(Token.ToLower()).ToString());
                    Debug.Log("Receiver: " + receiver + ", " + receiver.StartsWith("piki").ToString());

                    if (type.Equals("feedback") && ip.Equals(Token.ToLower()) && receiver.StartsWith("piki"))
                    {
                        if (interruption.StartsWith("high"))
                        {
                            sendHighMessage = true;
                            Debug.Log("HIGH: " + feedbackMessage + ", " + sendHighMessage);
                        }
                        else if (interruption.StartsWith("low"))
                        {
                            sendLowMessage = true;
                            Debug.Log("LOW: " + feedbackMessage + ", " + sendLowMessage);
                        }
                        else
                        {
                            Debug.Log("Neither HIGH of LOW: " + interruption);
                        }
                    }
                }
            }
        }
    }

    private bool ParseFeedbackXML(string xml, out string type, out string ip, out string text, out string interruption, out string receiver)
    {
        bool typeFlag = false,
             ipFlag = false,
             textFlag = false,
             intFlag = false,
             rcvFlag = false;

        type = "";
        ip = "";
        text = "";
        interruption = "";
        receiver = "";

        XMLReader xmlReader = new XMLReader();
        XMLNode xmlNode = (xmlReader.read(xml).children[0]) as XMLNode;
        foreach (XMLNode node in xmlNode.children)
        {
            switch (node.tagName)
            {
                case "actiontype":
                    if (node.attributes.ContainsKey("type"))
                    {
                        type = node.attributes["type"].ToLower();
                        typeFlag = true;
                        Debug.Log("actiontype " + type);
                    }
                    break;
                case "user":
                    if (node.attributes.ContainsKey("ip"))
                    {
                        ip = node.attributes["ip"].ToLower();
                        ipFlag = true;
                        Debug.Log("user " + ip);
                    }
                    break;
                case "object":
                    Dictionary<string, string> objProperties = ParseProperties(node);
                    if (objProperties.ContainsKey("text"))
                    {
                        text = objProperties["text"];
                        textFlag = true;
                        Debug.Log("object/text " + text);
                    }
                    if (objProperties.ContainsKey("interruption_type"))
                    {
                        interruption = objProperties["interruption_type"].ToLower();
                        intFlag = true;
                        Debug.Log("interruption_type " + interruption);
                    }
                    break;
                case "content":
                    Dictionary<string, string> contProperties = ParseProperties(node);
                    if (contProperties.ContainsKey("receiving_tool"))
                    {
                        receiver = contProperties["receiving_tool"].ToLower();
                        rcvFlag = true;
                        Debug.Log("content/receiving_tool " + receiver);
                    }
                    break;
            }
        }

        Debug.Log("XML parsed");
        
        return (typeFlag && ipFlag && textFlag && intFlag && rcvFlag);
    }

    private Dictionary<string, string> ParseProperties(XMLNode xmlNode)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();

        XMLNode propNode = null;
        foreach (XMLNode child in xmlNode.children)
        {
            if (child.tagName.Equals("properties"))
            {
                propNode = child;
            }
        }

        if (null != propNode)
        {
            foreach (XMLNode node in propNode.children)
            {
                if (node.tagName.Equals("property"))
                {
                    if (node.HasAttribute("name") && node.HasAttribute("value"))
                        ret.Add(node.attributes["name"].ToLower(), node.attributes["value"]);
                }
            }
        }
        return ret;
    }
    #endregion

    #region XML message functions
    public static Int64 DateTimeToMillisec(System.DateTime value)
    {
        System.TimeSpan span = (value - new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        return (Int64)(span.TotalSeconds * 1000.0);
    }

    private string BuildIndicator(PikiObject pObject, string indicatorType, string description)
    {
        Int64 millisec = DateTimeToMillisec(System.DateTime.UtcNow);
        string xml = "";
        if (_newXML)
        {
            xml += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n";
            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype type=\"indicator\" classification=\"other\" succeed=\"true\" logged=\"true\" />\n";
            foreach (string user in OtherUsers)
                xml += "<user id=\"" + user + "\" ip=\"" + Token + "\" role=\"originator\" />\n";
            xml += "<object id=\"" + pObject.Id + "\" type=\"" + pObject.Type + "\">\n";
            xml += "<properties>\n";
            foreach (KeyValuePair<string, string> property in pObject.Properties)
                xml += "<property name=\"" + property.Key + "\" value=\"" + property.Value + "\" />\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "<content>\n";
            xml += "<description><![CDATA[" + description + "]]></description>\n";
            xml += "<properties>\n";
            xml += "<property name=\"SENDING_TOOL\" value=\"" + TOOL + "\" />\n";
            xml += "<property name=\"GROUP_ID\" value=\"" + _groupID + "\" />\n";
            xml += "<property name=\"INDICATOR_TYPE\" value=\"" + indicatorType + "\" />\n";
            //xml += "<property name=\"TOOL\" value=\"" + TOOL + "\" />\n";
            xml += "<property name=\"CHALLENGE_ID\" value=\"" + _challengeID + "\" />\n";
            xml += "<property name=\"CHALLENGE_NAME\" value=\"" + _challengeName + "\" />\n";
            xml += "<property name=\"TOKEN\" value=\"" + Token + "\" />\n";
            xml += "</properties>\n";
            xml += "</content>\n";
            xml += "</action>\n";
        }
        else
        {
            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype type=\"indicator\" classification=\"other\" succeed=\"true\" logged=\"false\" />\n";
            xml += "<user id=\"" + _userID + "\" role=\"" + _roleID + "\" />\n";
            xml += "<object id=\"" + pObject.Id + "\" type=\"" + pObject.Type + "\">\n";
            xml += "<properties>\n";
            foreach (KeyValuePair<string, string> property in pObject.Properties)
                xml += "<property name=\"" + property.Key + "\" value=\"" + property.Value + "\" />\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "<content>\n";
            xml += "<description><![CDATA[" + description + "]]></description>\n";
            xml += "<properties>\n";
            xml += "<property name=\"INDICATOR_TYPE\" value=\"" + indicatorType + "\" />\n";
            xml += "<property name=\"TOOL\" value=\"" + TOOL + "\" />\n";
            xml += "<property name=\"GROUP_ID\" value=\"" + _groupID + "\" />\n";
            xml += "<property name=\"CHALLENGE_ID\" value=\"" + _challengeID + "\" />\n";
            xml += "<property name=\"CHALLENGE_NAME\" value=\"" + _challengeName + "\" />\n";
            xml += "</properties>\n";
            xml += "</content>\n";
            xml += "</action>\n";
        }
        return xml;
    }

    private string BuildNodeUrl(bool autosave)
    {
        Int64 millisec = DateTimeToMillisec(System.DateTime.UtcNow);
        string xml = "";
        if (_newXML)
        {
            xml += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n";
            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype classification=\"modify\" logged=\"false\" type=\"MODIFY_NODE_URL\"/>\n";
            foreach (string user in OtherUsers)
                xml += "<user id=\"" + user + "\" ip=\"" + Token + "\" role=\"originator\" />\n";
            xml += "<object id=\"" + PtNodeId + "\" type=\"PLANNING_TOOL_NODE\">\n";
            xml += "<properties>\n";
            xml += "<property name=\"RESOURCE_URL\" value=\"" + (IsTestServer ? PIKI_ADDRESS_TEST : PIKI_ADDRESS) + "?docId=" + DocId + "\"/>\n";
            xml += "<property name=\"PLANNING_TOOL_MAP\" value=\"" + PtMap + "\"/>\n";
            xml += "<property name=\"AUTOSAVE\" value=\"" + ((autosave) ? "TRUE" : "FALSE") + "\"/>\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "<content>\n";
            xml += "<property name=\"SENDING_TOOL\" value=\"" + (IsTestServer ? TOOL + "_TEST" : TOOL) + "\" />\n";
            xml += "<property name=\"RECEIVING_TOOL\" value=\"" + (IsTestServer ? "PLANNING_TOOL_TEST" : "PLANNING_TOOL") + "\" />\n";
            xml += "<property name=\"GROUP_ID\" value=\"" + _groupID + "\" />\n";
            xml += "<property name=\"CHALLENGE_ID\" value=\"" + _challengeID + "\" />\n";
            xml += "<property name=\"CHALLENGE_NAME\" value=\"" + _challengeName + "\" />\n";
            xml += "</content>\n";
            xml += "</action>\n";
        }
        else
        {
            xml += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n";
            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype classification=\"modify\" logged=\"true\" succeed=\"true\" type=\"modifyNodeUrl\"/>\n";
            xml += "<user id=\"" + UserID + "\" ip=\"" + Token + "\" role=\"originator\"/>\n";
            xml += "<user id=\"" + (IsTestServer ? "PlanningToolTest" : "PlanningTool") + "\" role=\"receiver\"/>\n";
            xml += "<object id=\"" + PtNodeId + "\" type=\"PlanningNode\">\n";
            xml += "<properties>\n";
            xml += "<property name=\"RESOURCE_URL\" value=\"" + PIKI_ADDRESS + "?docId=" + DocId + "\"/>\n";
            xml += "<property name=\"PLANNINGTOOLMAP\" value=\"" + PtMap + "\"/>\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "</action>\n";
        }
        return xml;
    }

    private string BuildLasadRefObj()
    {
        Int64 millisec = DateTimeToMillisec(System.DateTime.UtcNow);
        string xml = "";
        if (_newXML)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Clear();
            properties.Add("TEXT", "");
            properties.Add("VIEW_URL", (IsTestServer ? PIKI_BASE_TEST : PIKI_BASE) + "/piki_thumb.png");
            properties.Add("REFERENCE_URL", (IsTestServer ? PIKI_ADDRESS_TEST : PIKI_ADDRESS) + "?docId=" + DocId);
            properties.Add("OBJECT_HOME_TOOL", (IsTestServer ? TOOL + "_TEST" : TOOL));
            properties.Add("CHALLENGE_ID", ChallengeID);
            properties.Add("CHALLENGE_NAME", ChallengeName);
            properties.Add("TOKEN", Token);

            Dictionary<string, string> contents = new Dictionary<string, string>();
            contents.Clear();
            contents.Add("SENDING_TOOL", (IsTestServer ? TOOL + "_TEST" : TOOL));
            contents.Add("RECEIVING_TOOL", (IsTestServer ? "LASAD_TEST" : "LASAD"));
            contents.Add("CHALLENGE_ID", ChallengeID);
            contents.Add("CHALLENGE_NAME", ChallengeName);
            contents.Add("TOKEN", Token);

            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype classification=\"CREATE\" type=\"CREATE_REFERABLE_OBJECT\" logged=\"false\" />\n";
            foreach (string user in OtherUsers)
                xml += "<user id=\"" + user + "\" ip=\"" + Token + "\" role=\"originator\" />\n";
            xml += "<object id=\"0\" type=\"SHARE\">\n";
            xml += "<properties>\n";
            foreach (KeyValuePair<string, string> property in properties)
                xml += "<property name=\"" + property.Key + "\" value=\"" + property.Value + "\" />\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "<content>\n";
            xml += "<description><![CDATA[Create shared referable object]]></description>\n";
            xml += "<properties>\n";
            foreach (KeyValuePair<string, string> content in contents)
                xml += "<property name=\"" + content.Key + "\" value=\"" + content.Value + "\" />\n";
            xml += "</properties>\n";
            xml += "</content>\n";
            xml += "</action>\n";
        }
        else
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Clear();
            properties.Add("USERNAME", UserID);
            properties.Add("TOKEN", Token);
            properties.Add("GROUP_ID", GroupID);
            properties.Add("CHALLENGE_ID", ChallengeID);
            properties.Add("ELEMENT_TYPE", "MY_MICROWORLD");
            properties.Add("TOOL", TOOL);
            properties.Add("VIEW_URL", PIKI_BASE + "/piki_thumb.png");
            properties.Add("REFERENCE_URL", PIKI_ADDRESS + "?docId=" + DocId);
            properties.Add("TEXT", "");

            xml += "<action time=\"" + millisec.ToString() + "\">\n";
            xml += "<actiontype classification=\"USER_INTERACTION\" type=\"CREATE_ELEMENT\" succeed=\"UNKNOWN\" />\n";
            xml += "<user id=\"piki\" role=\"originator\" />\n";
            xml += "<user id=\"" + (IsTestServer ? "Lasad8080" : "Lasad8090") + "\" ip=\"1234567890123\" role=\"receiver\" />\n";
            xml += "<object id=\"0\" type=\"element\">\n";
            xml += "<properties>\n";
            foreach (KeyValuePair<string, string> property in properties)
                xml += "<property name=\"" + property.Key + "\" value=\"" + property.Value + "\" />\n";
            xml += "</properties>\n";
            xml += "</object>\n";
            xml += "</action>\n";
        }
        return xml;
    }

    private string BuildLandmark(PikiObject pObject, string landmarkType, string description)
    {
        Int64 millisec = DateTimeToMillisec(System.DateTime.UtcNow);
        string xml = "";

        xml += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n";
        xml += "<action time=\"" + millisec.ToString() + "\">\n";
        xml += "<actiontype type=\"LANDMARK\" classification=\"other\" succeed=\"true\" logged=\"true\" />\n";
        foreach (string user in OtherUsers)
            xml += "<user id=\"" + user + "\" ip=\"" + Token + "\" role=\"originator\" />\n";
        xml += "<object id=\"" + pObject.Id + "\" type=\"" + pObject.Type + "\">\n";
        xml += "<properties>\n";
        foreach (KeyValuePair<string, string> property in pObject.Properties)
            xml += "<property name=\"" + property.Key + "\" value=\"" + property.Value + "\" />\n";
        xml += "</properties>\n";
        xml += "</object>\n";
        xml += "<content>\n";
        xml += "<description><![CDATA[" + description + "]]></description>\n";
        xml += "<properties>\n";
        xml += "<property name=\"SENDING_TOOL\" value=\"" + TOOL + "\" />\n";
        xml += "<property name=\"GROUP_ID\" value=\"" + _groupID + "\" />\n";
        xml += "<property name=\"LANDMARK_TYPE\" value=\"" + landmarkType + "\" />\n";
        xml += "<property name=\"CHALLENGE_ID\" value=\"" + _challengeID + "\" />\n";
        xml += "<property name=\"CHALLENGE_NAME\" value=\"" + _challengeName + "\" />\n";
        xml += "<property name=\"TOKEN\" value=\"" + Token + "\" />\n";
        xml += "</properties>\n";
        xml += "</content>\n";
        xml += "</action>\n";

        return xml;
    }
    #endregion

    #region Public functions
    public void SendCannonShotMessage(PikiObject pObject)
    {
        string msg = BuildIndicator(pObject, "activity", UserID + " fires the cannon");
        Debug.Log(msg);
        LogMessage(ANALYSIS_CHANNEL, msg);
    }

    public void SendNodeUrlModification(bool autosave)
    {
        string msg = BuildNodeUrl(autosave);
        LogMessage(COMMAND_CHANNEL, msg);
    }

    public void SendReferableObject(string tool)
    {
        Debug.Log("*********************************** SendReferableObject to " + tool);
        string msg = "";
        if (tool.Equals("LASAD"))
            msg = BuildLasadRefObj();
        
        LogMessage(COMMAND_CHANNEL, msg);
    }

    public void SendTreasureHitLandmark(PikiObject pObject)
    {
        string msg = BuildLandmark(pObject, "activity", UserID + " hit a treasure placed by " + pObject.Properties["SCENE_CREATOR_GROUP"]);
        Debug.Log(msg);
        LogMessage(ANALYSIS_CHANNEL, msg);
    }

    public void SendAllTreasuresHitLandmark(PikiObject pObject)
    {
        string msg = BuildLandmark(pObject, "activity", UserID + " hit all treasures placed by " + pObject.Properties["SCENE_CREATOR_GROUP"]);
        Debug.Log(msg);
        LogMessage(ANALYSIS_CHANNEL, msg);
    }

    public void SendDenfenceObjectAddedIndicator(PikiObject pObject)
    {
        string msg = BuildIndicator(pObject, "activity", UserID + " added a new defensive object: " + pObject.Properties["OBJECT_TAG"]);
        Debug.Log(msg);
        LogMessage(ANALYSIS_CHANNEL, msg);
    }

    public void SendPublishSceneLandmark(PikiObject pObject)
    {
        string msg = BuildLandmark(pObject, "activity", UserID + " published a new scene named " + pObject.Properties["SCENE_NAME"]);
        Debug.Log(msg);
        LogMessage(ANALYSIS_CHANNEL, msg);
    }
    #endregion
}
