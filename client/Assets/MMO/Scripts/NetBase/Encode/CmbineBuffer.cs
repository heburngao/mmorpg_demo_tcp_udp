using System;

namespace ghbc.Net
{
	/// <summary>
	/// Combine buffer,处理 粘残包操作
	/// </summary>
	public class CombineBuffer
	{
		public static int BUF_MAX = 1024;
		//================= 粘包相关 =================
		/// <summary>
		/// 读取的缓存带
		/// </summary>
		public static byte[] Buffer = new byte[BUF_MAX];
		//缓存包

		/// <summary>
		/// 残包 ，缓存带
		/// </summary>
		private static byte[] tooShortPackage;
		//　短包
		/// <summary>
		/// 读取缓存 整包
		/// </summary>
		public static byte[] CombinedBuff;
		//　最终的包

		//		private static int _i;


		/// <summary>
		/// 残包， 粘包 下标
		/// </summary>
		public static int bufIndex;

		/// <summary>
		/// 读取整包，且返回长度
		/// </summary>
		/// <returns>The real read buffer.</returns>
		/// <param name="byteSize">Byte size.</param>
		public static int GetRealReadBuf (int byteSize)
		{
			bufIndex = 0;
			if (tooShortPackage != null) {//合并上次收到的残余包
				CombinedBuff = new byte[tooShortPackage.Length + byteSize];
				//DebugTool.LogBlue(string.Format("合包后，长: {0}", realReadBuf.Length)); TODO 注释掉
				Array.Copy (tooShortPackage, 0, CombinedBuff, 0, tooShortPackage.Length);//把上次收到的太短的包粘上
				Array.Copy (Buffer, 0, CombinedBuff, tooShortPackage.Length, byteSize);//在太短的包后面，追加新收到的buffer
				tooShortPackage = null;
			} else {
				CombinedBuff = new byte[byteSize];
				Array.Copy (Buffer, 0, CombinedBuff, 0, byteSize);
			}
			return CombinedBuff.Length;
		}

		/// <summary>
		/// 按fullSize指定尺寸截取一段buff , bufIndex += fullSize 向后顺推
		/// </summary>
		/// <returns>The real read buffer item.</returns>
		/// <param name="bytesRead">Bytes read.</param>
		public static byte[] GeUsefulBuff (int fullSize)
		{
			byte[] fullUsefulBuff = new byte[fullSize];
			Array.Copy (CombinedBuff, bufIndex, fullUsefulBuff, 0, fullSize);
			bufIndex += fullSize;
			return fullUsefulBuff;
		}

		/// <summary>
		/// 残包,缓存起来
		/// </summary>
		/// <param name="bytesRead"></param>
		public static void TooShortReceive (int bytesRead)
		{
			tooShortPackage = new byte[bytesRead];
			Array.Copy (CombinedBuff, bufIndex, tooShortPackage, 0, bytesRead);
		}

		//		public static short GetOpcode ()//"WO"
		//		{
		//			// 要预留2个字节出来记录Size的
		//			return (short)((realReadBuf [0] << 8) + realReadBuf [1]);
		////			return (short)((realReadBuf [bufIndex] << 8) + realReadBuf [1]);
		//		}
		//----------------读取长度--------------------------
		//		private static int _msgsize;

		/// <summary>
		/// 读取当前字节序 长度
		/// </summary>
		/// <returns>The send buffer length.</returns>
		public static int GetMsgSize ()
		{
			//			_msgsize += realReadBuf [2] << 24;
			//			_msgsize += realReadBuf [3] << 16;
			//			_msgsize += realReadBuf [4] << 8;
			//			return _msgsize + realReadBuf [5];
			//return (int)(realReadBuf[bufIndex++] << 24) + (realReadBuf[bufIndex++] << 16) + (realReadBuf[bufIndex++] << 8) + realReadBuf[bufIndex++];
			return (int)(CombinedBuff [2]) << 24 + (CombinedBuff [3]) << 16 + (CombinedBuff [4]) << 8 + CombinedBuff [5];//超长包粘包
			//return (int)((realReadBuf[bufIndex++] << 24) | (realReadBuf[bufIndex++] << 16) | (realReadBuf[bufIndex++] << 8) | realReadBuf[bufIndex++]);//如果是粘包(两包合一)，则继续bufIndex++
		}

		public static int GetMsgSize (int startIndex)
		{
			startIndex += 2;//绕过opcode
			return (int)((CombinedBuff [startIndex++]) << 24 | (CombinedBuff [startIndex++]) << 16 | (CombinedBuff [startIndex++]) << 8 | CombinedBuff [startIndex++]);//如果是粘包(两包合一)，则继续bufIndex++
		}
	}
}

