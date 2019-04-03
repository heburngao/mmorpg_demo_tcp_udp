using UnityEngine;
using System.Collections;
using ghbc.Net;
using ghbc;
using msg;
using System;
using System.Collections.Generic;

public class Netmanager
{
	public static NetConnection connect_TCP;
	public static NetConnection_UDP connect_UDP;
	public static NetworkConfig conf;
	//	public static NetworkConfig conf_UDP;
	//init
	public void init ()
	{
		// Use this for initialization
		NetConnection.MSG_haldlerPool = new System.Collections.Generic.Dictionary<ushort, Type> () {
			{ (ushort)NetCode.CreateSelf,typeof(Rsp_CreateSelf) },
			{ (ushort)NetCode.CreateOthers,typeof(Rsp_CreateOthers) },
			{ (ushort)NetCode.UDP_UpdateStatus,typeof(Rsp_UpdateStatus) },//UDP
			{ (ushort)NetCode.BeatHeart,typeof(Rsp_HeartBeating) },
			{ (ushort)NetCode.LeaveOffOthers,typeof(Rsp_LeaveOffOthers) },

			{ (ushort)NetCode.Move,typeof(Rsp_StateUpdateMove) },
			{ (ushort)NetCode.TabSwitch,typeof(Rsp_StatusUpdateTabSwitch) },
			{ (ushort)NetCode.Sprint,typeof(Rsp_StatusUpdateSprint) },
			{ (ushort)NetCode.Jump,typeof(Rsp_StatusUpdateJump) },
			{ (ushort)NetCode.ExitGame,typeof(Rsp_StatusUpdateExitGame) },

		};
		NetConnection_UDP.MSG_haldlerPool = NetConnection.MSG_haldlerPool;

		DebugTool.setDebug (true);
		DebugTool.LogYellow("直接跳过登陆环节,此项目暂未添加登陆");
		LoginInfo.LoginState = true;
		LoginInfo.loginInfo [0] = 1;
		LoginInfo.loginInfo [1] = 1;
		if (conf == null) {
			conf = NetworkConfig.GetInstance ();
			//		conf.ip = "192.168.78.226";
			conf.ip = "127.0.0.1";
			//		conf.ip = "172.20.6.11";
			//		conf.ip = "192.168.1.106";
			//		conf.ip = "172.30.58.7";
		}
		conf.port = 42020;
		conf.write_timeout = 2000;
		conf.receiv_buffer_size = 1024;
		conf.connect_timeout = 2000;
		//===============================
		//		if (conf_UDP == null) {
		//			conf_UDP = NetworkConfig.GetInstance ();
		//			//		conf.ip = "192.168.78.226";
		//			conf_UDP.ip = "127.0.0.1";
		//			//		conf.ip = "172.20.6.11";
		//			//		conf.ip = "192.168.1.106";
		//			//		conf.ip = "172.30.58.7";
		//		}
		//		conf_UDP.port = 2020;
		//		conf_UDP.write_timeout = 2000;
		//		conf_UDP.receiv_buffer_size = 1024;
		//		conf_UDP.connect_timeout = 2000;

		//Array.Copy(;
		DebugTool.LogRed ("init Net :" + conf.ip);

	}

	public static void Close ()
	{
		if (connect_TCP != null)
			connect_TCP.Close2 ();
		if (connect_UDP != null)
			connect_UDP.Close2 ();
	}
	//创建连接成功，立即创建角色，发送第一个心跳包
	public static void ToConnect ()
	{
		connect_TCP = new NetConnection (conf);
		connect_UDP = new NetConnection_UDP (conf);

		DebugTool.LogYellow ("connect to :" + conf.ip + ":" + conf.port);
		//NetworkConfig.GetInstance () = conf;

		sendCreateSelf (); //创角


		//		msg.PlayerInfo playerinfo = new msg.PlayerInfo ();
		//		playerinfo.FrameIndex = 0;
		//		playerinfo.Level = 1;
		//		playerinfo.Userid = "";
		//		playerinfo.Nickname = "";
		//		playerinfo.SpawnPos = new Vect2 (){ X = 0f, Y = 0f };
		//		sendHeartBeating (playerinfo); //用于tcp  有连接状态的心跳
		#region 第一次触发心跳包
		sendHeartBeating (); //心跳  xxx
		#endregion
	}

	

	//new player for me
	public static void sendCreateSelf ()
	{
		// ====== 走pb 发送 ====== or 直接发送 =====
#if PROTOBUFF
		
		
		Rqst_CreateSelf data = new Rqst_CreateSelf ();
		data.Account = ""+1000 + UnityEngine.Random.Range (1, 100);
		data.Password = "" +123456;
		DebugTool.LogYellow ("[TCP] sendCreateSelf 请求创建我方角色" + data.Account);
		connect_TCP.Send<Rqst_CreateSelf> ((short)NetCode.CreateSelf, data);
		GameProxy.mine = new PlayerData();
		GameProxy.mine.userid = data.Account;


#else
		DebugTool.LogError ("已经不许使用自写编码，请检察 UnityEngine里是否添加宏: PROTOBUFF");
		//===== 直接发送  自写的编码，无法对付可变长byte[]======
		SendBuffer.ResetIndex ();
		SendBuffer.writeUshort (10000);//cmd,  payload as below
		var r = UnityEngine.Random.Range (1, 100);
		SendBuffer.writeInt (9999 + r);//accountid
//		SendBuffer.writeString ("gaohebing" + UnityEngine.Random.Range (0, 100));//nickname
//		SendBuffer.writeInt (1);//level
		SendBuffer.writeInt (123);//password

		var arr = SendBuffer.GetBytes_HasData ();
		byte[] sendbuff = new byte[arr.Length];
		Array.Copy (arr, sendbuff, sendbuff.Length);

		connect_TCP.SendBytes (sendbuff);
#endif
	}


	public static void sendHeartBeating ()//msg.PlayerInfo playerinfo = )
	{
#if PROTOBUFF


		DebugTool.LogYellow("[TCP] sendHeartBeating");
		var data = new Rqst_HeartBeating ();
		data.Status = 0;
		//		data.Player = playerinfo;
		connect_TCP.Send<Rqst_HeartBeating> ((short)NetCode.BeatHeart, data);


		
#else
		SendBuffer.ResetIndex ();
		SendBuffer.writeUshort (110);//cmd,  payload as below
		SendBuffer.writeInt (1); // payload
		byte[] sendbuff = SendBuffer.GetBytes_HasData ();
		connect_TCP.SendBytes (sendbuff);
#endif
	}

	
	//remove player TODO

	#region #############  UDP  #############

	//update player
	public static void sendUpdateStatus_UDP (StatusInfo statusInfo)
	{
		Rqst_UpdateStatus data = new Rqst_UpdateStatus ();
		data.Info = statusInfo;//new PlayerInfo ();
		var isConnected = connect_TCP.IsConnected ();
		//		data.player.Add (playerinfo);///错误 protobuf已变更
		if (isConnected) {
			DebugTool.LogYellow(" UDP:: send updateStatus" );
			connect_UDP.Send<Rqst_UpdateStatus> ((short)NetCode.UDP_UpdateStatus, data);
		} else {
			DebugTool.LogError ("socket is closed, UDP send failed!");
		}
	}
	//把收到的udp 告知 服务器
	// public static void sendConfirm_UDP (long FrameId, string UserId)
	// {
	// 	if (connect.IsConnected ()) {
	// 		DebugTool.LogRed (" UDP:: send updateStatus 确认 FrameID: " + FrameId + " UserId:" + UserId);
	// 		Rqst_UpdateStatus_Confirm data = new Rqst_UpdateStatus_Confirm ();
	// 		data.FrameID = FrameId;
	// 		data.UserID = UserId;
	// 		connect_UDP.Send<Rqst_UpdateStatus_Confirm> ((short)NetCode.UDP_UpdateStatus_Confirm, data);
	// 	}
	// }

	#endregion #############  UDP  #############
}

public  enum NetCode
{
	BeatHeart = 110,
	CreateSelf = 10000,
	CreateOthers = 10001,
	UDP_UpdateStatus = 20002,
	UDP_UpdateStatus_Confirm = 20003,
	LeaveOffOthers = 10003,

	#region status

	Move = 11001,
	TabSwitch = 11002,
	Sprint = 11003,
	Jump = 11004,
	ExitGame = 11005,
	#endregion
}

public enum Event_Player
{
	New_player,
	New_Other_player,
	Update_Status_UDP,
	LeaveOffOthers,
	SyncPos_HeartBeat,
}