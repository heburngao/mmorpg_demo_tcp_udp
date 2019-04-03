//using ghbGame.Net;
using System;
using System.Text;
namespace ghbc
{
    public class MD5
    {

        private static int S11 = 7;
        private static int S12 = 12;
        private static int S13 = 17;
        private static int S14 = 22;

        private static int S21 = 5;
        private static int S22 = 9;
        private static int S23 = 14;
        private static int S24 = 20;

        private static int S31 = 4;
        private static int S32 = 11;
        private static int S33 = 16;
        private static int S34 = 23;

        private static int S41 = 6;
        private static int S42 = 10;
        private static int S43 = 15;
        private static int S44 = 21;

        static sbyte[] PADDING = new sbyte[] { -128, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static long[] state = new long[4];
        private static long[] count = new long[2];
        private static sbyte[] buffer = new sbyte[64];

        private static sbyte[] digest = new sbyte[16];


        private static void md5Init()
        {
            count[0] = 0L;
            count[1] = 0L;

            state[0] = 0x67452301L;
            state[1] = 0xefcdab89L;
            state[2] = 0x98badcfeL;
            state[3] = 0x10325476L;
        }

        private static void md5Memcpy(sbyte[] output, sbyte[] input, int outpos, int inpos, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                output[outpos + i] = input[inpos + i];
            }
        }

        private static long b2iu(sbyte b)
        {
            return b < 0 ? b & 0x7F + 128 : b;
        }

        private static void Decode(long[] output, sbyte[] input, int len)
        {
            for (int i = 0, j = 0; j < len; i++, j += 4)
            {
                output[i] = b2iu(input[j]) | (b2iu(input[j + 1]) << 8) | (b2iu(input[j + 2]) << 16) | (b2iu(input[j + 3]) << 24);
            }
        }

        private static long F(long x, long y, long z)
        {
            return (x & y) | ((~x) & z);
        }

        private static long G(long x, long y, long z)
        {
            return (x & z) | (y & (~z));
        }

        private static long H(long x, long y, long z)
        {
            return x ^ y ^ z;
        }

        private static long I(long x, long y, long z)
        {
            return y ^ (x | (~z));
        }

        private static long FF(long a, long b, long c, long d, long x, long s, long ac)
        {
            a += F(b, c, d) + x + ac;
            //		a = ((int)a << s) | ((int)a >> (32 - s));
            var pos = (int)((long)32 - s);
            a = ((int)a << (int)s) | (moveRight((int)a, pos));
            a += b;
            return a;
        }

        private static long GG(long a, long b, long c, long d, long x, long s, long ac)
        {
            a += G(b, c, d) + x + ac;
            //		a = ((int)a << s) | ((int)a >> (32 - s));
            a = ((int)a << (int)s) | (moveRight((int)a, (32 - (int)s)));
            a += b;
            return a;
        }

        private static long HH(long a, long b, long c, long d, long x, long s, long ac)
        {
            a += H(b, c, d) + x + ac;
            //		a = ((int)a << s) | ((int)a >> (32 - s));
            a = ((int)a << (int)s) | (moveRight((int)a, (32 - (int)s)));
            a += b;
            return a;
        }

        private static long II(long a, long b, long c, long d, long x, long s, long ac)
        {
            a += I(b, c, d) + x + ac;
            //		a = ((int)a << s) | ((int)a >> (32 - s));
            a = ((int)a << (int)s) | (moveRight((int)a, (32 - (int)s)));
            a += b;
            return a;
        }

        private static void md5Transform(sbyte[] block)
        {
            long a = state[0], b = state[1], c = state[2], d = state[3];
            long[] x = new long[16];

            Decode(x, block, 64);

            /* Round 1 */
            a = FF(a, b, c, d, x[0], S11, 0xd76aa478L); /* 1 */
            d = FF(d, a, b, c, x[1], S12, 0xe8c7b756L); /* 2 */
            c = FF(c, d, a, b, x[2], S13, 0x242070dbL); /* 3 */
            b = FF(b, c, d, a, x[3], S14, 0xc1bdceeeL); /* 4 */
            a = FF(a, b, c, d, x[4], S11, 0xf57c0fafL); /* 5 */
            d = FF(d, a, b, c, x[5], S12, 0x4787c62aL); /* 6 */
            c = FF(c, d, a, b, x[6], S13, 0xa8304613L); /* 7 */
            b = FF(b, c, d, a, x[7], S14, 0xfd469501L); /* 8 */
            a = FF(a, b, c, d, x[8], S11, 0x698098d8L); /* 9 */
            d = FF(d, a, b, c, x[9], S12, 0x8b44f7afL); /* 10 */
            c = FF(c, d, a, b, x[10], S13, 0xffff5bb1L); /* 11 */
            b = FF(b, c, d, a, x[11], S14, 0x895cd7beL); /* 12 */
            a = FF(a, b, c, d, x[12], S11, 0x6b901122L); /* 13 */
            d = FF(d, a, b, c, x[13], S12, 0xfd987193L); /* 14 */
            c = FF(c, d, a, b, x[14], S13, 0xa679438eL); /* 15 */
            b = FF(b, c, d, a, x[15], S14, 0x49b40821L); /* 16 */

            /* Round 2 */
            a = GG(a, b, c, d, x[1], S21, 0xf61e2562L); /* 17 */
            d = GG(d, a, b, c, x[6], S22, 0xc040b340L); /* 18 */
            c = GG(c, d, a, b, x[11], S23, 0x265e5a51L); /* 19 */
            b = GG(b, c, d, a, x[0], S24, 0xe9b6c7aaL); /* 20 */
            a = GG(a, b, c, d, x[5], S21, 0xd62f105dL); /* 21 */
            d = GG(d, a, b, c, x[10], S22, 0x2441453L); /* 22 */
            c = GG(c, d, a, b, x[15], S23, 0xd8a1e681L); /* 23 */
            b = GG(b, c, d, a, x[4], S24, 0xe7d3fbc8L); /* 24 */
            a = GG(a, b, c, d, x[9], S21, 0x21e1cde6L); /* 25 */
            d = GG(d, a, b, c, x[14], S22, 0xc33707d6L); /* 26 */
            c = GG(c, d, a, b, x[3], S23, 0xf4d50d87L); /* 27 */
            b = GG(b, c, d, a, x[8], S24, 0x455a14edL); /* 28 */
            a = GG(a, b, c, d, x[13], S21, 0xa9e3e905L); /* 29 */
            d = GG(d, a, b, c, x[2], S22, 0xfcefa3f8L); /* 30 */
            c = GG(c, d, a, b, x[7], S23, 0x676f02d9L); /* 31 */
            b = GG(b, c, d, a, x[12], S24, 0x8d2a4c8aL); /* 32 */

            /* Round 3 */
            a = HH(a, b, c, d, x[5], S31, 0xfffa3942L); /* 33 */
            d = HH(d, a, b, c, x[8], S32, 0x8771f681L); /* 34 */
            c = HH(c, d, a, b, x[11], S33, 0x6d9d6122L); /* 35 */
            b = HH(b, c, d, a, x[14], S34, 0xfde5380cL); /* 36 */
            a = HH(a, b, c, d, x[1], S31, 0xa4beea44L); /* 37 */
            d = HH(d, a, b, c, x[4], S32, 0x4bdecfa9L); /* 38 */
            c = HH(c, d, a, b, x[7], S33, 0xf6bb4b60L); /* 39 */
            b = HH(b, c, d, a, x[10], S34, 0xbebfbc70L); /* 40 */
            a = HH(a, b, c, d, x[13], S31, 0x289b7ec6L); /* 41 */
            d = HH(d, a, b, c, x[0], S32, 0xeaa127faL); /* 42 */
            c = HH(c, d, a, b, x[3], S33, 0xd4ef3085L); /* 43 */
            b = HH(b, c, d, a, x[6], S34, 0x4881d05L); /* 44 */
            a = HH(a, b, c, d, x[9], S31, 0xd9d4d039L); /* 45 */
            d = HH(d, a, b, c, x[12], S32, 0xe6db99e5L); /* 46 */
            c = HH(c, d, a, b, x[15], S33, 0x1fa27cf8L); /* 47 */
            b = HH(b, c, d, a, x[2], S34, 0xc4ac5665L); /* 48 */

            /* Round 4 */
            a = II(a, b, c, d, x[0], S41, 0xf4292244L); /* 49 */
            d = II(d, a, b, c, x[7], S42, 0x432aff97L); /* 50 */
            c = II(c, d, a, b, x[14], S43, 0xab9423a7L); /* 51 */
            b = II(b, c, d, a, x[5], S44, 0xfc93a039L); /* 52 */
            a = II(a, b, c, d, x[12], S41, 0x655b59c3L); /* 53 */
            d = II(d, a, b, c, x[3], S42, 0x8f0ccc92L); /* 54 */
            c = II(c, d, a, b, x[10], S43, 0xffeff47dL); /* 55 */
            b = II(b, c, d, a, x[1], S44, 0x85845dd1L); /* 56 */
            a = II(a, b, c, d, x[8], S41, 0x6fa87e4fL); /* 57 */
            d = II(d, a, b, c, x[15], S42, 0xfe2ce6e0L); /* 58 */
            c = II(c, d, a, b, x[6], S43, 0xa3014314L); /* 59 */
            b = II(b, c, d, a, x[13], S44, 0x4e0811a1L); /* 60 */
            a = II(a, b, c, d, x[4], S41, 0xf7537e82L); /* 61 */
            d = II(d, a, b, c, x[11], S42, 0xbd3af235L); /* 62 */
            c = II(c, d, a, b, x[2], S43, 0x2ad7d2bbL); /* 63 */
            b = II(b, c, d, a, x[9], S44, 0xeb86d391L); /* 64 */

            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        private static void md5Update(sbyte[] inbuf, int inputLen)
        {
            int i, index, partLen;
            sbyte[] block = new sbyte[64];
            //		index = (int)(count [0] >> 3) & 0x3F;
            index = moveRight((int)count[0], 3) & 0x3F;

            if ((count[0] += (inputLen << 3)) < (inputLen << 3))
                count[1]++;
            //		count [1] += (inputLen >> 29);
            count[1] += moveRight(inputLen, 29);

            partLen = 64 - index;

            if (inputLen >= partLen)
            {
                md5Memcpy(buffer, inbuf, index, 0, partLen);
                md5Transform(buffer);

                for (i = partLen; i + 63 < inputLen; i += 64)
                {
                    md5Memcpy(block, inbuf, 0, i, 64);
                    md5Transform(block);
                }
                index = 0;

            }
            else
            {
                i = 0;
            }

            md5Memcpy(buffer, inbuf, index, i, inputLen - i);
        }

        private static void Encode(sbyte[] output, long[] input, int len)
        {
            for (int i = 0, j = 0; j < len; i++, j += 4)
            {
                output[j] = (sbyte)(input[i] & 0xffL);
                //			output [j + 1] = (  sbyte)((input [i] >> 8) & 0xffL);
                //			output [j + 2] = (  sbyte)((input [i] >> 16) & 0xffL);
                //			output [j + 3] = (  sbyte)((input [i] >> 24) & 0xffL);
                output[j + 1] = (sbyte)(moveRight((int)input[i], 8) & 0xffL);
                output[j + 2] = (sbyte)(moveRight((int)input[i], 16) & 0xffL);
                output[j + 3] = (sbyte)(moveRight((int)input[i], 24) & 0xffL);
            }
        }

        private static void md5Final()
        {
            sbyte[] bits = new sbyte[8];
            int index, padLen;

            Encode(bits, count, 8);

            //		index = (int)(count [0] >> 3) & 0x3f;
            index = (int)(moveRight((int)count[0], 3) & 0x3f);
            padLen = (index < 56) ? (56 - index) : (120 - index);
            md5Update(PADDING, padLen);

            md5Update(bits, 8);

            Encode(digest, state, 16);
        }

        private static String byteHEX(sbyte ib)
        {
            char[] Digit = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            char[] ob = new char[2];
            //		ob [0] = Digit [(ib >> 4) & 0X0F];
            ob[0] = Digit[moveRight((int)ib, 4) & 0x0F];
            ob[1] = Digit[ib & 0X0F];
            return new String(ob);
        }

        public static String getMD5ofStr(String inbuf)
        {
            md5Init();
            sbyte[] strInbuf = TypeConvert.bytToSbyte(Encoding.UTF8.GetBytes(inbuf));
            md5Update(strInbuf, inbuf.Length);
            //        md5Update(inbuf.getBytes(), inbuf.length()); //JAVA VERSION
            md5Final();
            String digestHexStr = "";
            for (int i = 0; i < 16; ++i)
            {
                digestHexStr += byteHEX(digest[i]);
            }
            return digestHexStr;
        }
        // 	/// <summary>
        // 	/// Byts to sbyte.
        // 	/// </summary>
        // 	/// <returns>The to sbyte.</returns>
        // 	/// <param name="myByte">My byte.</param>
        // 	public static sbyte[] bytToSbyte (byte[] myByte)
        // 	{
        // 		sbyte[] mySByte = Array.ConvertAll (myByte, (x) => (sbyte)x);

        // 		//另一种实现方式::
        // //		sbyte[] mySByte = new sbyte[myByte.Length];
        // //		for (int i = 0; i < myByte.Length; i++) {
        // //			if (myByte [i] > 127)
        // //				mySByte [i] = (sbyte)(myByte [i] - 256);
        // //			else
        // //				mySByte [i] = (sbyte)myByte [i];
        // //		}
        // //		 
        // 		return mySByte;
        // 	}
        // 	public static byte[] sbytToByte (sbyte[] mySByte)
        // 	{
        // 		byte[] myByte = Array.ConvertAll (mySByte, (x) => (byte)x);
        // 		return myByte;
        // 	}
        // <summary>
        /// 特殊的 右移位操作 ，补0右移，如果是负数，需要进行特殊的转换处理（右移位）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static int moveRight(int value, int pos)
        {
            //		if (value < 0) {
            //			string s = Convert.ToString (value, 2);    // 转换为二进制
            //			for (int i = 0; i < pos; i++) {
            //				s = "0" + s.Substring (0, 31);
            //			}
            //			return Convert.ToInt32 (s, 2);            // 将二进制数字转换为数字
            //		} else {
            //			return value >> pos;
            //		}
            //另一种实现方式
            int mask = 0x7fffffff; //Integer.MAX_VALUE
            for (int i = 0; i < pos; i++)
            {
                value >>= 1;
                value &= mask;
            }
            return value;
        }
    }
}