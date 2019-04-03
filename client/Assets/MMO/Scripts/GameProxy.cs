using UnityEngine;
using System.Collections;
using ghbc;
using System.Collections.Generic;
using msg;

public class GameProxy : MonoBehaviour
{
	[SerializeField]
	private GameObject characterPrefab;

	/// <summary>
	/// 客户端玩家列表
	/// </summary>
	public static Dictionary<string ,PlayerData> ClientAllPlayerDic = new Dictionary<string, PlayerData> ();
	 
	//	private List<PlayerData> allPlayers = new List<PlayerData> ();
	// Use this for initialization
	void Start ()
	{
		Facade_Base.instance.AddEvent (Event_Player.New_player.ToString (), newPlayerHandler);
		Facade_Base.instance.AddEvent (Event_Player.New_Other_player.ToString (), newOtherPlayerHandler);
		Facade_Base.instance.AddEvent (Event_Player.Update_Status_UDP.ToString (), updateStatusHandler_UDP);
		Facade_Base.instance.AddEvent (Event_Player.LeaveOffOthers.ToString (), leaveOtherPlayerHandler);
		// Facade_Base.instance.AddEvent (Event_Player.SyncPos_HeartBeat.ToString (), syncPlayerPosHandler);
	}

	void OnDispose ()
	{
		Facade_Base.instance.RemoveEvent (Event_Player.New_player.ToString (), newPlayerHandler);
		Facade_Base.instance.RemoveEvent (Event_Player.New_Other_player.ToString (), newOtherPlayerHandler);
		Facade_Base.instance.RemoveEvent (Event_Player.Update_Status_UDP.ToString (), updateStatusHandler_UDP);
		Facade_Base.instance.RemoveEvent (Event_Player.LeaveOffOthers.ToString (), leaveOtherPlayerHandler);
		// Facade_Base.instance.RemoveEvent (Event_Player.SyncPos_HeartBeat.ToString (), syncPlayerPosHandler);
	}

	//===================================
// 	bool syncPlayerPosHandler (object[] args)
// 	{
		
// 		List<msg.PlayerInfo> playerlist = (List<msg.PlayerInfo>)args [0];
// 		if (playerlist == null) {
// 			Debug.LogError ("不拉回");
// 			return true;
// 		}
// 		foreach (msg.PlayerInfo syncPosItem in playerlist) {
// 			PlayerData item = null;
// 			if (ClientAllPlayerDic.TryGetValue (syncPosItem.Userid, out item)) {
// //				if (item.isMine == false && item.userid == syncPosItem.Userid) {//只处理别人的xxxxx
// //					ClientAllPlayerDic.Remove (item.userid);
// 				// item.Pos = syncPosItem.Pos;

// 				// if (item.comp.callbackSyncPos != null) {
// 				// 	item.comp.callbackSyncPos (item);
// 				// }
// //				}xxxxx
// 			}
// 		}
// 		return true;
// 	}

	bool leaveOtherPlayerHandler (object[] args)
	{
		var player = (List<PlayerInfo>)args [0];
//		for (int i = 0; i < allPlayers.Count; i++) {
//		PlayerData item = allPlayers [i];
		foreach (var leaveOff in player) {
			PlayerData item = null;
			if (ClientAllPlayerDic.TryGetValue (leaveOff.Userid, out item)) {
				if (item.isMine == false && item.userid == leaveOff.Userid) {
					Destroy (item.go);
//					allPlayers.RemoveAt (i);
					ClientAllPlayerDic.Remove (item.userid);
//					i--;
				}
			}
		}
		return true;
	}
	//#############################################################
	//update status
	Queue<msg.StatusInfo> queueInfo = new Queue<StatusInfo> ();

	bool updateStatusHandler_UDP (object[] args)
	{
		/*
		var updateList = (List<StatusInfo>)args [0]; 
		for (int j = 0; j < updateList.Count; j++) {
			StatusInfo updateInfo = updateList [j];
//			for (int i = 0; i < allPlayers.Count; i++) {
//				PlayerData item = allPlayers [i];
			PlayerData item = null;
			if (ClientAllPlayerDic.TryGetValue (updateInfo.Userid, out item)) {
			
				//如果不是玩家自己，且是对应的其他玩家列表中
//			if (item.isMine == false && item.userid == player.Userid) { TODO
//				if (mine.userid == item.userid) {
//					DebugTool.LogError ("同步到我自己的信息" + mine.userid + " inputForce: " + item.inputForce);
//				}  
				if (item.userid == updateInfo.Userid) {
//xxx					DebugTool.LogWarning ("同步所有玩家坐标信息 userid: " + updateInfo.Userid + " inputForce: " + item.inputForce + " 是我: " + item.isMine);
					item.userid = updateInfo.Userid;
//					item.nickname = updateInfo.Nickname;
//					item.level = updateInfo.Level;
//xxx					item.force = new Vector3 (updateInfo.Force.X, updateInfo.Force.Y, updateInfo.Force.Z);
//xxx					item.rotate = new Vector3 (updateInfo.Rotate.X, updateInfo.Rotate.Y, updateInfo.Rotate.Z);

					item.status = updateInfo.Status;
//					item.speed = updateInfo.Speed;
//xxx					item.inputForce = new Vector2 (updateInfo.InputForce.X, updateInfo.InputForce.Y);
//					item.Force = new msg.Vect3 ();
					if (item.targetPos == null) {
						DebugTool.LogError (" update status :: force is null");
					}
					item.targetPos = updateInfo.TargetPos;
					item.MoveSpeed = updateInfo.MoveSpeed;
//					var force = item.force.X + item.force.Z;
//					if (force == 0) {
//						item.status = msg.State.IDLE;
//					}
					DebugTool.LogYellow (" status :: " + item.status);
//					item.frameIndex = updateInfo.FrameIndex;
//					DebugTool.LogWhite ("<<<<<<<<<<<<<<< frameIndex: " + item.frameIndex);
//				item.go.transform.position = item.pos;
//				Facade_Base.instance.DispatchEvent (new CEvent ("status", new object[]{ item }));
					if (item.comp.callbackStatus != null) {
						item.comp.callbackStatus (item);
					}
				}
			}
		}*/


		var updateList2 = (List<StatusInfo>)args [0]; 
        // updateList2.Sort((x,y)=> x.FrameIndex .CompareTo(y.FrameIndex));//升序
                                                                        //updateList2.Sort((x, y) => -x.FrameIndex.CompareTo(y.FrameIndex));//降序
        for (int i = 0; i < updateList2.Count; i++)
        {
            // DebugTool.LogOrange(i+ " - "+ updateList2[i].FramSeIndex);
			queueInfo.Enqueue (updateList2 [i]);
		}
		
        //while (0 < queueInfo.Count)
        //{
        //    var updateList = queueInfo.Dequeue();
        //    //          for (int j = 0; j < updateList.Count; j++) {
        //    StatusInfo updateInfo = updateList;// [j];
        //    PlayerData item = null;
        //    if (ClientAllPlayerDic.TryGetValue(updateInfo.Userid, out item))
        //    {

        //        if (item.userid == updateInfo.Userid)
        //        {
        //            item.userid = updateInfo.Userid;
        //            item.status = updateInfo.Status;
        //            if (item.targetPos == null)
        //            {
        //                DebugTool.LogError(" update status :: force is null");
        //            }
        //            item.targetPos = updateInfo.TargetPos;
        //            item.MoveSpeed = updateInfo.MoveSpeed;
        //            DebugTool.LogYellow(" status :: " + item.status);
        //            if (item.comp.callbackStatus != null)
        //            {
        //                item.comp.callbackStatus(item);
        //            }
        //        }
        //    }
        //    //          }
        //}
		return true;
	}

	// float timer = .066f;
	//每隔66毫秒解读一次？
    
	// void FixedUpdate ()
	void Update ()
	{

		// timer -= Time.deltaTime;  
        // while (0f < timer) {
		// 	return;
		// }
		// timer = .066f;
		#region
		//#####
        while (0 < queueInfo.Count)
        {
            var updateList = queueInfo.Dequeue();
            //          for (int j = 0; j < updateList.Count; j++) {
            StatusInfo updateInfo = updateList;// [j];
            PlayerData item = null;
            if (ClientAllPlayerDic.TryGetValue(updateInfo.Userid, out item))
            {

                if (item.userid == updateInfo.Userid)
                {
                    item.userid = updateInfo.Userid;
                    item.status = updateInfo.Status;
                    // if (item.targetPos == null)
                    // {
                    //     DebugTool.LogError(" update status :: force is null");
                    // }
                    item.Pos = updateInfo.Pos;
                    item.Dir = updateInfo.Dir;
                    item.MoveSpeed = updateInfo.MoveSpeed;
                    DebugTool.LogOrange(" 收到 status 更新 :: " + item.status);
                    if (item.comp.callbackStatus != null)
                    {
                        item.comp.callbackStatus(item);
                    }
                }
            }
            //          }
        }
		//#####
		#endregion
		
	}
	//#############################################################
	//创建同屏角色
	bool newOtherPlayerHandler (object[] args)
	{
		var list = (List<PlayerData>)args [0];
		foreach (PlayerData newplayerData in list) {
			if (ClientAllPlayerDic.ContainsKey (newplayerData.userid) == false) {
				if (mine == null || newplayerData.userid == mine.userid) {
					DebugTool.LogError ("排除自己");
					continue;//排除自己
				}
			
				newplayerData.go = Instantiate (characterPrefab, this.transform);
				newplayerData.go.tag = "Untagged";
				newplayerData.go.transform.position = new Vector3 (newplayerData.Pos.X * .0001f, 2f, newplayerData.Pos.Y * .0001f);//+= Vector3.left * Random.Range (0f, 1f);
				newplayerData.go.name = newplayerData.userid.ToString ();//"Player_" + newplayerData.nickname;
				newplayerData.isMine = false;
				newplayerData.comp = newplayerData.go.AddComponent<Player> ();
				newplayerData.comp.data = newplayerData;

//			newplayerData.comp.data.isMine = false;
				newplayerData.go.SetActive (true);
//			if (allPlayers.ContainsKey (newplayerData.userid)) {
//				DebugTool.LogError ("重复的其他玩家数据的添加,userid:" + newplayerData.userid);
//			}
				ClientAllPlayerDic.Add (newplayerData.userid, newplayerData);
//			allPlayers.Add (newplayerData);
			}
		}
		return true;
	}

	private static PlayerData _mine;
	public static Player myplayer;
	//============
	/// <summary>
	/// 玩家自己的数据
	/// </summary>
	public static PlayerData mine {
		get {
			if (myplayer != null ){
				
				_mine.Pos = new Vect3 () {
					X = (int)(myplayer.transform.position.x * 10000),
					Y = (int)(myplayer.transform.position.y * 10000),
					Z = (int)(myplayer.transform.position.z * 10000),

				};
				print ("获取我的local坐标: " + myplayer.transform.position);

			}
			return _mine;
		}
		set {
			_mine = value;
		}
	}
	//创建玩家
	bool newPlayerHandler (object[] args)
	{

		PlayerData newplayerData = (PlayerData)args [0];
		if (ClientAllPlayerDic.ContainsKey(newplayerData.userid)) {
				DebugTool.LogError ("阻止重复的创角 uid: " + newplayerData.userid);
				return true;
		}

		DebugTool.LogYellow ("\r\n ==========  new player born  =========== ");
		newplayerData.go = Instantiate (characterPrefab, this.transform);
		 
		
		newplayerData.go.transform.position = new Vector3 (newplayerData.Pos.X * .0001f, newplayerData.Pos.Y * .0001f, newplayerData.Pos.Z * .0001f);//服务器给出 y:2f 空中生成掉下
//		newplayerData.go.transform.position += Vector3.left * Random.Range (0f, 1f);
		
		newplayerData.comp = newplayerData.go.AddComponent<Player> ();
		newplayerData.comp.data = newplayerData;
//		newplayerData.comp.data.isMine = true;
		if(GameProxy.mine != null && newplayerData.userid == GameProxy.mine.userid){
			newplayerData.go.name = "me" + newplayerData.userid.ToString ();//"Player_" + newplayerData.nickname;
			newplayerData.isMine = true;
			newplayerData.go.tag = "Me";
			mine = newplayerData;
		}else{
			newplayerData.go.name = "other_" + newplayerData.userid.ToString ();//"Player_" + newplayerData.nickname;
			newplayerData.isMine = false;
			newplayerData.go.tag = "Player";
		}
		newplayerData.go.SetActive (true);
		myplayer = newplayerData.comp;
		ClientAllPlayerDic.Add (newplayerData.userid, newplayerData);
//		allPlayers.Add (newplayerData);
		return true;
	}
	// Update is called once per frame
	 
}
