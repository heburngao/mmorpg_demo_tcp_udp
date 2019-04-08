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
	public abstract class UnitySocket_UDP
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
			int i = 0;
				if (len_rcv < i+HeaderLen+BufferLen) {
					// PR("[UDP] break len_rcv: " , len_rcv)
					queue_LogError.Enqueue ("[UDP] len_rcv < i+HeaderLen+BufferLen break");
					return new byte[]{};
				}
				byte[] buf_header_words = UnitySocket.CopyToBigEndianBytes(buffer,i,HeaderLen,false);//DB无需转为BigEndian
				 
				 queue_LogError.Enqueue ("[UDP] =====解包=======");
				if(System.Text.Encoding.UTF8.GetString(buf_header_words) == HeadWords){ //无需转为BigEndian
					//messagelength : BufferLen 长度对应的内容表达长度
					//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
					queue_LogError.Enqueue ("[UDP] =====" + HeadWords + "=======");
					byte[] buf_totoal_len =  UnitySocket.CopyToBigEndianBytes(buffer ,i+HeaderLen, BufferLen);
					int DB_ttsize_cmd_ret_payload = BitConverter.ToInt32(buf_totoal_len,0);//IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf_msg_len,0));//BytesToInt(buffer[i+HeaderLen : i+HeaderLen+BufferLen])
					queue_LogError.Enqueue ("[UDP] =====buffer总长: " + DB_ttsize_cmd_ret_payload + " =======");
					//if len_rcv < i+HeaderLen+BufferLen+messageLength {
					if (len_rcv < i+ DB_ttsize_cmd_ret_payload) {
						queue_LogError.Enqueue ("[UDP] break b " + len_rcv + ", "+ DB_ttsize_cmd_ret_payload +","+ i+","+ HeaderLen +","+ BufferLen);
						return new byte[] {};
					}
					byte[] buf_cmd_ret_payload =  UnitySocket.CopyToBigEndianBytes(buffer,i+HeaderLen+BufferLen ,DB_ttsize_cmd_ret_payload - HeaderLen - BufferLen , false);
			 
					 
					//================================
							if (LoginInfo.LoginState) {//登陆后的处理 //  CMD + RET + PAYLOAD  
																	//  short + short + byte[]
														var cmd_byte =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,0,CMDLen);
								short cmd = BitConverter.ToInt16(cmd_byte,0);//ReadBuffer.readUshort ();//buffer.readUshort(); //控制码
														var ret_byte =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen,RETLen);
								short ret = BitConverter.ToInt16(ret_byte,0);//ReadBuffer.readUshort ();
								//payloads由protobuffer编码而成，所以不需要转为BigEndian
														byte[] payloads =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen + RETLen,buf_cmd_ret_payload.Length - CMDLen - RETLen,false);
								 
								queue_LogError.Enqueue("[UDP] <color=yellow>登陆后的处理</color>");

								queue_payloads.Enqueue (payloads);
								queue_ret.Enqueue (ret);
								queue_cmd.Enqueue (cmd);
								queue_LogError.Enqueue ("[UDP] <<< 普通的处理 ret: " + ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉

							}

							if (!LoginInfo.LoginState) {//特别处理 登陆
								queue_LogError.Enqueue("[UDP] <color=yellow>未登陆后的处理</color>");
								int ID = BitConverter.ToInt32(buf_cmd_ret_payload,0);//ReadBuffer.readUint ();//bf.readUint();
								int SKEY = BitConverter.ToInt16(buf_cmd_ret_payload,4);//ReadBuffer.readUint ();//bf.readUint();

								LoginInfo.LoginState = true;
								LoginInfo.loginInfo [0] = ID;
								LoginInfo.loginInfo [1] = SKEY;
								//											DebugTool.Log (LoginInfo.loginInfo [0] + " |login| " + LoginInfo.loginInfo [1]);
								queue_LogError.Enqueue ("[UDP] 登陆 id: " + LoginInfo.loginInfo [0] + " skey: " + LoginInfo.loginInfo [1]);
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
					// i = i + DB_ttsize_cmd_ret_payload - 1;
					i = i + DB_ttsize_cmd_ret_payload;
					//PR("[UDP] cc " , data , " index: " , i)
				}

			if (i == len_rcv) {
				queue_LogError.Enqueue ( "[UDP] return over " + i);
				return new byte[]{};//make([]byte, 0)
			}
			if (reconnect_Lock) {
				reconnect_Lock = false;
			}
			queue_LogError.Enqueue ("[UDP] xxxxxx return buffer.Length : " + buffer.Length);

			byte[] buf_next = new byte[buffer.Length - i];
			Array.Copy(buffer ,i,buf_next,0, buffer.Length - i); 
			return buf_next;

		}
		private byte[] UnPacket2 (byte[] buffer){

			// DB + LEN + CMD + RET + PAYLOAD  
			// short + int + short + short + byte[]

			// func Unpack(buffer []byte, readerChannel chan []byte) []byte {
			var len_rcv = buffer.Length;
			int i ;
			for (i = 0; i < len_rcv; i = i + 1) {
				if (len_rcv < i+HeaderLen+BufferLen) {
					// PR("[UDP] break len_rcv: " , len_rcv)
					queue_LogError.Enqueue ("[UDP] len_rcv < i+HeaderLen+BufferLen break");
					break;
				}
				byte[] buf_header_words = UnitySocket.CopyToBigEndianBytes(buffer,i,HeaderLen,false);//DB无需转为BigEndian
				 
				 queue_LogError.Enqueue ("[UDP] =====解包=======");
				if(System.Text.Encoding.UTF8.GetString(buf_header_words) == HeadWords){ //无需转为BigEndian
					//messagelength : BufferLen 长度对应的内容表达长度
					//[68 66 0 0 0 15 39 16 8 189 8 16 192 196 7]  = messageLength = buffer[i+HeaderLen : i+HeaderLen+BufferLen] = [0,0,0,15]
					queue_LogError.Enqueue ("[UDP] =====" + HeadWords + "=======");
					byte[] buf_totoal_len =  UnitySocket.CopyToBigEndianBytes(buffer ,i+HeaderLen, BufferLen);
				 

					int DB_ttsize_cmd_ret_payload = BitConverter.ToInt32(buf_totoal_len,0);//IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf_msg_len,0));//BytesToInt(buffer[i+HeaderLen : i+HeaderLen+BufferLen])
					queue_LogError.Enqueue ("[UDP] =====buffer总长: " + DB_ttsize_cmd_ret_payload + " =======");
					//if len_rcv < i+HeaderLen+BufferLen+messageLength {
					if (len_rcv < i+ DB_ttsize_cmd_ret_payload) {
						// PR("[UDP] break b " , len_rcv, totoal_msg_len , i, HeaderLen , BufferLen)
						break;
					}
					byte[] buf_cmd_ret_payload =  UnitySocket.CopyToBigEndianBytes(buffer,i+HeaderLen+BufferLen ,DB_ttsize_cmd_ret_payload - HeaderLen - BufferLen , false);
			 
					 
					//================================
							if (LoginInfo.LoginState) {//登陆后的处理 //  CMD + RET + PAYLOAD  
																	//  short + short + byte[]
														var cmd_byte =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,0,CMDLen);
								short cmd = BitConverter.ToInt16(cmd_byte,0);//ReadBuffer.readUshort ();//buffer.readUshort(); //控制码
														var ret_byte =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen,RETLen);
								short ret = BitConverter.ToInt16(ret_byte,0);//ReadBuffer.readUshort ();
								//payloads由protobuffer编码而成，所以不需要转为BigEndian
														byte[] payloads =  UnitySocket.CopyToBigEndianBytes(buf_cmd_ret_payload,CMDLen + RETLen,buf_cmd_ret_payload.Length - CMDLen - RETLen,false);
								 
								queue_LogError.Enqueue("[UDP] <color=yellow>登陆后的处理</color>");

								queue_payloads.Enqueue (payloads);
								queue_ret.Enqueue (ret);
								queue_cmd.Enqueue (cmd);
								queue_LogError.Enqueue ("[UDP] <<< 普通的处理 ret: " + ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉

							}

							if (!LoginInfo.LoginState) {//特别处理 登陆
								queue_LogError.Enqueue("[UDP] <color=yellow>未登陆后的处理</color>");
								int ID = BitConverter.ToInt32(buf_cmd_ret_payload,0);//ReadBuffer.readUint ();//bf.readUint();
								int SKEY = BitConverter.ToInt16(buf_cmd_ret_payload,4);//ReadBuffer.readUint ();//bf.readUint();

								LoginInfo.LoginState = true;
								LoginInfo.loginInfo [0] = ID;
								LoginInfo.loginInfo [1] = SKEY;
								//											DebugTool.Log (LoginInfo.loginInfo [0] + " |login| " + LoginInfo.loginInfo [1]);
								queue_LogError.Enqueue ("[UDP] 登陆 id: " + LoginInfo.loginInfo [0] + " skey: " + LoginInfo.loginInfo [1]);
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
					//PR("[UDP] cc " , data , " index: " , i)
				}
			}

			if (i == len_rcv) {
				// PR("[UDP] return over " , i)
				return new byte[]{};//make([]byte, 0)
			}
			if (reconnect_Lock) {
				reconnect_Lock = false;
			}
			// PR("[UDP] xxxxxx return buffer[i:] : " , buffer[i:])

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

					

					Array.Copy(opcode_bytes,0,buffer_snd,0,HeaderLen); //DB short
					Array.Copy(totoal_len_bytes,0,buffer_snd,HeaderLen,BufferLen); //LEN int , need convert to BigEndian
					Array.Copy(cmd_bytes,0,buffer_snd,HeaderLen+BufferLen,CMDLen); // CMD short , need convert to BigEndian
					Array.Copy(pb_stream,0,buffer_snd,HeaderLen+BufferLen+CMDLen,pb_stream.Length); //protobuff  []byte
					return buffer_snd;
		}

		/// <summary>
		/// 异步收到消息处理器
		/// </summary>
		/// <param name="data"></param>
// 		private   void OnSocketDataArrivalHandler2 (byte[] CombinedData)
// 		{
// 			try {
// 				queue_LogError.Enqueue ("解包DataArrival --------------- socketDataArrivalHandler: " + CombinedData.Length + " isLogin: " + LoginInfo.LoginState);
// 				queue_LogError.Enqueue ("解包DataArrival ---------------  isInsteadOf:: " + isInsteadOf + " isLogin:: " + LoginInfo.LoginState);
// 				var combinedLen = CombinedData.Length;
// 				var index_getCombineSize = 0;
// 				while (combinedLen > 0) {

// 					if (combinedLen < PAKH_EADER) {// 接收小于定义的收，继续接收
// 						combineBuffer.TooShortReceive (combinedLen);
// 						//DebugTool.LogCyan("接收不完整 ,则缓存之");
// 						queue_LogError.Enqueue ("接收头部不完整 ,则缓存之");
// 						break;
// 					} else {
// 						// modify by ghb 20160720
// 						int msgSettingSize = combineBuffer.GetMsgSize (index_getCombineSize);//GetMsgSize(); 额定包长
// 						//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} || 额定包长:{1}", bytesReadLen, msgSize));
// 						queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} || 额定包长:{1}", combinedLen, msgSettingSize));
// 						if (combinedLen < msgSettingSize) { // 实收包长 < 第一份额定包长定义
// 							//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} < 额定包长:{1}", bytesReadLen, msgSize));// TODO 注释掉
// 							queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} < 额定包长:{1}", combinedLen, msgSettingSize));
// 							combineBuffer.TooShortReceive (combinedLen);//粘包
// 							break;
// 						} else {

// 							//if (bytesReadLen > msgSettingSize)
// 							//{//超长粘包处理

// 							//}
// 							//else
// 							//{//实收包长 == 额定包长定义
// 							//  TODO ....
// 							////最终合并
// 							byte[] realReadBufItem = combineBuffer.GeUsefulBuff (msgSettingSize);//实际收到的 总包片段
// 							//DebugTool.LogCyan(string.Format("<<< 实收包长:{0} >= 额定包长:{1}", bytesReadLen, msgSize));
// 							queue_LogError.Enqueue (string.Format ("<<< 实收包长:{0} >= 额定包长:{1}", combinedLen, msgSettingSize));
// 							//DebugTool.LogCyan("混合总包长度 :" + realReadBufItem.Length);
// 							queue_LogError.Enqueue (string.Format ("混合总包长度 :" + realReadBufItem.Length));
// 							//================ 解读 =================
// 							ReadBuffer_Dynamic readBuffer = new ReadBuffer_Dynamic ();
// 							readBuffer.StartReadBuffer (realReadBufItem);//设置到  读取 缓存带中  正式解包
// 							short msgOpcode = readBuffer.GetOpcode ();
// 							int msgsize = readBuffer.GetMsgSize ();
// 							readBuffer.printBytes (combinedLen);
// 							//DebugTool.LogCyan(">>>>>>>>>>>>>>>> 解码::  opcode : " + msgOpcode + " | msgsize : " + msgsize); //TODO 注释掉
// 							queue_LogError.Enqueue (">>>>>>>>>>>>>>>> 解码::  opcode : " + msgOpcode + " | msgsize : " + msgsize);

// 							//  isLogin 与 !isLogin 的顺序不能换 
// 							if (LoginInfo.LoginState) {//登陆后的处理
// 								//DebugTool.LogBlue("<<< 普通的处理"); //TODO 注释掉
// 								//CreateMessage(bf);
// 								//											(short)Opcode+(int)size + (ushort)cmd + (ushort)ret + (byte[]) payloads

// 								ushort cmd = readBuffer.readUshort ();//buffer.readUshort(); //控制码
// 								ushort ret = readBuffer.readUshort ();
// 								byte[] payloads = readBuffer.readBytes ();

// 								queue_payloads.Enqueue (payloads);
// 								queue_ret.Enqueue (ret);
// 								queue_cmd.Enqueue (cmd);
// 								//DebugTool.LogBlue("<<< 普通的处理 ret: "+ ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉
// 								queue_LogError.Enqueue ("<<< 普通的处理 ret: " + ret + " cmd: " + cmd + " payloads.len: " + payloads.Length); //TODO 注释掉

// 								//							while (queue_cmd.Count > 0)
// 								//							{
// 								//								try{
// 								//									
// 								//									ushort _cmd = queue_cmd.Dequeue();
// 								//										queue_LogError.Enqueue("cmd: " + _cmd + " | queue_cmd.Count: " + queue_cmd.Count);
// 								//									CreateMessage(_cmd);
// 								//								}catch(Exception e){
// 								//										queue_LogError.Enqueue("CreateMessage::" + e.Message);
// 								//								}
// 								//							}
// 							}

// 							if (!LoginInfo.LoginState) {//特别处理 登陆
// 								//																				var ret = ReadBuffer.readUshort();//buffer.readUshort();//错误码 0: 无错
// 								//									BaseMessage msg = map[msgOpcode].Create();
// 								//									MSG_haldlerPool[
// 								int ID = readBuffer.readUint ();//bf.readUint();
// 								int SKEY = readBuffer.readUint ();//bf.readUint();
// 								//DebugTool.LogBlue (string.Format ("<<< recv::特别处理-> 取得连接码用以重连,链接skey信息::id:: {0}, skey::{1} ", ID, SKEY)); TODO 注释掉

// 								//isLogining = true;
// 								LoginInfo.LoginState = true;
// 								LoginInfo.loginInfo [0] = ID;
// 								LoginInfo.loginInfo [1] = SKEY;
// 								// 记录以作 断线重连用
// 								//											DebugTool.Log (LoginInfo.loginInfo [0] + " |login| " + LoginInfo.loginInfo [1]);
// 								queue_LogError.Enqueue ("登陆 id: " + LoginInfo.loginInfo [0] + " skey: " + LoginInfo.loginInfo [1]);
// 								// 登陆成功 发送一般性 请求  as below::
// 								//ConnetionTest.doRequest ();
// 								//         IsConnectionSuccessful = true;
// 								if (isInsteadOf == false) {//非被顶替性　连接 登陆
// 									Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.Get_SKey_Success_Back.ToString (), null));
// 								} else {
// 									isInsteadOf = false;
// 								}

// 							}
// 							//------------------------------------------------------------------------------


// 							combinedLen -= msgSettingSize;
// 							if (combinedLen > 0) {
// 								//收到的包太长，一次未处理尽，属于粘包
// //								index_getCombineSize = msgSettingSize;//下标顺延
// 								index_getCombineSize += msgSettingSize;
// 							}
// 							//DebugTool.LogCyan(string.Format("处理后 收到字节对比: 实收:{0}|||额定:{1} ", bytesReadLen , msgSize));
// 							if (reconnect_Lock) {
// 								reconnect_Lock = false;
// 							}
// 							//}//end of if
// 						}

// 						//         bytesRead -= msgsize;
// 						//         bufIndex += msgsize;
// 					}
// 				}
// 			} catch (Exception e) {
// 				queue_LogError.Enqueue ("Error ----------- socketDataArrivalHandler:: " + e.Message);
// 			}
// 		}

		/// <summary>
		/// socket由于连接中断(软/硬中断)的后续工作处理器
		/// </summary>
		private void OnSocketDisconnectedHandler ()
		{
			queue_LogError.Enqueue ("[UDP] 当断开时，发起重连----------- socketDisconnectedHandler --> Reconnection()");
			Reconnection ();
		}

		private ManualResetEvent timeOutObject = new ManualResetEvent (false);
		//		private bool IsconnectSuccess = false; //异步连接情况，由异步连接回调函数置位
		//		private object lockObj_IsConnectSuccess = new object();
		//private   Exception socketexception;
		private const int timeoutLen = 1000;


		//	public   Action _onConnectionSuccess;
		//		public   Action _onConnectionFail;
		private string _hostName;
		private int _port;

		public static Socket m_Socket = null;
		private static   System.Timers.Timer aTimer;
		/// <summary>
		/// true: 启用重连 false: 初次连接,不可重连接
		/// </summary>
		private bool reconnect_Lock = false;
		//  /// <summary>
		//  /// false 可开启重连机制一次 , true 锁定重连机制, true:给 reconnect_Lock 赋初始值
		//  /// </summary>
		//  private bool _locked = false;Z
		/// <summary>
		/// true 可触发重连机制一次
		/// </summary>
		public  bool toReconnect = false;
		private Queue<short> queue_cmd;
		private Queue<short> queue_ret;
		private Queue<byte[]> queue_payloads;
		private Queue<string> queue_LogError;
		//		public   bool canDequeue = true;


		private  IAsyncResult connect;

		private IPEndPoint _serverIPE;
		private EndPoint _serverPE;


		//	private   int errorCount = 0;

		public  static  Dictionary<ushort, Type> MSG_haldlerPool;
		//    = new Dictionary<ushort, Type> {
		//	{ 4096,typeof(S2C_1_Handler) },
		//	{ 4097,typeof(S2C_2_Handler) },
		//	{ 4098,typeof(S2C_3_Handler) },
		//	{ 4099,typeof(S2C_4_Handler) }
		//  };

		public const int PAKH_EADER = 6;
		//protected   bool isLogining = false;

		public UnitySocket_UDP ()
		{
			OnSocketDataArrival = this.UnPacket;
			OnSocketDisconnected = OnSocketDisconnectedHandler;

			//DebugTool.LogError("unitysocket:" + this.GetHashCode());
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
				//				queue_LogError.Enqueue("------ 去重连!! ------");
				queue_LogError.Enqueue (Enum_NetEvent.WifiOff.ToString ());
				////			SocketConnection(_hostName, _port, _onConnectionSuccess, _onConnectionFail);
				//								AutoReconnectSocketConnection ();

			}
			//
			//		if (IsConnect())
			//		{
			//			if (_onConnectionSuccess != null)
			//			{
			//				_onConnectionSuccess();
			//				_onConnectionSuccess = null;
			//			}
			//
			//		}
			//		//			else {
			//		//
			//		//				if (m_Socket != null)
			//		//					m_Socket.EndConnect (connect);
			//		//				if (_onConnectionFail != null) {
			//		//					_onConnectionFail ();
			//		//					_onConnectionFail = null;
			//		//				}
			//		//			}
			//


			while (queue_cmd.Count > 0) {//此处必需在主线程中运行,否则报错 e.g.:: get_realtimeSinceStartup can only be called from the main thread.
//				try {
				short cmd = queue_cmd.Dequeue ();
//				queue_LogError.Enqueue ("cmd:::::::::: " + cmd + " | queue_cmd.Count remain : " + queue_cmd.Count);
				DebugTool.LogYellow ("[ UDP ] cmd:::::::::: " + cmd + " | queue_cmd.Count remain : " + queue_cmd.Count);
				CreateMessage (cmd);
//				} catch (Exception e) {
//					queue_LogError.Enqueue ("Error ------- CreateMessage::" + e.Message);
//				}
			}
   
			//
			if (queue_LogError.Count > 0) {
				string log = queue_LogError.Dequeue ();
				// DebugTool.Log ("[UDP] >>>>>>>>>> " + log);
				if (log == Enum_NetEvent.WifiOff.ToString ()) {
					TimerMgr.HeartBeatStop ();
					//if (isInsteadOf == false && isLogining == true) {
					Facade_Base.instance.DispatchEvent (new CEvent (Enum_NetEvent.WifiOff.ToString (), null));//　显示出ＷＩＦＩ标识
					Socket_Create_Connection ();//重新创建连接
					//}else 
					//{
					//Facade.instance.DispatchEvent(new CEvent(Enum_Base.SocketClosed.ToString(), null));//显示出　重登陆ＵＩ


					//}
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
			DebugTool.LogYellow ("[ UDP ]CreateMessage() cmd: " + cmd);
			//(ushort)Opcode + (int)msgsize + (ushort)cmd + (ushort) ret
			// 此处buffer 已经读掉了头 和 长度 ,以下两条不要
			//		var wo = buffer.readUshort ();
			//		var len = buffer.readUint ();

			//			var op = buffer.getOpcode ();
			//			var cmd = ReadBuffer.readUshort();//buffer.readUshort(); //控制码
			short ret = queue_ret.Dequeue ();//ReadBuffer.readUshort ();//buffer.readUshort();//错误码 0: 无错
			var payloads = queue_payloads.Dequeue ();
			DebugTool.LogYellow ("[ UDP ]CreateMessage() cmd: " + cmd + " ret :" + ret);
			// payload below ::
			//		ProtoBuf.Serializer.Deserialize<
			if (MSG_haldlerPool == null) {
				DebugTool.LogError ("[ UDP ]CreateMessage() MSG_haldlerPool is null");
				return null;
			} 
			DebugTool.LogYellow ("[ UDP ]CreateMessage() MSG_haldlerPool.Count: " + MSG_haldlerPool.Count);

   
			//try
			//{
			Type type = null;
			foreach (var item in MSG_haldlerPool) {
				// DebugTool.LogYellow ("[ UDP ]HAS Message() item.key:" + item.Key + " item.value:" + item.Value);

				if (item.Key == (cmd)) {
					type = item.Value;
				}
			}
			if (type != null) {
				DebugTool.LogYellow ("[ UDP ]===== 完成 ===== CreateMessage() type is  " + type);
				IS2C_Handler rsp = (IS2C_Handler)Activator.CreateInstance (type);
				rsp.execute (cmd, ret, payloads);
				//				canDecode = true;
				return rsp;
			} else {
				DebugTool.LogError ("[ UDP ]CreateMessage() type is null");
			}
			//}catch(Exception e)
			//{
			// DebugTool.LogError(e.Message+" | " + MSG_haldlerPool.Count);
			//}
			return null;
		}

		private void InitTimer ()
		{
			queue_LogError.Enqueue ("[UDP] InitTimer!");
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
			queue_LogError.Enqueue ("[UDP] StopTimer!");
			aTimer.Stop ();
			reconnectTimes = 0;

		}

		int reconnectTimes = 0;

		private   void StartTimer ()
		{
			aTimer.Start ();
			queue_LogError.Enqueue ("[UDP] StartTimer! reconnectTimes:: " + reconnectTimes);
			reconnectTimes++;
			if (reconnectTimes > 1) {
				//try
				//{

				queue_LogError.Enqueue ("[UDP] 自动重连超时,需重登!");
				queue_LogError.Enqueue (Enum_Base.SocketClosed.ToString ());
				StopTimer ();
				Close (m_Socket);
				//}catch(Exception e)
				//{
				// queue_LogError.Enqueue("Error---- 自动重连超时,需重登!");
				//}
			}
		}

		private void OnTimedEvent (object source, System.Timers.ElapsedEventArgs e)
		{
			queue_LogError.Enqueue ("[UDP] OnTimedEvent 间隔6秒! reconnect_Lock: " + reconnect_Lock);
			if (reconnect_Lock) {
				Reconnection ();
			}
		}


		// private CmbineBuffer_Dynamic combineBuffer;

		#region socket connection

		public   void Socket_Create_Connection (string HostName, int LocalPort)
		{
			var iplist = Dns.GetHostAddresses (HostName);
			_hostName = HostName;
			_port = LocalPort;
			IPAddress _ip = iplist [0];//IPAddress.Parse (LocalIP);   
			_serverIPE = new IPEndPoint (_ip, _port);
			queue_LogError.Enqueue ("[UDP] Socket_Create_Connection init");
			Socket_Create_Connection ();
		}

		public void Socket_Create_Connection ()//, Action onConnected, Action onConnectFail)
		{
			queue_LogError.Enqueue ("[UDP] Socket_Create_Connection func");
			//--------------------------------------
//			if ((m_Socket != null && m_Socket.Connected == true)) { xxx
//				Close (m_Socket);xxx
//				return;xxx
//			}xxx
			m_Socket = null;
			timeOutObject.Reset ();//复位timeout事件 
			// #############################################################################################################################
			m_Socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			// combineBuffer = new CmbineBuffer_Dynamic ();
			// #############################################################################################################################
			queue_LogError.Enqueue ("[UDP] a ------------- Socket_Create_Connection");
			try {
				//				m_Socket.SendTimeout = timeoutLen;

				//--------------------------------------
				//   _onConnectionSuccess = onConnected;
				//   _onConnectionFail = onConnectFail;


				#region 异步连接
				 
				//UDP
				// #############################################################################################################################
//				m_Socket.Bind (_serverIPE); //只用于udp
				_serverPE = (EndPoint)_serverIPE;
				// m_Socket.BeginReceiveFrom (combineBuffer.Buffer, 0, CombineBuffer.BUF_MAX, SocketFlags.None, ref _serverPE, new AsyncCallback (OnRcv_UDP), null);
				//   m_Socket.BeginReceiveFrom (this.buffer, 0, 1024, SocketFlags.None, ref _serverPE, new AsyncCallback (OnRcv_UDP), m_Socket);
				KeepAlive();
				// #############################################################################################################################
			} catch (Exception e) {
				queue_LogError.Enqueue ("[UDP] b Error-------------Create Socket ::  BeginConnect::" + e.Message);
				//				 if(OnSocketDisconnected != null){
				//				 	OnSocketDisconnected();
				//				 }
			}
			//tcp
//			if (timeOutObject.WaitOne (timeoutLen)) {
//				DebugTool.LogYellow ("连接成功 :) ");
//			} else {
//				DebugTool.LogYellow ("连接失败 :( ");
//				
//			}
			//tcp
			#endregion
		}


		#endregion

		#region socket close

		public   void Close (Socket socket)
		{
			try {
//				if (socket != null )//&&  socket.Connected)
//				{
		 
//					if (socket.Connected) {
//						socket.Shutdown (SocketShutdown.Both);
//						socket.Disconnect (false);
				//				IsconnectSuccess = false;
//					m_Socket.Shutdown (SocketShutdown.Both);
				socket.Close ();
//					}
				//					socket = null;
				//m_Socket = null;
				//isLogining = false;
//				}
			} catch (SocketException e) {
				queue_LogError.Enqueue ("[UDP] 错误! Close: msg: " + e.Message + " | errorCode: " + e.ErrorCode);
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

		#region  send scoekt api 
		/// <summary>
		/// 发送字节流
		/// </summary>
		/// <param name="data">Data.</param>
		//  public virtual void Send (byte[] data, int len)  TODO by ghb
		//		{
		//			if (IsConnect ()) {
		//				try {
		////					m_Socket.Send (data, 0, len, SocketFlags.None);
		//					int sended_Size = 0;
		//					do {
		//						sended_Size += m_Socket.Send (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None);
		//					} while (sended_Size < SendBuffer.GetMsgSize ());
		//				} catch (Exception e) {
		//					Reconnection ();
		//					//DebugTool.LogError (e.Message);
		//				}
		//			} else {
		//				Reconnection ();
		//			}
		//		}

		//	public   bool IsConnect(){
		//		if (m_Socket == null)
		//	   {
		//	    DebugTool.LogError("socket is null");
		//	    return false;
		//	   }
		//		#region 过程
		//		// This is how you can determine whether a socket is still connected.
		//		bool connectState = true;
		//		bool blockingState = m_Socket.Blocking;
		//		try
		//		{
		//			byte[] tmp = new byte[1];
		//
		//			m_Socket.Blocking = false;
		//			m_Socket.Send(tmp, 0, 0);
		//			queue_LogError.Enqueue("a Connected!");
		//			connectState = true; //若Send错误会跳去执行catch体，而不会执行其try体里其之后的代码
		//		}
		//		catch (SocketException e)
		//		{
		//			// 10035 == WSAEWOULDBLOCK
		//			if (e.NativeErrorCode.Equals(10035))
		//			{
		//					queue_LogError.Enqueue("b Error----- Still Connected, but the Send would block");
		//				connectState = true;
		//			}
		//
		//			else
		//			{
		//					queue_LogError.Enqueue(string.Format("c Error----- Disconnected: error code {0}!", e.NativeErrorCode));
		//				connectState = false;
		//			}
		//		}
		//		finally
		//		{
		//				m_Socket.Blocking = blockingState;
		//		}
		//
		//			queue_LogError.Enqueue(string.Format("d Connected: {0}", m_Socket.Connected));
		//			queue_LogError.Enqueue(string.Format("e connectState:{0}", connectState));
		//		return connectState;
		//		#endregion
		//	}
		//  public   bool IsConnect()  //TODO xx
		//  {
		//   if (m_Socket == null)
		//   {
		////    DebugTool.LogError("socket is null");
		//    return false;
		//   }
		//   if (!m_Socket.Connected)
		//   {
		////    DebugTool.LogError("socket 链接关闭");
		//    //m_Socket.Close ();
		////    m_Socket.Shutdown(SocketShutdown.Both);
		////    m_Socket.Close();
		////    m_Socket = null;
		////    Facade.instance.DispatchEvent(new CEvent(Enum_Base.SocketAutoReconnect.ToString(), null));
		////		queue_LogError.Enqueue(Enum_Base.SocketAutoReconnect.ToString());
		////		Reconnection ();
		//    return false;
		//   }
		//   return true;
		//  }
		//
		#endregion
		#region connect call back for tcp
		// true 被顶替的
		private bool isInsteadOf = false;

// 		private void AsyncConnectCallbac (IAsyncResult iar) //弃用，udp本来就无连接
// 		{
// 			//			lock (lockObj_IsConnectSuccess) {
// 			Socket client = (Socket)iar.AsyncState;
// 			//			if (!client.Connected) {tcp
// 			//				queue_LogError.Enqueue ("client.Connected status :: " + client.Connected);tcp
// 			//					StartTimer();
// 			//				return;tcp
// //			}tcp
// 			//			queue_LogError.Enqueue ("!!!!!!!!!!!!!!!!连接成功返回 ---- client.Connected :: " + client.Connected);tcp
// 			try {
// 				client.EndConnect (iar);
// 				//					IsconnectSuccess = true;
// 				if (LoginInfo.loginInfo [0] == 0 && LoginInfo.loginInfo [1] != 0 && isInsteadOf == false) {
// 					StopTimer ();
// 					isInsteadOf = true;//此号被顶
// 					queue_LogError.Enqueue ("此号被顶,打断自动重连机制");
// 					queue_LogError.Enqueue (Enum_Base.SocketClosed.ToString ());

// 				} else {
// 					//isInsteadOf = false;
// 					KeepAlive ();
// 					reconnect_Lock = false;
// 					//xxxxx 以下机制为初次skey 为0时发起申请skey，以作为重连依据,如果没有这样的机制，先注释掉 TODO
// 					//QuestKey_for_Reconnect (LoginInfo.loginInfo [0], LoginInfo.loginInfo [1],reconnect_Lock);//　连接成功，立马申请skey
// 				}
// 				//					_locked = false;
// 			} catch (Exception e) {
// 				//					IsconnectSuccess = false;
// 				queue_LogError.Enqueue ("Error ------ AsyncConnectCallbac error 无连接:: 重连 " + e.Message);
// 				//					if (OnSocketDisconnected != null) {//TODO xxx
// 				//       				OnSocketDisconnected ();
// 				//     			}
// 			} finally {
// 				queue_LogError.Enqueue ("AsyncConnectCallbac ------ finally");
// 				timeOutObject.Set ();	
// 			}

// 			//			}
// 		}

		#endregion

		#region==== Async receive
		private byte[] buffer = new byte[1024];
		private void KeepAlive ()
		{
			try {
				queue_LogError.Enqueue ("<color=cyan>[UDP] KeepAlive ::ing...</color>");
//				m_Socket.BeginReceive (CombineBuffer.buffer, 0, CombineBuffer.BUF_MAX, SocketFlags.None, new AsyncCallback (OnReceiveCallback), m_Socket);
				// m_Socket.BeginReceiveFrom (combineBuffer.Buffer, 0, CombineBuffer.BUF_MAX, SocketFlags.None, ref _serverPE, new AsyncCallback (OnReceiveCallback_UDP), m_Socket);
				m_Socket.BeginReceiveFrom (buffer, 0, 1024, SocketFlags.None, ref _serverPE, new AsyncCallback (OnRcv_UDP), m_Socket);
				// m_Socket.BeginConnect(_serverPE,new AsyncCallback(OnRcv_UDP),m_Socket);
				
			} catch (Exception e) {
				//					if (OnSocketDisconnected != null) {
				//       				OnSocketDisconnected ();
				//     			}
				queue_LogError.Enqueue ("[UDP] Error ----- KeepAlive :: " + e.Message);
			}
		}


		public void OnSendCallback_UDP (IAsyncResult ar)
		{
			DebugTool.LogBlue ("[UDP] <<< ####### 收到UDP socket 发送成功的 ［ 反馈 ］!!!! ----------------------------============="); //TODO 注释掉
			//			lock (lockObj_IsConnectSuccess) {
			//if (m_Socket != null) {
			//	if (m_Socket.Connected) {
			if (ar.IsCompleted == false)
				return;

			try {
				//      IsConnectionSuccessful = false;
				//				Socket peerSocket = (Socket)ar.AsyncState;tcp
				//				if (peerSocket.Connected == false)tcp
				//					return;tcp

				//方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
				//				int bytesReadLen = peerSocket.EndReceive (ar);UDP
				int bytesReadLen = m_Socket.EndSendTo (ar);//UDP
    DebugTool.LogOrange ("[UDP] len:" + bytesReadLen + " serverip:" + _serverPE);
				//DebugTool.LogPurple ("<<< 收到socket 长度 :: " + bytesReadLen);// TODO 注释掉
				//				queue_LogError.Enqueue ("<<< 收到socket 长度 :: " + bytesReadLen + " peerSocketConnection: " + peerSocket.Connected); tcp
				if (bytesReadLen > 0) {
					//DebugTool.LogCyan(string.Format("<<< AsyncReceiveMsg :: 新增 收到字节长度 : {0}", bytesReadLen));// TODO 注释掉

//					bytesReadLen = combineBuffer.GetRealReadBuf (bytesReadLen);
//					byte[] real = combineBuffer.CombinedBuff;
//
//					//粘包处理
//					if (OnSocketDataArrival != null) {
//						OnSocketDataArrival (real);
//					}
				 

				} else {
					queue_LogError.Enqueue ("[UDP] peerSocket收到字节是0");// All the data has arrived; put it in response. TODO 注释掉
					 
					return;//xxxxx
					 
				}
//				Thread.Sleep (10);
//				KeepAlive ();
				timeOutObject.WaitOne ();	
			} catch (SocketException ex) {

				//      IsConnectionSuccessful = false;
				//      Reconnection();
				//socketexception = ex;
				queue_LogError.Enqueue ("[UDP] 错误----- socket 接收错误: msg: " + ex.Message + " |  errorCode: " + ex.ErrorCode);

				if (OnSocketDisconnected != null) {
					queue_LogError.Enqueue ("[UDP] A Error----- socket 接收错误: Reconnection " + ex.Message);
					//					if (m_Socket != null)tcp
					//						queue_LogError.Enqueue ("Ax Error----- socket 接收错误: m_Socket.Connected : " + m_Socket.Connected);tcp
					//								if (_locked == false) {
					if (!reconnect_Lock) {
						//									_locked = true;
						OnSocketDisconnected ();
						reconnect_Lock = true;
						//									StartTimer();
					}
					//								queue_LogError.Enqueue (string.Format ("_locked: {0} ", _locked));
					queue_LogError.Enqueue (string.Format ("[UDP] reconnect_Lock: {0} ", reconnect_Lock));
					// return;
				}
			} finally {
				queue_LogError.Enqueue ("[UDP] OnReceiveCallback ----- finally ");
				//      		timeOutObject.Set();
			}
			//					} else {
			//						queue_LogError.Enqueue ("----- socket 未连接,重连 ");
			////						if (OnSocketDisconnected != null) {
			////							OnSocketDisconnected ();
			////						}
			//					}
			//				}
			//			}
		}
		//============================================
		//		  byte[] buffer = new byte[4096];
		//  byte[] bufCache = null;
		//  public RecvBuf bf = null;
		//		public int bfLen = 0;
		//		public int _msgsize = 0;
		//		public int opcode = 0;
		//		private int headIndex = 0;
		private int copy_from_index = 0;
		/// <summary>
		/// 回调函数，处理服务端返回之SOCKET通信消息
		/// </summary>
		/// <param name="ar"></param>
		public void OnRcv_UDP (IAsyncResult ar)
		{
			
			//			lock (lockObj_IsConnectSuccess) {
			//if (m_Socket != null) {
			//	if (m_Socket.Connected) {
			if (ar.IsCompleted == false)
				return;
			queue_LogError.Enqueue("[UDP] <color=yellow>[UDP] OnRcv_UDP</color>");
			// DebugTool.LogBlue ("<<< 收到UDP socket !!!! ----------------------------============="); //TODO 注释掉
			try {
				//      IsConnectionSuccessful = false;
//				Socket peerSocket = (Socket)ar.AsyncState;tcp
//				if (peerSocket.Connected == false)tcp
//					return;tcp

				//方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
//				int bytesReadLen = peerSocket.EndReceive (ar);UDP
				int bytesReadLen = m_Socket.EndReceiveFrom (ar, ref _serverPE);//UDP
				DebugTool.LogYellow ("[UDP] len:" + bytesReadLen + " serverip:" + _serverPE);
				//DebugTool.LogPurple ("<<< 收到socket 长度 :: " + bytesReadLen);// TODO 注释掉
//				queue_LogError.Enqueue ("<<< 收到socket 长度 :: " + bytesReadLen + " peerSocketConnection: " + peerSocket.Connected); tcp
				if (bytesReadLen > 0) {
					//DebugTool.LogCyan(string.Format("<<< AsyncReceiveMsg :: 新增 收到字节长度 : {0}", bytesReadLen));// TODO 注释掉
					// bytesReadLen = combineBuffer.GetRealReadBuf (bytesReadLen);
					// byte[] real = combineBuffer.CombinedBuff;

					// //粘包处理
					// if (OnSocketDataArrival != null) {
					// 	OnSocketDataArrival (real);
					// }

					//改成如下
					// byte[] real = CombineBuffer.CombinedBuff;
						// byte[] real = this.buffer;
						byte[] real = UnitySocket.CopyToBigEndianBytes(this.buffer,copy_from_index,bytesReadLen,false);
					//粘包处理
					// if (OnSocketDataArrival != null) {
					// 	OnSocketDataArrival (real);
					// }
					if (OnSocketDataArrival != null) {
						real = OnSocketDataArrival (real);
						copy_from_index = real.Length;
					}



					//DebugTool.LogCyan(string.Format("<<< AsyncReceiveMsg :: 并包后 整包 长度 : {0}", bytesReadLen));//, CombineBuffer.GetMsgSize ())); TODO 注释掉 TODO 注释掉

					//DebugTool.LogBlue ("BeginReceive::totoal buflen 剩余长度 ::" + bytesReadLen); TODO 注释掉
					//接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
					//			_socket.BeginReceive (buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback (AsyncReceiveMsg), _socket);
					//Get the rest of the data.
					//       KeepAlive(); TODO xx

					 
				} else {
					queue_LogError.Enqueue ("[UDP] peerSocket收到字节是0");// All the data has arrived; put it in response. TODO 注释掉
						
//					Close (peerSocket);//xxxxx


//					if (peerSocket != null && peerSocket.Connected) {//aaaaa tcp
					//						peerSocket.Shutdown (SocketShutdown.Both);//aaaaa  tcp
					//						peerSocket.Disconnect (false);//aaa tcp
					//						peerSocket.Close ();//aaa tcp
//					}

//						Close(m_Socket);

//					queue_LogError.Enqueue (Enum_Base.SocketClosed.ToString ());xxxxx
					return;//xxxxx
					//								if (m_Socket.Connected) {
					//									if (OnSocketDisconnected != null) {
					//										OnSocketDisconnected ();
					//										queue_LogError.Enqueue ("退出不再 BeginReceive");
					////							return;
					//									}
					//								}
				}
				Thread.Sleep (10);
				KeepAlive ();
				timeOutObject.Set ();	//??TODO 要不要换成WaitOne ??
			} catch (SocketException ex) {
				
				//      IsConnectionSuccessful = false;
				//      Reconnection();
				//socketexception = ex;
				queue_LogError.Enqueue ("[UDP] 错误----- socket 接收错误: msg: " + ex.Message + " |  errorCode: " + ex.ErrorCode);

				if (OnSocketDisconnected != null) {
					queue_LogError.Enqueue ("[UDP] A Error----- socket 接收错误: Reconnection " + ex.Message);
					//					if (m_Socket != null)tcp
//						queue_LogError.Enqueue ("Ax Error----- socket 接收错误: m_Socket.Connected : " + m_Socket.Connected);tcp
					//								if (_locked == false) {
					if (!reconnect_Lock) {
						//									_locked = true;
						OnSocketDisconnected ();
						reconnect_Lock = true;
						//									StartTimer();
					}
					//								queue_LogError.Enqueue (string.Format ("_locked: {0} ", _locked));
					queue_LogError.Enqueue (string.Format ("[UDP] reconnect_Lock: {0} ", reconnect_Lock));
					// return;
				}
			} finally {
				queue_LogError.Enqueue ("[UDP] OnReceiveCallback ----- finally ");
				//      		timeOutObject.Set();
			}
			//					} else {
			//						queue_LogError.Enqueue ("----- socket 未连接,重连 ");
			////						if (OnSocketDisconnected != null) {
			////							OnSocketDisconnected ();
			////						}
			//					}
			//				}
			//			}
		}

		//		private void reconnect(){
		//			try{
		//				QuestKey_for_Reconnect (LoginInfo.loginInfo [0], LoginInfo.loginInfo [1]);
		//			}catch(Exception e){
		//				queue_LogError.Enqueue ("Error ---------- reconnect :: error! "+e.Message);
		//			}
		//		}
		private void Reconnection ()
		{
			toReconnect = true; //TODO xx
			//		reconnectFnc = reconnect;
			queue_LogError.Enqueue ("[UDP] 1---------- Reconnection 发起自动重连!! ----------");
			//    	Array.Clear(CombineBuffer.readBuf, 0, CombineBuffer.readBuf.Length);
			// //			if (m_Socket != null && m_Socket.Connected) {
			// 			if (m_Socket != null  || m_Socket.Connected) {
			// 				queue_LogError.Enqueue(string.Format("2---------- Reconnection 发起自动重连!! ----socket:{0}, connected:{1}------",m_Socket, m_Socket.Connected));
			// 			}
			// 			try{
			// //			if (m_Socket != null  || m_Socket.Connected) {
			// 				queue_LogError.Enqueue(string.Format("3---------- SocketShutdown !! ----socket:{0}, connected:{1}------",m_Socket, m_Socket.Connected));
			// 				m_Socket.Disconnect (true);
			// 				IsconnectSuccess = false;
			// 				m_Socket.Shutdown (SocketShutdown.Both);
			// 				m_Socket.Close ();
			// 				m_Socket = null;
			// 					queue_LogError.Enqueue(string.Format("3x---------- SocketShutdown !! ----socket:{0}, connected:{1}------",m_Socket, m_Socket.Connected));
			// //				Close ();
			// //			}
			// 			}catch(Exception e){
			// 				queue_LogError.Enqueue ("4 Error ---------- Reconnection error! "+ e.Message);
			// 			}
			// 			queue_LogError.Enqueue("5---------- Reconnection 发起自动重连!! Socket_Create_Connection ----------");
			try {
				Close (m_Socket);
				//				Socket_Create_Connection();
			} catch (Exception e) {
				queue_LogError.Enqueue ("[UDP] 4 Error ---------- Reconnection 发起自动重连 error! " + e.Message);
			}
			StartTimer ();
			//Close (); ?
			//SocketConnection(_hostName, _port, _onConnectionSuccess, _onConnectionFail);
			//SendConnect(LoginInfo.loginInfo[0], LoginInfo.loginInfo[1]);
			//			SendLogin (); -> api迁移 move to updated()  to run;
		}






		/// <summary>
		/// 复制数组
		/// </summary>
		/// <param name="dest">to destination</param>
		/// <param name="src">from source</param>
		/// <param name="begin"></param>
		/// <param name="size"></param>
		public void CopyArray (Array dest, Array src, int begin, int size)
		{
			if (dest == null || dest.Length < begin + size || src.Length < begin + size)
				return;
			for (int i = begin; i < size; i++)
				dest.SetValue (src.GetValue (i), i);
		}

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
			queue_LogError.Enqueue (string.Format ("[UDP] id:{0}, skey:{1},QuestKey_for_Reconnect locked:{2} , reconnect_Lock: {3}", id, skey, locked, reconnect_Lock));
			if (reconnect_Lock == false && fisrtTime == false) {
				queue_LogError.Enqueue (Enum_NetEvent.WifiOn.ToString ());
			}
			fisrtTime = false;
			if (reconnect_Lock)
				return;//true: 重连中,禁用发送


			if (m_Socket != null) {
				//				if (m_Socket.Connected) {tcp
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
					queue_LogError.Enqueue ("[UDP] SendConnect :: Serialize_mobile:: " + e.Message);
				}
//			} else {tcp
				//					queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::  connected : false ");tcp
				//      if(OnSocketDisconnected != null){
				//      	OnSocketDisconnected();
				//      }
//			}tcp
			}
			return;
		}
		public void Send<T> (short cmd, T data, bool locked = false)
		{
			 queue_LogError.Enqueue ("[UDP] <color=yellow>[UDP]</color> Send<" + typeof(T) + ">  , cmd: " + cmd);
			if (reconnect_Lock)
				return;

			if (m_Socket != null) {
				// if (m_Socket.Connected) {
					
					var rqst = data;
					byte[] pb_stream = ProtoTool.Serialize_mobile<T> (rqst, ProtoTool.proto);//protobuf 也无需再次转为BigEndian

					byte[] buffer_snd = this.Packet(cmd,pb_stream);
					// queue_LogError.Enqueue("[UDP] ======= ");
					// foreach(var item in buffer_snd){
					// 	queue_LogError.Enqueue("[UDP] 发:" + item);
					// }
					try {
						int sended_Size = 0;
						do {
							// sended_Size += m_Socket.Send (buffer_snd, sended_Size, buffer_snd.Length - sended_Size, SocketFlags.None);
							sended_Size += m_Socket.SendTo(buffer_snd,sended_Size,buffer_snd.Length-sended_Size,SocketFlags.None,_serverPE);
						} while (sended_Size < buffer_snd.Length);

					} catch (Exception e) {
						queue_LogError.Enqueue ("[UDP] Send :: Serialize_mobile:: " + e.Message);
					}
				// } else {
				// 	Reconnection ();
				// }
			}
		}
		public void Send2<T> (ushort cmd, T data, bool locked = false)
		{
			queue_LogError.Enqueue ("[UDP] Send<" + typeof(T) + ">  , cmd: " + cmd);
			if (reconnect_Lock)
				return;

			if (m_Socket != null) {
//				if (m_Socket.Connected) {
				//     reconnect_Lock = locked;
				//     if (reconnect_Lock)
				//     {
				//      StartTimer();
				//     }

				byte[] opcode_bytes = System.Text.Encoding.UTF8.GetBytes ("DB");
				short opcode = TypeConvert.getShort (opcode_bytes, true);
				var rqst = data;//new CalcAddReq ();
				try {
					byte[] bytes_strm = ProtoTool.Serialize_mobile<T> (rqst, ProtoTool.proto);
					SendBuffer.writeOPcode (opcode);
					SendBuffer.writeUshort (cmd);
					SendBuffer.writeBytes (bytes_strm);
					SendBuffer.last ();

//						var bytes = SendBuffer.sendBuf;
					var realsize = SendBuffer.GetMsgSize ();

					//DebugTool.LogCyan(">>> send buf len: " + realsize + " cmd: " + cmd+ "  pb: " + data.GetType().FullName); //33 TODO 注释掉
					queue_LogError.Enqueue ("[UDP] >>>xxxxx send buf len: " + realsize + " cmd: " + cmd + "  pb: " + data.GetType ().FullName);
					SendBuffer.printBytes ();
					int sended_Size = 0;
					do {
						
//						sended_Size += m_Socket.SendTo (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None, _serverIPE);
						//m_Socket.BeginSendTo (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None, _serverIPE, new AsyncCallback (OnSendCallback_UDP), m_Socket);
      //或 as below
      					m_Socket.SendTo(SendBuffer.sendBuf,sended_Size,SendBuffer.GetMsgSize()-sended_Size,SocketFlags.None,_serverPE);
						sended_Size += SendBuffer.GetMsgSize ();
					} while (sended_Size < SendBuffer.GetMsgSize ());

				} catch (Exception e) {
					queue_LogError.Enqueue ("[UDP] Send :: Serialize_mobile:: " + e.Message);
				}
//				} else {
//					Reconnection ();
//				}
			}
			//------------------------------------------------------------------
		}
// 		//end func
// 		/// <summary>
// 		/// Sends the bytes.已处理了opcode + length ,型参中需含 cmd + playload
// 		/// </summary>
// 		/// <param name="bytes">Bytes.</param>
// 		/// <param name="locked">If set to <c>true</c> locked.</param>
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
// //				if (m_Socket.Connected) {tcp
// 				//     reconnect_Lock = locked;
// 				//     if (reconnect_Lock)
// 				//     {
// 				//      StartTimer();
// 				//     }
// 				byte[] OPcodeBytes = System.Text.Encoding.UTF8.GetBytes ("DB");
// 				short opcode = TypeConvert.getShort (OPcodeBytes, true);//true BigEndian
// 				try {
// 					SendBuffer.writeOPcode (opcode);//0,1 下标位写入opcode
// 					SendBuffer.writeBytes (bytes);
// 					SendBuffer.last ();//尾部写入opcode至结尾的全部长度放在 2,3,4,5的下标位
// //						var bytes_send = SendBuffer.sendBuf;

// //						var realsize = SendBuffer.GetMsgSize ();
// 					SendBuffer.printBytes ();
// 					//			DebugTool.LogBlue (">>> send connect len: " + realsize);  //14 TODO 注释掉
// 					int sended_Size = 0;
// 					do {
// //							sended_Size += m_Socket.Send (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None);
// 						sended_Size += m_Socket.SendTo (SendBuffer.sendBuf, sended_Size, SendBuffer.GetMsgSize () - sended_Size, SocketFlags.None, _serverIPE);
// 					} while (sended_Size < SendBuffer.GetMsgSize ());
// 				} catch (Exception e) {
// 					queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::III " + e.Message);
// 				}
// //				} else {tcp
// 				//					queue_LogError.Enqueue ("SendConnect :: Serialize_mobile::  connected : false ");tcp
// 				//      if(OnSocketDisconnected != null){
// 				//      	OnSocketDisconnected();
// 				//      }
// //				}tcp
// 			}
// 			return;
// 		}
		//end func
	}
	// end class
}
// end package




//public class LoginInfo
//{
//	public  static  uint[] loginInfo = new uint[2];
//	public  static  bool LoginState = false;
//}

