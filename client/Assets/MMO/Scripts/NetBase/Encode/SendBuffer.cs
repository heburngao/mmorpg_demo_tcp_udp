using System;

namespace ghbc.Net
{
	/// <summary>
	/// Stream buffer ， 压处字节序操作 本code采用大端
	/// //小端是低端数据存放在低端地址，大端是高端数据存在低端地址。
	/// </summary>
	public class SendBuffer
	{



		//================= 压包相关 =================
		private static int BUF_MAX = 1024;
		/// <summary>
		/// 写入 字节序 下标
		/// </summary>
		private static int sendbufIndex;
		/// <summary>
		/// 写入的缓存带
		/// </summary>
		public static byte[] sendBuf = new byte[BUF_MAX];
		private static int utflen, i, strlen;
		private static char c;
		private static int idxRec;

		public SendBuffer ()
		{
		}

		public static void printBytes ()
		{
			for (int i = 0; i < GetMsgSize (); i++) {//sendBuf.Length; i++)
				DebugTool.LogCyan ("index:" + i + "/" + sendBuf [i].ToString ("x2") + " / " + sendBuf [i].ToString ());
			}
		}

		public static byte[] GetBytes_HasData ()
		{
//			ReadBuffer.ResetIndex ();
//			return ReadBuffer.readBytes (sendbufIndex);
			byte[] result = new byte[sendbufIndex];
			Array.Copy (sendBuf, 0, result, 0, result.Length);
			return result;
		}

		public static void ResetIndex ()
		{
			sendbufIndex = 0;
		}

		public static void writeBoolean (bool b)
		{
			sendBuf [sendbufIndex++] = (byte)(b ? 1 : 0);
		}

		public static void writeByte (byte b)
		{
			sendBuf [sendbufIndex++] = b;
		}

		public static void  writeBytes (byte[] bytes)
		{
			Array.Copy (bytes, 0, sendBuf, sendbufIndex, bytes.Length);
			sendbufIndex += (int)bytes.Length;
		}

		public static void writeBytes (byte[] bytes, int len)
		{
			Array.Copy (bytes, 0, sendBuf, sendbufIndex, len);
			sendbufIndex += (int)len;
		}

		public static void writeUshort (ushort s)
		{
			var st = (short)s;
			writeShort (st);
		}

		public static void writeShort (short s)
		{
			sendBuf [sendbufIndex++] = (byte)(s >> 8); // s的低位数据　存储在　sendBuf的　低位地址
			sendBuf [sendbufIndex++] = (byte)(s & 0xff);// s的低位数据 存储在　sendBuf 的高位地址
		}

		public static void writeUint (uint i)
		{
			var uit = (int)i;
			writeInt (uit);
		}

		public static void writeInt (int i)
		{
			sendBuf [sendbufIndex++] = (byte)(i >> 24);
			sendBuf [sendbufIndex++] = (byte)(i >> 16 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(i >> 8 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(i & 0xff);
		}

		public static void writeFloat (float f)
		{
			writeInt ((int)f);
		}

		public static void writeLong (long l)
		{
			sendBuf [sendbufIndex++] = (byte)(l >> 56 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 48 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 40 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 32 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 24 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 16 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l >> 8 & 0xff);
			sendBuf [sendbufIndex++] = (byte)(l & 0xff);
		}

		public static void writeDouble (double d)
		{
			writeLong ((long)d);
		}

		public static void writeString (String str)
		{
			var original = sendbufIndex;
			if (!String.IsNullOrEmpty (str)) {
				utflen = 0;
				idxRec = sendbufIndex;
				sendbufIndex += 2;
				for (i = 0, strlen = str.Length; i < strlen; ++i) {
					c = str [i];
					if (c > 0x00 && c < 0x80) {
						sendBuf [sendbufIndex++] = (byte)c;
						++utflen;
					} else if (c > 0x07FF) {
						sendBuf [sendbufIndex++] = (byte)(0xE0 | c >> 12);
						sendBuf [sendbufIndex++] = (byte)(0x80 | c >> 6 & 0x3F);
						sendBuf [sendbufIndex++] = (byte)(0x80 | c & 0x3F);
						utflen += 3;
					} else {
						sendBuf [sendbufIndex++] = (byte)(0xC0 | c >> 6 & 0x1F);
						sendBuf [sendbufIndex++] = (byte)(0x80 | c & 0x3F);
						utflen += 2;
					}
				}
				sendBuf [idxRec++] = (byte)(utflen >> 8);
				sendBuf [idxRec] = (byte)(utflen & 0xff);
			} else {
				sendBuf [sendbufIndex++] = 0;
				sendBuf [sendbufIndex++] = 0;
			}
			DebugTool.Log ("string:" + str + " 长: " + (sendbufIndex - original));
		}

		public static void writeOPcode (byte[] opcode)
		{
			// 要预留2个字节出来记录Size的
			sendBuf [0] = opcode [0];
			sendBuf [1] = opcode [1];
			// 2字节size 和 2字节opcode
			sendbufIndex = 6;
		}

		public static void writeOPcode (short opcode)
		{
			// 要预留2个字节出来记录Size的
			sendBuf [0] = (byte)(opcode >> 8);
			sendBuf [1] = (byte)(opcode & 0xff);
			// 2字节size 和 2字节opcode
			sendbufIndex = 6;
		}
		//		public static short GetOpcode ()//"WO"
		//		{
		//			//			_i = realReadBuf [bufIndex++] << 8;
		//			//			return (short)(_i + realReadBuf [bufIndex++]);
		//
		//			// 要预留2个字节出来记录Size的
		//			return (short)((sendBuf [0] << 8) + sendBuf [1]);
		//			//			return _i;
		//
		//			// 要预留2个字节出来记录Size的
		//			//			arrBuf [0] = (byte)(this.opcode >> 8);
		//			//			arrBuf [1] = (byte)(this.opcode & 0xff);
		//			// 4字节size 和 2字节opcode
		//			//			sendbufIndex = 6;
		//		}

		/**发送消息前调用此函数，作最后封包，这时候会记录size属性*/
		public static void last ()//写入长度到预留的int位
		{
			sendBuf [2] = (byte)(sendbufIndex >> 24);//sendbufIndex的高位数据存在sendBuf的低位地址
			sendBuf [3] = (byte)(sendbufIndex >> 16 & 0xff);
			sendBuf [4] = (byte)(sendbufIndex >> 8 & 0xff);
			sendBuf [5] = (byte)(sendbufIndex & 0xff);//sendbufIndex的低位数据存放在sendBuf的高位地址
//			writeInt (sendbufIndex);
			sendbufIndex = 0;// 打包完一次 写入 字节序后， 归0 下标

		}
		//----------------读取长度--------------------------
		private static int _msgsize;

		/// <summary>
		/// 读取当前字节序 长度
		/// </summary>
		/// <returns>The send buffer length.</returns>
		public static int GetMsgSize ()
		{
			_msgsize += sendBuf [2] << 24;
			_msgsize += sendBuf [3] << 16;
			_msgsize += sendBuf [4] << 8;
			return _msgsize + sendBuf [5];

//			return (int)(sendBuf [sendbufIndex++] << 24) + (sendBuf [sendbufIndex++] << 16) + (sendBuf [sendbufIndex++] << 8) + sendBuf [sendbufIndex++];
		}

		//		public static short GetMsgSize()
		//        {
		////            _i = realReadBuf[bufIndex] << 8;
		////            return (short)(_i + realReadBuf[bufIndex + 1]);
		//
		//			_i = (realReadBuf[bufIndex++] << 24) + (realReadBuf[bufIndex++] << 16);
		//			_i += (short)( (realReadBuf [bufIndex++] << 8) + realReadBuf [bufIndex++]);
		//			return _i;
		//        }

	}
}

