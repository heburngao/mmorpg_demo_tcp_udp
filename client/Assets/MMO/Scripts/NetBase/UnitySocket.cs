// 描   述：封装c# socket数据传输协议
using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using ghbc.Net.Interface;
using System.Threading;
using ghbc.Enums;
using NetComp.Net.Event;

namespace ghbc.Net
{
	/// <summary>
	/// 本scoket　采用大端,即:大端是高端数据存在低端地址。
	/// 
	//大端法，即l的高位数据存放在byte[] 的低位地址，因为地址是
	//从低向高发展的
	//protobuf rules:
	//connect send:
	//byte[2]("DB")+ int(length) + uint(id) + uint(skey)
	//read:
	//byte[2]("DB")+ int(length) + uint(id) + uint(skey)

	//=============================================================================
	//normal send:
	//byte[2]("DB") + int(length) + uShort(cmd)+byte[n](payload)
	//read:
	//byte[2]("DB")+ int(length) + uShort(cmd) + uShort(ret) + byte[n](payload)


	/// </summary>
	public abstract class UnitySocket
	{


		private delegate void Delegate_SocketReconnect ();

		private Delegate_SocketReconnect reconnectFnc;

		// private delegate void Delegate_SocketDataArrival (byte[] data);

		// Delegate_SocketDataArrival OnSocketDataArrival = null;

		private delegate byte[] Delegate_SocketDataArrival (byte[] data);

		Delegate_SocketDataArrival OnSocketDataArrival = null;
		private delegate void Delegate_SocketDisconnected ();

		Delegate_SocketDisconnected OnSocketDisconnected = null;
		private const short HeaderLen = 2;
		private const int BufferLen = 4;
		private const short CMDLen = 2;
		private const short RETLen = 2;
		private const string HeadWords = "DB";
		private byte[] UnPacket (byte[] buffer){

			// DB + LEN + CMD + RET + PAYLOAD  
			// short + int + short + short + byte[]

			// func Unpack(buffer []byte, readerChannel chan []byte) []byte {
			var len_rcv = buffer.Length;
			int i ;
			for (i = 0; i < len_rcv; i = i + 1) {
				if (len_rcv < i+HeaderLen+BufferLen) {
					// PR("[TCP] break len_rcv: " , len_rcv)
					queue_LogError.Enqueue ("len_rcv < i+HeaderLen+BufferLen break");
					break;
				}
				byte[] buf_header_words = CopyToBigEndianBytes(buffer,i,HeaderLen,0,false);//DB无需转为BigEndian
				 
				 queue_LogError.Enqueue ("=====解包=======");
				if(System.Text.Encoding.UTF8.GetString(buf_header_words) == HeadWords){ //无需转为BigEndian
					//messagelength : BufferLen 长度对应的内容表达长度
					//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
					queue_LogError.Enqueue ("=====" + HeadWords + "=======");
					byte[] buf_totoal_len = CopyToBigEndianBytes(buffer ,i+HeaderLen, BufferLen);
				 

					int DB_ttsize_cmd_ret_payload = BitConverter.ToInt32(buf_totoal_len,0);//IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf_msg_len,0));//BytesToInt(buffer[i+HeaderLen : i+HeaderLen+BufferLen])
					queue_LogError.Enqueue ("=====buffer总长: " + DB_ttsize_cmd_ret_payload + " =======");
					//if len_rcv < i+HeaderLen+BufferLen+messageLength {
					if (len_rcv < i+ DB_ttsize_cmd_ret_payload) {
						// PR("[TCP] break b " , len_rcv, totoal_msg_len , i, HeaderLen , BufferLen)
						break;
					}
					byte[] buf_cmd_ret_payload = CopyToBigEndianBytes(buffer,i+HeaderLen+BufferLen ,DB_ttsize_cmd_ret_payload - HeaderLen - BufferLen , 0 , false);
			 
					 
					//================================
							if (LoginInfo.LoginState) {//登陆后的处理 //  CMD + RET + PAYLOAD  
																	//  short + short + byte[]
														var cmd_byte = CopyToBigEndianBytes(buf_cmd_ret_payload,0,CMDLen);
								short cmd = BitConverter.ToInt16(cmd_byte,0);//ReadBuffer.readUshort ();//buffer.readUshort(); //控制码
														var ret_byte = CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen,RETLen);
								short ret = BitConverter.ToInt16(ret_byte,0);//ReadBuffer.readUshort ();
								//payloads由protobuffer编码而成，所以不需要转为BigEndian
														byte[] payloads = CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen + RETLen,buf_cmd_ret_payload.Length - CMDLen - RETLen, 0 ,false);
								 
								queue_LogError.Enqueue("<color=yellow>登陆后的处理</color>");

								queue_payloads.Enqueue (payloads);
								queue_ret.Enqueue (ret);
								queue_cmd.Enqueue (cmd);
								queue_LogError.Enqueue ("<<< 普通的处理 ret: " + ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉

							}

							if (!LoginInfo.LoginState) {//特别处理 登陆
								queue_LogError.Enqueue("<color=yellow>未登陆后的处理</color>");
								int ID = BitConverter.ToInt32(buf_cmd_ret_payload,0);//ReadBuffer.readUint ();//bf.readUint();
								int SKEY = BitConverter.ToInt16(buf_cmd_ret_payload,4);//ReadBuffer.readUint ();//bf.readUint();

								LoginInfo.LoginState = true;
								LoginInfo.loginInfo [0] = ID;
								LoginInfo.loginInfo [1] = SKEY;
								//											DebugTool.Log (LoginInfo.loginInfo [0] + " |login| " + LoginInfo.loginInfo [1]);
								queue_LogError.Enqueue ("登陆 id: " + LoginInfo.loginInfo [0] + " skey: " + LoginInfo.loginInfo [1]);
								// 登陆成功 发送一般性 请求  as below::
								//ConnetionTest.doRequest ();
								if (isInsteadOf == false) {//非被顶替性　连接 登陆
									Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.Get_SKey_Success_Back.ToString (), null));
								} else {
									isInsteadOf = false;
								}

							}
							//=============================



					//i =  i + HeaderLen + BufferLen + messageLength - 1
					i = i + DB_ttsize_cmd_ret_payload - 1;
					//PR("[TCP] cc " , data , " index: " , i)
				}
			}

			if (i == len_rcv) {
				// PR("[TCP] return over " , i)
				return new byte[]{};//make([]byte, 0)
			}
			if (reconnect_Lock) {
				reconnect_Lock = false;
			}
			// PR("[TCP] xxxxxx return buffer[i:] : " , buffer[i:])

			byte[] buf_next = new byte[buffer.Length - i];
			Array.Copy(buffer ,i,buf_next,0, buffer.Length - i); 
			return buf_next;

		}
		private byte[] Packet(short cmd, byte[] pb_stream){
			byte[] buffer_snd = new byte[HeaderLen + BufferLen + CMDLen + pb_stream.Length];
					//==========
					byte[] opcode_bytes = System.Text.Encoding.UTF8.GetBytes ("DB");//无需转为BigEndian
					 
					byte[] totoal_len_bytes = BitConverter.GetBytes(buffer_snd.Length);
					if(BitConverter.IsLittleEndian){
						Array.Reverse(totoal_len_bytes);
					}

					byte[] cmd_bytes = BitConverter.GetBytes(cmd);
					if(BitConverter.IsLittleEndian){
						Array.Reverse(cmd_bytes);
					}

					

					Array.Copy(opcode_bytes,0,buffer_snd,0,HeaderLen);
					Array.Copy(totoal_len_bytes,0,buffer_snd,HeaderLen,BufferLen);
					Array.Copy(cmd_bytes,0,buffer_snd,HeaderLen+BufferLen,CMDLen);
					Array.Copy(pb_stream,0,buffer_snd,HeaderLen+BufferLen+CMDLen,pb_stream.Length);
					return buffer_snd;
		}
		// / <summary>
		// / 异步收到消息处理器
		// / </summary>
		// / <param name="data"></param>
		// private   void OnSocketDataArrivalHandler2 (byte[] CombinedData)
		// {
		// 	try {
		// 		queue_LogError.Enqueue ("解包DataArrival --------------- socketDataArrivalHandler: " + CombinedData.Length + " isLogin: " + LoginInfo.LoginState);
		// 		queue_LogError.Enqueue ("解包DataArrival ---------------  isInsteadOf:: " + isInsteadOf + " isLogin:: " + LoginInfo.LoginState);
		// 		var combinedLen = CombinedData.Length;
		// 		var index_getCombineSize = 0;
		// 		while (combinedLen > 0) {

		// 			if (combinedLen < PAKH_EADER) {// 接收小于定义的收，继续接收
		// 				// CombineBuffer.TooShortReceive (combinedLen);
		// 				//DebugTool.LogCyan("接收不完整 ,则缓存之");
		// 				queue_LogError.Enqueue ("接收头部不完整 ,则缓存之");
		// 				break;
		// 			} else {
		// 				// modify by ghb 20160720
		// 				// int msgSettingSize = CombineBuffer.GetMsgSize (index_getCombineSize);//GetMsgSize(); 额定包长
		// 				//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} || 额定包长:{1}", bytesReadLen, msgSize));
		// 				queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} || 额定包长:{1}", combinedLen, msgSettingSize));
		// 				if (combinedLen < msgSettingSize) { // 实收包长 < 第一份额定包长定义
		// 					//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} < 额定包长:{1}", bytesReadLen, msgSize));// TODO 注释掉
		// 					queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} < 额定包长:{1}", combinedLen, msgSettingSize));
		// 					// CombineBuffer.TooShortReceive (combinedLen);//粘包
		// 					break;
		// 				} else {

		// 					//if (bytesReadLen > msgSettingSize)
		// 					//{//超长粘包处理

		// 					//}
		// 					//else
		// 					//{//实收包长 == 额定包长定义
		// 					//  TODO ....
		// 					////最终合并
		// 					// byte[] realReadBufItem = CombineBuffer.GeUsefulBuff (msgSettingSize);//实际收到的 总包片段
		// 					//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} >= 额定包长:{1}", bytesReadLen, msgSize));
		// 					queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} >= 额定包长:{1}", combinedLen, msgSettingSize));
		// 					//DebugTool.LogCyan("混合总包长度 :" + realReadBufItem.Length);
		// 					queue_LogError.Enqueue (string.Format ("混合总包长度 :" + realReadBufItem.Length));
		// 					//================ 解读 =================

		// 					ReadBuffer.StartReadBuffer (realReadBufItem);//设置到  读取 缓存带中  正式解包
		// 					short msgOpcode = ReadBuffer.GetOpcode ();
		// 					int msgsize = ReadBuffer.GetMsgSize ();
		// 					ReadBuffer.printBytes (combinedLen);
		// 					//DebugTool.LogCyan(">>>>>>>>>>>>>>>> 解码::  opcode : " + msgOpcode + " | msgsize : " + msgsize); //TODO 注释掉
		// 					queue_LogError.Enqueue (">>>>>>>>>>>>>>>> 解码::  opcode : " + msgOpcode + " | msgsize : " + msgsize);

		// 					//  isLogin 与 !isLogin 的顺序不能换 
		// 					if (LoginInfo.LoginState) {//登陆后的处理
		// 						//DebugTool.LogBlue("<<< 普通的处理"); //TODO 注释掉
		// 						//CreateMessage(bf);
		// 						//											(short)Opcode+(int)size + (ushort)cmd + (ushort)ret + (byte[]) payloads

		// 						ushort cmd = ReadBuffer.readUshort ();//buffer.readUshort(); //控制码
		// 						ushort ret = ReadBuffer.readUshort ();
		// 						byte[] payloads = ReadBuffer.readBytes ();

		// 						queue_payloads.Enqueue (payloads);
		// 						queue_ret.Enqueue (ret);
		// 						queue_cmd.Enqueue (cmd);
		// 						//DebugTool.LogBlue("<<< 普通的处理 ret: "+ ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉
		// 						queue_LogError.Enqueue ("<<< 普通的处理 ret: " + ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉

		// 					}

		// 					if (!LoginInfo.LoginState) {//特别处理 登陆
		// 						uint ID = ReadBuffer.readUint ();//bf.readUint();
		// 						uint SKEY = ReadBuffer.readUint ();//bf.readUint();
		// 						//DebugTool.LogBlue (string.Format ("<<< recv::特别处理-> 取得连接码用以重连,链接skey信息::id:: {0}, skey::{1} ", ID, SKEY)); TODO 注释掉

		// 						LoginInfo.LoginState = true;
		// 						LoginInfo.loginInfo [0] = ID;
		// 						LoginInfo.loginInfo [1] = SKEY;
		// 						//											DebugTool.Log (LoginInfo.loginInfo [0] + " |login| " + LoginInfo.loginInfo [1]);
		// 						queue_LogError.Enqueue ("登陆 id: " + LoginInfo.loginInfo [0] + " skey: " + LoginInfo.loginInfo [1]);
		// 						// 登陆成功 发送一般性 请求  as below::
		// 						//ConnetionTest.doRequest ();
		// 						//         IsConnectionSuccessful = true;
		// 						if (isInsteadOf == false) {//非被顶替性　连接 登陆
		// 							Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.Get_SKey_Success_Back.ToString (), null));
		// 						} else {
		// 							isInsteadOf = false;
		// 						}

		// 					}
		// 					//------------------------------------------------------------------------------


		// 					combinedLen -= msgSettingSize;
		// 					if (combinedLen > 0) {
		// 						//收到的包太长，一次未处理尽，属于粘包
		// 						index_getCombineSize += msgSettingSize;
		// 					}
		// 					//DebugTool.LogCyan(string.Format("处理后 收到字节对比: 实收:{0}|||额定:{1} ", bytesReadLen , msgSize));
		// 					if (reconnect_Lock) {
		// 						reconnect_Lock = false;
		// 					}
		// 					//}//end of if
		// 				}

		// 				//         bytesRead -= msgsize;
		// 				//         bufIndex += msgsize;
		// 			}
		// 		}
		// 	} catch (Exception e) {
		// 		queue_LogError.Enqueue ("Error ----------- socketDataArrivalHandler:: " + e.Message);
		// 	}
		// }
		

		
		/// <summary>
		/// socket由于连接中断(软/硬中断)的后续工作处理器
		/// </summary>
		private void OnSocketDisconnectedHandler ()
		{
			queue_LogError.Enqueue ("当断开时，发起重连----------- socketDisconnectedHandler --> Reconnection()");
			Reconnection ();
		}

		private ManualResetEvent timeOutObject = new ManualResetEvent (false);
		private const int timeoutLen = 1000;


		private string _hostName;
		private int _port;

		public static Socket m_Socket = null;
		private static   System.Timers.Timer aTimer;
		/// <summary>
		/// true: 启用重连 false: 初次连接,不可重连接
		/// </summary>
		private bool reconnect_Lock = false;
		 
		/// <summary>
		/// true 可触发重连机制一次
		/// </summary>
		public  bool toReconnect = false;
		private Queue<short> queue_cmd;
		private Queue<short> queue_ret;
		private Queue<byte[]> queue_payloads;
		private Queue<string> queue_LogError;


		private  IAsyncResult connect_TCP;

		private IPEndPoint _ipe;



		public  static  Dictionary<ushort, Type> MSG_haldlerPool;
		//    = new Dictionary<ushort, Type> {
		//	{ 4096,typeof(S2C_1_Handler) },
		//	{ 4097,typeof(S2C_2_Handler) },
		//	{ 4098,typeof(S2C_3_Handler) },
		//	{ 4099,typeof(S2C_4_Handler) }
		//  };

		public const int PAKH_EADER = 6;

		public UnitySocket ()
		{
			OnSocketDataArrival = UnPacket;
			OnSocketDisconnected = OnSocketDisconnectedHandler;

			reconnect_Lock = false;
			queue_cmd = new Queue<short> ();
			queue_ret = new Queue<short> ();
			queue_payloads = new Queue<byte[]> ();
			queue_LogError = new Queue<string> ();
			InitTimer ();
			TimerMgr.UpdateTime ("socketRuning" + UnityEngine.Random.Range (0f, 1f), () => {
				Updated ();
			});

		}

		protected virtual void Updated () //TODO xx
		{
			if (toReconnect) {//连接成功
				toReconnect = false;
				queue_LogError.Enqueue (Enum_NetEvent.WifiOff.ToString ());

			}


			while (queue_cmd.Count > 0) {//此处必需在主线程中运行,否则报错 e.g.:: get_realtimeSinceStartup can only be called from the main thread.
				DebugTool.LogYellow ("[TCP] [rcv] | queue_cmd.Count : " + queue_cmd.Count);
				short cmd = queue_cmd.Dequeue ();
				DebugTool.LogYellow ("[TCP] [rcv] cmd:::::::::: " + cmd + " | queue_cmd.Count remain : " + queue_cmd.Count);
				queue_LogError.Enqueue ("[TCP] [rcv] cmd:::::::::: " + cmd + " | queue_cmd.Count remain : " + queue_cmd.Count);
				CreateMessage (cmd); // cmd short -> int 
			}
   
			//
			if (queue_LogError.Count > 0) {
				string log = queue_LogError.Dequeue ();
				// DebugTool.Log (">>>>>>>>>> " + log); xxx 暂时去掉，以便调试UDP
				if (log == Enum_NetEvent.WifiOff.ToString ()) {
					TimerMgr.HeartBeatStop ();
					Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.WifiOff.ToString (), null));//　显示出ＷＩＦＩ标识
					Socket_Create_Connection ();//重新创建连接
				} else if (log == Enum_NetEvent.WifiOn.ToString ()) {
					//					    TimerMgr.HeartBeatStart ();
					Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.WifiOn.ToString (), null));
				} else if (log == Enum_Base.SocketClosed.ToString ()) {
					LoginInfo.LoginState = false;
					Facade_Base.instance.DispatchEvent (new CEvent (Enum_Base.SocketClosed.ToString (), null));
				}
			}
		}
		//分发消息
		private   IS2C_Handler CreateMessage (short cmd)//(RecvBuf buffer)
		{
			DebugTool.LogYellow ("CreateMessage() cmd: " + cmd);
			//(ushort)Opcode + (int)msgsize + (ushort)cmd + (ushort) ret
			// 此处buffer 已经读掉了头 和 长度 ,以下两条不要
			//		var wo = buffer.readUshort ();
			//		var len = buffer.readUint ();

			//			var op = buffer.getOpcode ();
			//			var cmd = ReadBuffer.readUshort();//buffer.readUshort(); //控制码
			short ret_errCode = queue_ret.Dequeue ();//ReadBuffer.readUshort ();//buffer.readUshort();//错误码 0: 无错
			var payloads = queue_payloads.Dequeue ();
			DebugTool.LogYellow ("CreateMessage() cmd: " + cmd + " ret :" + ret_errCode);
			// payload below ::
			//		ProtoBuf.Serializer.Deserialize<
			if (MSG_haldlerPool == null) {
				DebugTool.LogError ("CreateMessage() MSG_haldlerPool is null");
				return null;
			} 
			DebugTool.LogYellow ("CreateMessage() MSG_haldlerPool.Count: " + MSG_haldlerPool.Count);

   
			Type type = null;
			foreach (var item in MSG_haldlerPool) {
				// DebugTool.LogYellow ("[ TCP ] HAS Message() item.key:" + item.Key + " item.value:" + item.Value);

				if (item.Key == (cmd)) {
					type = item.Value;
				}
			}
			if (type != null) {
				DebugTool.LogYellow ("===== 完成 ===== CreateMessage() type is  " + type);
				IS2C_Handler rsp = null;
				rsp = (IS2C_Handler)Activator.CreateInstance (type);
				rsp.execute (cmd, ret_errCode, payloads);
				  
				return rsp;
			} else {
				DebugTool.LogError ("CreateMessage() type is null");
			}
 
			 
			return null;
		}

		private void InitTimer ()
		{
			queue_LogError.Enqueue ("InitTimer!");
			// Create a timer with a two second interval.
			aTimer = new System.Timers.Timer (6000);
			// Hook up the Elapsed event for the timer. 
			aTimer.Elapsed += OnTimedEvent;
			// Have the timer fire repeated events (true is the default)
			aTimer.AutoReset = false;
			// Start the timer
			aTimer.Enabled = true;
			aTimer.Stop ();
		}

		private void StopTimer ()
		{
			queue_LogError.Enqueue ("StopTimer!");
			aTimer.Stop ();
			reconnectTimes = 0;

		}

		int reconnectTimes = 0;

		private   void StartTimer ()
		{
			aTimer.Start ();
			queue_LogError.Enqueue ("StartTimer! reconnectTimes:: " + reconnectTimes);
			reconnectTimes++;
			if (reconnectTimes > 1) {

				queue_LogError.Enqueue ("自动重连超时,需重登!");
				queue_LogError.Enqueue (Enum_Base.SocketClosed.ToString ());
				StopTimer ();
				Close (m_Socket);
			}
		}

		private void OnTimedEvent (object source, System.Timers.ElapsedEventArgs e)
		{
			queue_LogError.Enqueue ("OnTimedEvent 间隔6秒! reconnect_Lock: " + reconnect_Lock);
			if (reconnect_Lock) {
				Reconnection ();
			}
		}



		#region socket connection

		public   void Socket_Create_Connection (string HostName, int LocalPort)
		{
			var iplist = Dns.GetHostAddresses (HostName);
			_hostName = HostName;
			_port = LocalPort;
			IPAddress _ip = iplist [0];//IPAddress.Parse (LocalIP);   
			_ipe = new IPEndPoint (_ip, _port);
			queue_LogError.Enqueue ("Socket_Create_Connection init");
			Socket_Create_Connection ();
		}

		public void Socket_Create_Connection ()//, Action onConnected, Action onConnectFail)
		{
			queue_LogError.Enqueue ("Socket_Create_Connection func");
			//--------------------------------------
			if ((m_Socket != null && m_Socket.Connected == true)) {
				Close (m_Socket);
				return;
			}
			m_Socket = null;
			timeOutObject.Reset ();//复位timeout事件
			m_Socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			queue_LogError.Enqueue ("a ------------- Socket_Create_Connection");
			try {
				#region 异步连接
				connect_TCP = m_Socket.BeginConnect (_ipe, AsyncConnectCallbac, m_Socket);
			} catch (Exception e) {
				queue_LogError.Enqueue ("b Error-------------Create Socket ::  BeginConnect::" + e.Message);
			}
			if (timeOutObject.WaitOne (timeoutLen)) {
				DebugTool.LogYellow ("连接成功 :) ");
			} else {
				DebugTool.LogYellow ("连接失败 :( ");
				
			}
			#endregion
		}


		#endregion

		#region socket close

		public   void Close (Socket socket)
		{
			try {
				  
				socket.Close ();
			} catch (SocketException e) {
				queue_LogError.Enqueue ("错误! Close: msg: " + e.Message + " | errorCode: " + e.ErrorCode);
			}
		}

		#endregion

		/// <summary>
		/// Hearts the beat.心跳包发送
		/// </summary>
		//public void HeartBeat (short commandID)
		//{
		// //DebugTool.Log ("send heartbeat");
		// sbyte[] buffer = BufferManager.getSendBuffer (commandID, true).last ().getBuffer ();
		// byte[] bytes = MD5.sbytToByte (buffer);
		// this.Send(bytes);
		//}

		 
		public bool IsConnected ()
		{
			if (m_Socket == null || m_Socket.Connected == false) {
				return false;
			}
			return true;
		}
		 

		// true 被顶替的
		private bool isInsteadOf = false;

		private void AsyncConnectCallbac (IAsyncResult iar)
		{
			Socket client = (Socket)iar.AsyncState;
			if (!client.Connected) {
				queue_LogError.Enqueue ("client.Connected status :: " + client.Connected);
				return;
			}
			queue_LogError.Enqueue ("!!!!!!!!!!!!!!!!连接成功返回 ---- client.Connected :: " + client.Connected);
			try {
				client.EndConnect (iar);
				if (LoginInfo.loginInfo [0] == 0 && LoginInfo.loginInfo [1] != 0 && isInsteadOf == false) {
					StopTimer ();
					isInsteadOf = true;//此号被顶
					queue_LogError.Enqueue ("此号被顶,打断自动重连机制");
					queue_LogError.Enqueue (Enum_Base.SocketClosed.ToString ());

				} else {
					KeepAlive ();
					reconnect_Lock = false;
					//xxxxx 以下机制为初次skey 为0时发起申请skey，以作为重连依据,如果没有这样的机制，先注释掉 TODO
					//QuestKey_for_Reconnect (LoginInfo.loginInfo [0], LoginInfo.loginInfo [1],reconnect_Lock);//　连接成功，立马申请skey
				}
				//					_locked = false;
			} catch (Exception e) {
				//					IsconnectSuccess = false;
				queue_LogError.Enqueue ("Error ------ AsyncConnectCallbac error 无连接:: 重连 " + e.Message);
			} finally {
				queue_LogError.Enqueue ("AsyncConnectCallbac ------ finally");
				timeOutObject.Set ();	
			}

			//			}
		}

		#region==== Async receive

		private byte[] buffer = new byte[1024];
		private void KeepAlive ()
		{
			try {
				queue_LogError.Enqueue ("<color=yellow>KeepAlive ::ing...</color>");
				// m_Socket.BeginReceive (CombineBuffer.Buffer, 0, CombineBuffer.BUF_MAX, SocketFlags.None, new AsyncCallback (OnReceiveCallback), m_Socket);
				m_Socket.BeginReceive (buffer, 0, 1024, SocketFlags.None, new AsyncCallback (OnRcv), m_Socket);
			} catch (Exception e) {
				queue_LogError.Enqueue ("Error --x--- KeepAlive :: " + e.Message);
			}
		}
		private int copy_to_index = 0;
		//============================================
		//		  byte[] buffer = new byte[4096];
		//  byte[] bufCache = null;
		//  public RecvBuf bf = null;
		//		public int bfLen = 0;
		//		public int _msgsize = 0;
		//		public int opcode = 0;
		//		private int headIndex = 0;
		/// <summary>
		/// 回调函数，处理服务端返回之SOCKET通信消息
		/// </summary>
		/// <param name="ar"></param>
		public void OnRcv (IAsyncResult ar)
		{
			try {
				queue_LogError.Enqueue("<color=green>[TCP] OnRcv</color>");
				Socket peerSocket = (Socket)ar.AsyncState;
				if (peerSocket.Connected == false)
					return;
				//方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
				int bytesReadLen = peerSocket.EndReceive (ar);

				queue_LogError.Enqueue ("<<< <color=yellow>收到socket</color> 长度 :: " + bytesReadLen + " peerSocketConnection: " + peerSocket.Connected);
				if (bytesReadLen > 0) {


					//DebugTool.LogCyan(string.Format("<<< AsyncReceiveMsg :: 新增 收到字节长度 : {0}", bytesReadLen));// TODO 注释掉

					// bytesReadLen = CombineBuffer.GetRealReadBuf (bytesReadLen);
					// byte[] real = CombineBuffer.CombinedBuff;

						byte[] real = CopyToBigEndianBytes(this.buffer,0,bytesReadLen, copy_to_index ,false);
						// foreach(var item in real ){
						// 	queue_LogError.Enqueue("rcv: " + item);	
						// }
					//粘包处理
					// if (OnSocketDataArrival != null) {
					// 	OnSocketDataArrival (real);
					// }
					if (OnSocketDataArrival != null) {
						queue_LogError.Enqueue("解包");
						real = OnSocketDataArrival (real);
						copy_to_index = real.Length;
					}else{
						queue_LogError.Enqueue("无解包器");
					}


				} else {
					queue_LogError.Enqueue ("peerSocket收到字节是0");// All the data has arrived; put it in response. TODO 注释掉
						

					if (peerSocket != null && peerSocket.Connected) {//aaaaa
						peerSocket.Shutdown (SocketShutdown.Both);//aaaaa
						peerSocket.Disconnect (false);//aaa
						peerSocket.Close ();//aaa
					}

					return;//xxxxx
				}
				Thread.Sleep (10);
				KeepAlive ();
			} catch (SocketException ex) {
				
				queue_LogError.Enqueue ("错误----- socket 接收错误: msg: " + ex.Message + " |  errorCode: " + ex.ErrorCode);

				if (OnSocketDisconnected != null) {
					queue_LogError.Enqueue ("A Error----- socket 接收错误: Reconnection " + ex.Message);
					if (m_Socket != null)
						queue_LogError.Enqueue ("Ax Error----- socket 接收错误: m_Socket.Connected : " + m_Socket.Connected);
					if (!reconnect_Lock) {
						OnSocketDisconnected ();
						reconnect_Lock = true;
					}
					queue_LogError.Enqueue (string.Format ("reconnect_Lock: {0} ", reconnect_Lock));
				}
			} finally {
				queue_LogError.Enqueue ("OnReceiveCallback ----- finally ");
			}
		}

		 
		private void Reconnection ()
		{
			toReconnect = true; //TODO xx
			queue_LogError.Enqueue ("1---------- Reconnection 发起自动重连!! ----------");
			
			try {
				Close (m_Socket);
			} catch (Exception e) {
				queue_LogError.Enqueue ("4 Error ---------- Reconnection 发起自动重连 error! " + e.Message);
			}
			StartTimer ();
			 
		}






		// / <summary>
		// / 复制数组
		// / </summary>
		// / <param name="dest">to destination</param>
		// / <param name="src">from source</param>
		// / <param name="begin"></param>
		// / <param name="size"></param>
		// public void CopyArray (Array dest, Array src, int begin, int size)
		// {
		// 	if (dest == null || dest.Length < begin + size || src.Length < begin + size)
		// 		return;
		// 	for (int i = begin; i < size; i++)
		// 		dest.SetValue (src.GetValue (i), i);
		// }

		#endregion

		private bool fisrtTime = true;
		//-------------------------------------------------
		/// <summary>
		/// 连接建立，立马进行skey申请(skey初次为0,再次为1表示为重连接); //xxxxx 以下机制为初次skey 为0时发起申请skey，以作为重连依据,如果没有这样的机制，先注释掉 TODO
		/// </summary>
		/// <param name="id"></param>
		/// <param name="skey"></param>
		/// <param name="locked"></param>
		private void QuestKey_for_Reconnect (uint id, uint skey, bool locked = false)
		{
			//	 if (!checkSocketState ()) return false;
			StopTimer ();
			queue_LogError.Enqueue (string.Format ("id:{0}, skey:{1},QuestKey_for_Reconnect locked:{2} , reconnect_Lock: {3}", id, skey, locked, reconnect_Lock));
			if (reconnect_Lock == false && fisrtTime == false) {
				queue_LogError.Enqueue (Enum_NetEvent.WifiOn.ToString ());
			}
			fisrtTime = false;
			if (reconnect_Lock)
				return;//true: 重连中,禁用发送


			if (m_Socket != null) {
				if (m_Socket.Connected) {
					//     reconnect_Lock = locked;
					//     if (reconnect_Lock)
					//     {
					//      StartTimer();
					//     }
					byte[] bytes = System.Text.Encoding.UTF8.GetBytes ("DB");
					short opcode = TypeConvert.getShort (bytes, true);
					try {
						SendBuffer.writeOPcode (opcode);
						SendBuffer.writeUint (id);
						SendBuffer.writeUint (skey);
						SendBuffer.last ();
//						var bytes_send = SendBuffer.sendBuf;

//						var realsize = SendBuffer.GetMsgSize ();
						//			DebugTool.LogBlue (">>> send connect len: " + realsize);  //14 TODO 注释掉
						int sended_Size = 0;
						do {
							sended_Size += m_Socket.Send (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None);
						} while (sended_Size < SendBuffer.GetMsgSize ());
					} catch (Exception e) {
						queue_LogError.Enqueue ("SendConnect :: Serialize_mobile:: " + e.Message);
					}
				} else {
					queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::  connected : false ");
				}
			}
			return;
		}
		public void Send<T> (short cmd, T data, bool locked = false)
		{
			// queue_LogError.Enqueue ("[TCP] <color=red>Send</color><" + typeof(T) + ">  , cmd: " + cmd);
			if (reconnect_Lock)
				return;

			if (m_Socket != null) {
				if (m_Socket.Connected) {
					
					var rqst = data;
					byte[] pb_stream = ProtoTool.Serialize_mobile<T> (rqst, ProtoTool.proto);//protobuf 也无需再次转为BigEndian

					byte[] buffer_snd = this.Packet(cmd,pb_stream);
					// queue_LogError.Enqueue("[TCP] ======= ");
					// foreach(var item in buffer_snd){
					// 	queue_LogError.Enqueue("[TCP] 发:" + item);
					// }
					try {
						int sended_Size = 0;
						do {
							sended_Size += m_Socket.Send (buffer_snd, sended_Size, buffer_snd.Length - sended_Size, SocketFlags.None);
						} while (sended_Size < buffer_snd.Length);

					} catch (Exception e) {
						queue_LogError.Enqueue ("Send :: Serialize_mobile:: " + e.Message);
					}
				} else {
					Reconnection ();
				}
			}
			//------------------------------------------------------------------
		}
// 		public void Send2<T> (short cmd, T data, bool locked = false)
// 		{
// 			queue_LogError.Enqueue ("Send<" + typeof(T) + ">  , cmd: " + cmd);
// 			if (reconnect_Lock)
// 				return;

// 			if (m_Socket != null) {
// 				if (m_Socket.Connected) {
					


// 					byte[] opcode_bytes = System.Text.Encoding.UTF8.GetBytes ("DB");
// 					short opcode = TypeConvert.getShort (opcode_bytes, true);
// 					var rqst = data;//new CalcAddReq ();
// 					try {
// 						byte[] bytes_strm = ProtoTool.Serialize_mobile<T> (rqst, ProtoTool.proto);
// 						SendBuffer.writeOPcode (opcode);
// 						SendBuffer.writeUshort ((ushort)cmd);
// 						SendBuffer.writeBytes (bytes_strm);
// 						SendBuffer.last ();

// //						var bytes = SendBuffer.sendBuf;
// 						var realsize = SendBuffer.GetMsgSize ();

// 						//DebugTool.LogCyan(">>> send buf len: " + realsize + " cmd: " + cmd+ "  pb: " + data.GetType().FullName); //33 TODO 注释掉
// 						queue_LogError.Enqueue (">>>xxxxx send buf len: " + realsize + " cmd: " + cmd + "  pb: " + data.GetType ().FullName);
// 						SendBuffer.printBytes ();
// 						int sended_Size = 0;
// 						do {
// 							sended_Size += m_Socket.Send (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None);
// 						} while (sended_Size < SendBuffer.GetMsgSize ());

// 					} catch (Exception e) {
// 						queue_LogError.Enqueue ("Send :: Serialize_mobile:: " + e.Message);
// 					}
// 				} else {
// 					Reconnection ();
// 				}
// 			}
// 			//------------------------------------------------------------------
// 		}
		//end func
		/// <summary>
		/// Sends the bytes.已处理了opcode + length ,型参中需含 cmd + playload
		/// </summary>
		/// <param name="bytes">Bytes.</param>
		/// <param name="locked">If set to <c>true</c> locked.</param>
// 		public void SendBytes (byte[] bytes, bool locked = false)
// 		{
// 			//	 if (!checkSocketState ()) return false;
// 			StopTimer ();
// 			queue_LogError.Enqueue (string.Format ("SendBytes len:{0} locked:{1} , reconnect_Lock: {2}", bytes.Length, locked, reconnect_Lock));
// 			if (reconnect_Lock == false && fisrtTime == false) {
// 				queue_LogError.Enqueue (Enum_NetEvent.WifiOn.ToString ());
// 			}
// 			fisrtTime = false;
// 			if (reconnect_Lock)
// 				return;//true: 重连中,禁用发送


// 			if (m_Socket != null) {
// 				if (m_Socket.Connected) {
// 					byte[] OPcodeBytes = System.Text.Encoding.UTF8.GetBytes ("DB");
// 					short opcode = TypeConvert.getShort (OPcodeBytes, true);//true BigEndian
// 					try {
// 						SendBuffer.writeOPcode (opcode);//0,1 下标位写入opcode
// 						SendBuffer.writeBytes (bytes);
// 						SendBuffer.last ();//尾部写入opcode至结尾的全部长度放在 2,3,4,5的下标位
// //						var bytes_send = SendBuffer.sendBuf;

// //						var realsize = SendBuffer.GetMsgSize ();
// 						SendBuffer.printBytes ();
// 						//			DebugTool.LogBlue (">>> send connect len: " + realsize);  //14 TODO 注释掉
// 						int sended_Size = 0;
// 						do {
// 							sended_Size += m_Socket.Send (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None);
// 						} while (sended_Size < SendBuffer.GetMsgSize ());
// 					} catch (Exception e) {
// 						queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::III " + e.Message);
// 					}
// 				} else {
// 					queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::  connected : false ");
// 				}
// 			}
// 			return;
// 		}
		//end func

		//autoConvertToBigEndian : true  对字节码做反转，转为BigEndian ,因为网络字节序都是BigEndian
		public static byte[] CopyToBigEndianBytes(byte[] sourceBytes , int sourceStartIndex , int len , int copytoindex = 0, bool autoConvertToBigEndian = true){
			var byte__ = new byte[len];
			Array.Copy(sourceBytes,sourceStartIndex,byte__,0,len);
			if(BitConverter.IsLittleEndian && autoConvertToBigEndian){
				Array.Reverse(byte__);
			}
			return byte__;
		}

		 
	}
	// end class
}
// end package




public class LoginInfo
{
	public  static  int[] loginInfo = new int[2];
	public  static  bool LoginState = false;
}

