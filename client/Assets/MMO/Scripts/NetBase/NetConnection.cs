using System;

namespace ghbc.Net
{
	public class NetConnection : UnitySocket
	{
		 
		public NetConnection (NetworkConfig config)//, Action onConnectSuccess,Action update, Action onConnectFail)TODO xx
		{
			Socket_Create_Connection (config.ip, config.port);

		}
		public void Close2 ()
		{
			if (m_Socket != null && m_Socket.Connected) {
				Close (m_Socket);
			}
		}
		 
	}
}


