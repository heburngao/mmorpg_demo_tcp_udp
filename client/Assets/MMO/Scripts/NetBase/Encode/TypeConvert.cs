//namespace ghbGame.Net
//{
using System;

public class TypeConvert
{
		
	public TypeConvert ()
	{  
	}

	public  static   byte[] getBytes (float s, bool asc)
	{  
		int buf = (int)(s * 100);  
		return getBytes (buf, asc);  
	}

	/// <summary>
	/// asc:true 大端, false 小端
	/// </summary>
	/// <param name="s"></param>
	/// <param name="asc"></param>
	/// <returns></returns>
	public static   byte[] getBytes (byte s, bool asc)
	{  
		byte[] buf = new   byte[1];  
		if (asc) {  
			for (int i = buf.Length - 1; i >= 0; i--) {  
				buf [i] = (byte)(s & 0x00ff);  
				s >>= 8;  
			}  
		} else {  
			for (int i = 0; i < buf.Length; i++) {  
				buf [i] = (byte)(s & 0x00ff);  
				s >>= 8;  
			}  
		}  
		return buf;  
	}

	public static   byte[] getBytes (short s, bool asc)
	{  
		byte[] buf = new   byte[2];  
		if (asc) {  
			for (int i = buf.Length - 1; i >= 0; i--) {  
				buf [i] = (byte)(s & 0x00ff);  
				s >>= 8;  
			}  
		} else {  
			for (int i = 0; i < buf.Length; i++) {  
					
				buf [i] = (byte)(s & 0x00ff);  
				s >>= 8;  
			}  
		}  
		return buf;  
	}

	public static   byte[] getBytes (int s, bool asc)
	{  
		byte[] buf = new   byte[4];  
		if (asc)
			for (int i = buf.Length - 1; i >= 0; i--) {  
				buf [i] = (byte)(s & 0x000000ff);  
				s >>= 8;  
			}
		else
			for (int i = 0; i < buf.Length; i++) {  
				buf [i] = (byte)(s & 0x000000ff);  
				s >>= 8;  
			}  
		return buf;  
	}

	public static   byte[] getBytes (long s, bool asc)
	{  
		byte[] buf = new   byte[8];  
		if (asc)
			for (int i = buf.Length - 1; i >= 0; i--) {  
				buf [i] = (byte)(s & 0x00000000000000ff);  
				s >>= 8;  
			}
		else
			for (int i = 0; i < buf.Length; i++) {  
				buf [i] = (byte)(s & 0x00000000000000ff);  
				s >>= 8;  
			}  
		return buf;  
	}
	//===================================================
	public static float getFloat (byte[] buf, bool asc)
	{  
		int i = getInt (buf, asc);  
		float s = (float)i;  
		return s / 100;  
	}

	public static short getShort (byte[] buf, bool asc)
	{  
		if (buf == null) {  
//				throw new IllegalArgumentException("  byte array is null!");  
		}  
		if (buf.Length > 2) {  
			//throw new IllegalArgumentException("  byte array size > 2 !");  
		}  
		short r = 0;  
		if (!asc)
			for (int i = buf.Length - 1; i >= 0; i--) {  
				r <<= 8;
				r |= (short)(buf [i] & 0x00ff);
			}
		else
			for (int i = 0; i < buf.Length; i++) {  
				r <<= 8;
				r |= (short)(buf [i] & 0x00ff);
			}  
		return r;
	}

	public static int getInt (byte[] buf, bool asc)
	{  
		if (buf == null) {
			// throw new IllegalArgumentException("  byte array is null!");  
		}  
		if (buf.Length > 4) {
			//throw new IllegalArgumentException("  byte array size > 4 !");  
		}  
		int r = 0;
		if (!asc)
			for (int i = buf.Length - 1; i >= 0; i--) {  
				r <<= 8;
				r |= (buf [i] & 0x000000ff);
			}
		else
			for (int i = 0; i < buf.Length; i++) {  
				r <<= 8;
				r |= (buf [i] & 0x000000ff);
			}  
		return r;  
	}

	public static long getLong (byte[] buf, bool asc)
	{  
		if (buf == null) {  
			//throw new IllegalArgumentException("  byte array is null!");  
		}  
		if (buf.Length > 8) {  
			//throw new IllegalArgumentException("  byte array size > 8 !");  
		}  
		long r = 0;  
		if (!asc)//bigendian
			for (int i = buf.Length - 1; i >= 0; i--) {  
				r <<= 8;
				r |= (buf [i] & 0x00000000000000ff);
			}
		else
			for (int i = 0; i < buf.Length; i++) {  
				r <<= 8;
				r |= (buf [i] & 0x00000000000000ff);
			}  
		return r;  
	}



	/// <summary>
	/// Byts to sbyte.
	/// </summary>
	/// <returns>The to sbyte.</returns>
	/// <param name="myByte">My byte.</param>
	public static sbyte[] bytToSbyte (byte[] myByte)
	{
		sbyte[] mySByte = Array.ConvertAll (myByte, (x) => (sbyte)x);
			 
		//另一种实现方式::
		//		sbyte[] mySByte = new sbyte[myByte.Length];
		//		for (int i = 0; i < myByte.Length; i++) {
		//			if (myByte [i] > 127)
		//				mySByte [i] = (sbyte)(myByte [i] - 256);
		//			else
		//				mySByte [i] = (sbyte)myByte [i];
		//		}
		//		 
		return mySByte;
	}

	public static byte[] sbytToByte (sbyte[] mySByte)
	{
		byte[] myByte = Array.ConvertAll (mySByte, (x) => (byte)x);
		return myByte;
	}

	#region 对int的字节序进行大/小端翻转====================================================

	//翻转byte数组
	public static void ReverseBytes (byte[] bytes)
	{
		byte tmp;
		int len = bytes.Length;

		for (int i = 0; i < len / 2; i++) {
			tmp = bytes [len - 1 - i];
			bytes [len - 1 - i] = bytes [i];
			bytes [i] = tmp;
		}
	}

	//规定转换起始位置和长度
	public static void ReverseBytes (byte[] bytes, int start, int len)
	{
		int end = start + len - 1;
		byte tmp;
		int i = 0;
		for (int index = start; index < start + len / 2; index++, i++) {
			tmp = bytes [end - i];
			bytes [end - i] = bytes [index];
			bytes [index] = tmp;
		}
	}

	// reverse byte order (16-bit)
	public static UInt16 ReverseBytes (UInt16 value)
	{
		return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
	}
	// reverse byte order (32-bit)
	public static UInt32 ReverseBytes (UInt32 value)
	{
		return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
		(value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
	}
	// reverse byte order (64-bit)
	public static UInt64 ReverseBytes (UInt64 value)
	{
		return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
		(value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
		(value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
		(value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
	}

	#endregion
}
//}