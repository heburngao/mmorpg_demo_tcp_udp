using System;
using System.Collections.Generic;
using msg;
using UnityEngine;

namespace ghbc.Net
{
	public class Rsp_CreateOthers:S2C_Handler
	{
		 
		public override void execute (short cmd, short ErrCode, byte[] payloads)
		{
			base.execute (cmd, ErrCode, payloads);
//			try {
			#if PROTOBUFF
			var list = new List<PlayerData> ();
			var data = getData<Rspn_CreateOthers> ();
			if (data == null) {
				DebugTool.LogError (" get data error create Others ");
				return;
			}
			foreach (var item in data.Player) {
				var playerdata = new PlayerData ();
				playerdata.userid = item.Userid;
				playerdata.nickname = item.Nickname;
				playerdata.level = item.Level;
//xxx					force = new Vector3 (item.Force.X, item.Force.Y, item.Force.Z),
//xxx					rotate = new Vector3 (item.Rotate.X, item.Rotate.Y, item.Rotate.Z),
				playerdata.status = item.Status;
				// playerdata.frameIndex = item.FrameIndex;
				playerdata.Pos = item.Pos;
//xxx					speed = item.Speed,
//xxx					inputForce = new Vector2 (item.InputForce.X, item.InputForce.Y),


				DebugTool.LogYellow (string.Format ("收到服务器返回 其他玩家:<<<<<<<<<  userID :{0}, NickName :{1} , Level :{2}  , Status : {3} ", item.Userid, item.Nickname, item.Level, item.Status));
				list.Add (playerdata);
			}
			//			data.Level;
			//			data.SessionID;
			#else
//			} catch (Exception ex) {
//				DebugTool.LogError (ex.Message);
//			}
//
			//========直接处理 as below ==========
			foreach (var item in payloads) {
				DebugTool.LogYellow (item);
			}
			var list = new List<PlayerData> ();
			getData ();
			var len = ReadInt ();
			for (int i = 0; i < len; i++) {
				ResetReadIndex ();
				var userid = ReadInt ();
				var nickname = ReadString ();
				var level = ReadInt ();
				 
				DebugTool.LogYellow (string.Format ("收到服务器返回:<<<<<<<<< 其他玩家  userID :{0}, NickName :{1} , Level :{2}    ", userid, nickname, level));
				var data = new PlayerData {
					nickname = nickname,
					userid = userid,
					level = level,
					 
				};
				list.Add (data);
			}
			#endif
			facade.DispatchEvent (new CEvent (Event_Player.New_Other_player.ToString (), new object[]{ list }));
		}
	}
}

