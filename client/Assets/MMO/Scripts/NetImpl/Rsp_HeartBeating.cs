using System;
using System.Collections.Generic;

namespace ghbc.Net
{
	public class Rsp_HeartBeating:S2C_Handler
	{
		public override void execute (short cmd, short ErrCode, byte[] payloads)
		{
			base.execute (cmd, ErrCode, payloads);
			#if PROTOBUFF

            #region ######
            TimerMgr.Register("delayHeartBeat", 15f, () => {
                DebugTool.LogRed("发送心跳包");
                //              var mydata = GameProxy.mine;
                //              msg.PlayerInfo playerinfo = new msg.PlayerInfo ();
                //              playerinfo.FrameIndex = 0;
                //              playerinfo.Level = mydata.level;
                //              playerinfo.Userid = mydata.userid;
                //              playerinfo.Nickname = mydata.nickname;
                //              playerinfo.SpawnPos = mydata.spawnPos;//TODO 处理拉回
                Netmanager.sendHeartBeating();
            });
            #endregion


			try {
				var data = getData<msg.Rspn_HeartBeating> ();
				var statusCode = data.Status;
				// List<msg.PlayerInfo> playerList = data.PlayerList;
				// 暂不使用心跳把拉回坐标 2019.3.26 facade.DispatchEvent (new CEvent (Event_Player.SyncPos_HeartBeat.ToString (), new object[]{ playerList }));
				 
				DebugTool.LogRed (string.Format ("收到服务器返回 心跳包:<<<<<<<<<  statusCode :{0}", statusCode));
			} catch (Exception ex) {
				DebugTool.LogError (ex.Message);
			} 
//			TimerMgr.Register ("delayHeartBeat", 15f, () => {
//				DebugTool.LogRed ("发送心跳包");
//				msg.PlayerInfo playerinfo = new msg.PlayerInfo();
//				playerinfo.FrameIndex = 0 ;
//				playerinfo.Level = 
//				Netmanager.sendHeartBeating (playerinfo);
//			});




			




			#else
			foreach (var item in payloads) {
				DebugTool.LogYellow (item);
			}
			getData ();

			var statusCode = ReadInt ();
			 
			DebugTool.LogRed (string.Format ("收到服务器返回 心跳包:<<<<<<<<<  statusCode :{0}", statusCode));
			TimerMgr.Register ("delayHeartBeat", 15f, () => {
			DebugTool.LogRed ("发送心跳包");
			Netmanager.sendHeartBeating ();
			}); 
			#endif

//			facade.DispatchEvent (new CEvent (Event_Player.New_player.ToString (), new object[]{ data }));
		}
	}
}

