<?
  $userId = (isset($_REQUEST["user"]))?$_REQUEST["user"]:"none";
  $groupId = (isset($_REQUEST["groupId"]))?$_REQUEST["groupId"]:"none";
  $challengeId = (isset($_REQUEST["challengeId"]))?$_REQUEST["challengeId"]:"none";
  $challengeName = (isset($_REQUEST["challengeName"]))?$_REQUEST["challengeName"]:"none";
  $role = (isset($_REQUEST["role"]))?$_REQUEST["role"]:"none";
  $token = (isset($_REQUEST["token"]))?$_REQUEST["token"]:"none";
  $testServer = (isset($_REQUEST["testServer"]))?$_REQUEST["testServer"]:"none";
  $ptNodeId = (isset($_REQUEST["ptNodeId"]))?$_REQUEST["ptNodeId"]:"none";
  $ptMap = (isset($_REQUEST["ptMap"]))?$_REQUEST["ptMap"]:"none";
  $docId = (isset($_REQUEST["docId"]))?$_REQUEST["docId"]:"none";


  for ($i = 1; $i < 20; $i++)
  {
    if(isset($_REQUEST["otherUser".$i]))
    {
      $userId .= ";".$_REQUEST["otherUser".$i];
    }
  }
?>
<!DOCTYPE html>
<html>
 <head>
  <title>Pirates of the Kinematic Island</title>

    <script type="text/javascript" src="http://webplayer.unity3d.com/download_webplayer-3.x/3.0/uo/UnityObject.js"></script>
    <script type="text/javascript">
    <!--
    function GetUnity() {
      if (typeof unityObject != "undefined") {
        return unityObject.getObjectById("unityPlayer");
      }
      return null;
    }

    var params = {
      disableContextMenu: true
    };

    if (typeof unityObject != "undefined") {
      unityObject.embedUnity("unityPlayer", "WebPlayer.unity3d?userId=<?=$userId?>&groupId=<?=$groupId?>&challengeId=<?=$challengeId?>&challengeName=<?=$challengeName?>&role=<?=$role?>&token=<?=$token?>&testServer=<?=$testServer?>&ptNodeId=<?=$ptNodeId?>&ptMap=<?=$ptMap?>&docId=<?=$docId?>", 800, 600, params);

    }
    -->
    </script>
    <style type="text/css">
    <!--
    div#unityPlayer {
      cursor: default;
      height: 600px;
      width: 800px;
    }
    -->
    </style>


   </head>
  <body style="margin-left: 0px; margin-top: 0px;">

    <div id="unityPlayer">
      <div class="missing">
        <a href="http://unity3d.com/webplayer/" title="Unity Web Player. Install now!">
          <img alt="Unity Web Player. Install now!" src="http://webplayer.unity3d.com/installation/getunity.png" width="193" height="63" />
        </a>
      </div>
    </div>

   </body>
</html>
