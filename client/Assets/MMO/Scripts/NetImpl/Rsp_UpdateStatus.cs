using System;
using msg;
using System.Collections.Generic;

namespace ghbc.Net
{
	public class Rsp_UpdateStatus:S2C_Handler
	{
		public override void execute (short cmd, short ErrCode, byte[] payloads)
		{
			base.execute (cmd, ErrCode, payloads);
			var data = getData<Rspn_UpdateStatus> ();
			if (data != null && data.Info != null) {
				List<StatusInfo> playerList = data.Info;
				DebugTool.LogOrange("收到UDP status pos ");
				facade.DispatchEvent (new CEvent (Event_Player.Update_Status_UDP.ToString (), new object[]{ playerList }));
				///告知服务器，client成功接收 暂去 2018.3.9
//                for (int i = 0; i < playerList.Count; i ++){
//                    long frameid = playerList[i].FrameIndex;
//                    DebugTool.LogOrange("逐条上报 frameid: " + frameid);
//                    Netmanager.sendConfirm_UDP(frameid,playerList[i].Userid);
//				}
			}
		}
	}
}

 