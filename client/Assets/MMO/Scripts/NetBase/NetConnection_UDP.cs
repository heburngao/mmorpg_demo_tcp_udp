using System;

namespace ghbc.Net
{
	public class NetConnection_UDP : UnitySocket_UDP
	{
		 
		//        private Action _onUpdate;TODO xx
		public NetConnection_UDP (NetworkConfig config)//, Action onConnectSuccess,Action update, Action onConnectFail)TODO xx
		{
			//            this._onUpdate = update;TODO xx
			Socket_Create_Connection (config.ip, 52020);//config.port);
//			Socket_Create_Connection ();//,onConnectSuccess,onConnectFail);

		}
		//		public static void SetCallBack(Action callback){
		//			_onConnectionSuccess = callback;
		//		}
		//		public void QuestKey_for_Reconnect2(uint id, uint skey){
		//			QuestKey_for_Reconnect(id,skey);
		//		}
		public void Close2 ()
		{
			if (m_Socket != null) {// && m_Socket.Connected) {
				Close (m_Socket);
			}
		}
		//        protected override void Updated()  TODO xx
		//        {
		//            if (IsConnect())
		//            {
		//                if (this._onUpdate != null)
		//                {
		//                    this._onUpdate();
		//                }
		//            }
		//		base.Updated ();
		//        }
	}
}


