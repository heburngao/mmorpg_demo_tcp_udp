using System;

namespace ghbc.Net
{
	/// <summary>
	/// Read buffer，只是单纯读取 字节序， 无其他粘包操作
	/// </summary>
	public class ReadBuffer_Dynamic
	{
		private static  int BUF_MAX = 1024;
		/// <summary>
		/// 当前读取操作的 包片
		/// </summary>
		private   byte[] arrBuf;
		/// <summary>
		/// 字符读取-源-缓存带
		/// </summary>
		private   byte[] byteArr = new byte[BUF_MAX];
		/// <summary>
		/// 字符读取缓存带
		/// </summary>
		private   char[] charArr = new char[BUF_MAX];
		/// <summary>
		/// 读取操作 包片 的下标
		/// </summary>
		private   short bufIndex;
		private   int _i;
		private   long _l;
		/// <summary>
		/// long 读取 缓存带
		/// </summary>
		private   long[] lArr = new long[8];

		public   void StartReadBuffer (byte[] _arrBuf)
		{
			arrBuf = _arrBuf;
			bufIndex = 0;
			//			bufIndex = 6;

		}

		public   void printBytes (int len)
		{
			 
//			DebugTool.LogRed ("len: " + len);
			int i = 0;
			try {

				for (i = 0; i < len; i++) {//sendBuf.Length; i++)
					if (i < arrBuf.Length)
						DebugTool.LogGreen ("index:" + i + "/" + arrBuf [i].ToString ("x2") + "/" + arrBuf [i].ToString ());

				}
			} catch (Exception ex) {
				DebugTool.LogError (ex.Message);
			}
		}
		//		public   byte[] StartReadBuffer
		//		{
		//			set{
		//				arrBuf = value;
		//				bufIndex = 0;
		//			}
		//			//			bufIndex = 6;
		//		}
		public   void ResetIndex ()
		{
			bufIndex = 0;
		}

		public   bool readBoolean ()
		{
			return arrBuf [bufIndex++] != 0;
		}

		public   byte[] readBytes ()
		{
			var len = arrBuf.Length - bufIndex;//msgBodySize - bufIndex;
			byte[] result = new byte[len];
			Array.Copy (arrBuf, bufIndex, result, 0, result.Length);
			bufIndex += (short)result.Length;
			return result;
		}

		public   byte[] readBytes (int len)
		{
			byte[] result = new byte[len];
			Array.Copy (arrBuf, bufIndex, result, 0, result.Length);
			bufIndex += (short)result.Length;
			return result;
		}

		public   byte readByte ()
		{
			return arrBuf [bufIndex++];
		}

		public   sbyte readSbyte ()
		{
			return (sbyte)arrBuf [bufIndex++];
		}

		public   ushort readUshort ()
		{
			return (ushort)readShort ();
		}

		private   int _st;

		public   short readShort ()
		{
			try {
				_st = arrBuf [bufIndex++] << 8;
			} catch (Exception e) {
				DebugTool.LogError (e.Message);
			}
			return (short)(_st + arrBuf [bufIndex++]);
		}

		public   uint readUint ()
		{
			return (uint)readInt ();
		}

		private   int _int;

		public   int readInt ()
		{
			try {
				_int = arrBuf [bufIndex++] << 24;
			} catch (Exception e) {
				DebugTool.LogError (e.Message);
			}

			try {
				_int += arrBuf [bufIndex++] << 16;
			} catch (Exception e) {
				DebugTool.LogError (e.Message);
			}
			try {
				_int += arrBuf [bufIndex++] << 8;

			} catch (Exception e) {
				DebugTool.LogError (e.Message);
			}
			return _int + arrBuf [bufIndex++];
		}

		public   float readFloat ()
		{
			return readInt ();
		}

		public   long readLong ()
		{
			lArr [0] = arrBuf [bufIndex++];
			lArr [1] = arrBuf [bufIndex++];
			lArr [2] = arrBuf [bufIndex++];
			lArr [3] = arrBuf [bufIndex++];
			lArr [4] = arrBuf [bufIndex++];
			lArr [5] = arrBuf [bufIndex++];
			lArr [6] = arrBuf [bufIndex++];
			lArr [7] = arrBuf [bufIndex++];
			_l = lArr [0] << 56;
			_l += lArr [1] << 48;
			_l += lArr [2] << 40;
			_l += lArr [3] << 32;
			_l += lArr [4] << 24;
			_l += lArr [5] << 16;
			_l += lArr [6] << 8;
			return _l + lArr [7];
		}

		public   double readDouble ()
		{
			return (double)ReadBuffer.readInt () + (double)ReadBuffer.readShort () / 1000;
		}

		/// <summary>
		/// 如果解读其他服务器的字符串，需先定义一short的字符串长度，否则不可用
		/// </summary>
		/// <returns>The string.</returns>
		public   String readString ()
		{
			_i = readShort ();//如果解读其他服务器的字符串，需先定义一short的字符串长度，否则不可用
			DebugTool.LogYellow ("A string,长  : " + _i);//charArrCount);
			//========= 方法一
//			var arr = new byte[_i];
//			Array.Copy (arrBuf, bufIndex, arr, 0, _i);
//			bufIndex += (short)_i;
//			foreach (var item in arr) {
//				DebugTool.LogCyan (item);
//			}
//			return System.Text.Encoding.UTF8.GetString (arr);
			//========= 方法二
			if (_i != 0) {
				int i = 0;
				do {
					byteArr [i++] = arrBuf [bufIndex++];
				} while (i != _i);
				int charArrCount = 0, c, b;
				byte b1, b2;
				i = 0;
				do {
					c = byteArr [i++] & 0xFF;
					b = c >> 4;
					if (b >= 0x00 && b < 0x08) {/* 0xxxxxxx */
						charArr [charArrCount++] = (char)c;
					} else if (b == 0x0C || b == 0x0D) {/* 110x xxxx 10xx xxxx */
						/*
                      * if (++byteArrCount > utflen) { throw new
                      * UTFDataFormatException
                      * ("malformed input: partial character at end"); }
                      */
						b1 = byteArr [i++];
						/*
                         * if ((b1 & 0xC0) != 0x80) { throw new
                         * UTFDataFormatException("malformed input around byte " +
                         * byteArrCount); }
                         */
						charArr [charArrCount++] = (char)((c & 0x1F) << 6 | b1 & 0x3F);
					} else if (b == 0x0E) {/* 1110 xxxx 10xx xxxx 10xx xxxx */
						/*
                      * if ((byteArrCount += 2) > utflen) { throw new
                      * UTFDataFormatException
                      * ("malformed input: partial character at end"); }
                      */
						b1 = byteArr [i++];
						b2 = byteArr [i++];
						/*
                         * if ((b1 & 0xC0) != 0x80 || (b2 & 0xC0) != 0x80) { throw
                         * new UTFDataFormatException("malformed input around byte "
                         * + (byteArrCount - 1)); }
                         */
						charArr [charArrCount++] = (char)((c & 0x0F) << 12 | (b1 & 0x3F) << 6 | b2 & 0x3F);
					} //else {/* 10xx xxxx, 1111 xxxx */
					//try {
					//throw new UTFDataFormatException("malformed input around byte " + byteArrCount);
					//} catch (UTFDataFormatException e) {
					//e.printStackTrace();
					//}
					//}
				} while (i != _i);
				// The number of chars produced may be less than utflen
				DebugTool.LogYellow ("B string,长  : " + charArrCount);
				return new String (charArr, 0, charArrCount);
			}
			return "";
		}

		//----------------读取长度--------------------------
		public   short GetOpcode ()//"WO"
		{
			//bufIndex = 0;
			// 要预留2个字节出来记录Size的
			//			return (short)((arrBuf [0] << 8) + arrBuf [1]);
			return (short)((arrBuf [bufIndex++] << 8) + arrBuf [bufIndex++]);
		}
		//----------------读取长度--------------------------
		private   int _msgsize;

		/// <summary>
		/// 读取当前字节序 长度
		/// </summary>
		/// <returns>The send buffer length.</returns>
		public   int GetMsgSize ()
		{
			//			_msgsize += arrBuf [2] << 24;
			//			_msgsize += arrBuf [3] << 16;
			//			_msgsize += arrBuf [4] << 8;
			//			return _msgsize + arrBuf [5];
			//return (int)((arrBuf[2] << 24) | (arrBuf[3] << 16) | (arrBuf[4] << 8) | arrBuf[5]);// 貌　似不能用这样的下标

			//return (int)((arrBuf[bufIndex++] << 24) | (arrBuf[bufIndex++] << 16) | (arrBuf[bufIndex++] << 8) | arrBuf[bufIndex++]);
			//return (int)((arrBuf[bufIndex++] << 24) + (arrBuf[bufIndex++] << 16) + (arrBuf[bufIndex++] << 8) + arrBuf[bufIndex++]);//original
			return (int)((arrBuf [bufIndex++] << 24) | (arrBuf [bufIndex++] << 16) | (arrBuf [bufIndex++] << 8) | arrBuf [bufIndex++]);
		}

		private   int _msgsize2;

		public   int GetMsgSize2 ()
		{
			_msgsize2 += arrBuf [2] << 24;
			_msgsize2 += arrBuf [3] << 16;
			_msgsize2 += arrBuf [4] << 8;
			return _msgsize2 + arrBuf [5];
		}

	}


}
