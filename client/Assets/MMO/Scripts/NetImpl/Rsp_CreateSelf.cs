using UnityEngine;
using System.Collections;
using ghbc.Net;
using msg;
using System;
using ghbc;

public class Rsp_CreateSelf : S2C_Handler
{

	public override void execute (short cmd, short ErrCode, byte[] payloads)
	{

		base.execute (cmd, ErrCode, payloads);



		#if PROTOBUFF
		PlayerData data = null;
		try {
			var receivedata = getData<Rspn_CreateSelf> ();
			data = new PlayerData ();
			
			data.userid = receivedata.Player.Userid;
			data.nickname = receivedata.Player.Nickname;
			data.level = receivedata.Player.Level;
//xxx			force = new Vector3 (receivedata.player.Force.X, receivedata.player.Force.Y, receivedata.player.Force.Z),
//xxx			rotate = new Vector3 (receivedata.player.Rotate.X, receivedata.player.Rotate.Y, receivedata.player.Rotate.Z),
			data.status = receivedata.Player.Status;
			// data.frameIndex = receivedata.Player.FrameIndex;
			data.Pos = receivedata.Player.Pos;
            data.Dir = receivedata.Player.Dir;
//xxx			speed = receivedata.player.Speed,
//xxx			inputForce = new Vector2 (receivedata.player.InputForce.X, receivedata.player.InputForce.Y),

			DebugTool.LogYellow (string.Format ("收到服务器返回[ newplayer ] :<<<<<<<<<  userID :{0}, NickName :{1}  ", data.userid, data.nickname));
		} catch (Exception ex) {
			DebugTool.LogError ("创建回调对角出错" + ex.Message);
		}





		#else
		//========直接处理 as below ==========
		foreach (var item in payloads) {
			DebugTool.LogYellow (item);
		}
		getData ();

		var nickname = ReadString ();
		var userid = ReadInt ();
		var level = ReadInt ();
//		var seesionid = ReadString ();
		Debug.Log (string.Format ("收到服务器返回:<<<<<<<<<创角  userID :{0}, NickName :{1} , Level :{2}    ", userid, nickname, level));
		var data = new PlayerData {
			nickname = nickname,
			userid = userid,
			level = level,
//			sessionid = seesionid
		};
		#endif





		facade.DispatchEvent (new CEvent (Event_Player.New_player.ToString (), new object[]{ data }));
	}
}

public class PlayerData
{
	public string nickname;
	public string userid;
	public int level;
	//	public float X;
	//	public float Y;
	public int MoveSpeed;
	public msg.Vect3 Pos;
    public msg.Vect3 Dir;
	public msg.State status;
	// public int frameIndex;
	// public msg.Vect2 spawnPos;

	//xxx	public Vector3 force;
	//xxx	public Vector3 rotate;
	//xxx	public float speed;
	//xxx	public Vector2 inputForce;
	//	public string sessionid;
	public GameObject go;
	public Player comp;
	public bool isMine = false;

}

