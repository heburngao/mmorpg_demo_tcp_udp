using ghbc.Net.Interface;
using proto;
using System;
using System.IO;

namespace ghbc.Net
{
	/// <summary>
	/// S2 c handler.base handler
	/// </summary>
	public class S2C_Handler : IS2C_Handler
	{

		protected Facade_Base facade = Facade_Base.instance;
		//        protected RecvBuf _buffer;
		protected byte[] byts;
		//        public virtual void execute(RecvBuf buffer, short ErrCode)

		public virtual void execute (short cmd, short ErrCode, byte[] payloads)
		{
			//            _buffer = buffer;
			//            byts = buffer.readBytes();
			//			byts = ReadBuffer.readBytes ();
			DebugTool.LogGreen ("S2c_Handler:: cmd: " + cmd);
			byts = payloads;
			if (byts == null) {
				byts = new byte[0];
				DebugTool.LogError ("byts.length is 0, S2c_Handler:: cmd: " + cmd);
			}
//			UnitySocket.canDequeue = true;
		}
		//=========================
		protected void ResetReadIndex ()
		{
			ReadBuffer.ResetIndex ();
		}

		protected int ReadInt ()
		{
			return ReadBuffer.readInt ();
		}

		protected bool ReadBoolean ()
		{
			return ReadBuffer.readBoolean ();
		}

		protected short ReadShort ()
		{
			return ReadBuffer.readShort ();
		}

		protected uint ReadUint ()
		{
			return ReadBuffer.readUint ();
		}

		protected byte[] ReadBytes ()
		{
			return ReadBuffer.readBytes ();
		}

		protected byte[] ReadBytes (int len)
		{
			return ReadBuffer.readBytes (len);
		}

		protected byte ReadByte ()
		{
			return ReadBuffer.readByte ();
		}

		protected sbyte ReadSByte ()
		{
			return ReadBuffer.readSbyte ();
		}

		protected ushort ReadUshort ()
		{
			return ReadBuffer.readUshort ();
		}

		protected long ReadLong ()
		{
			return ReadBuffer.readLong ();
		}

		protected float ReadFloat ()
		{
			return ReadBuffer.readFloat ();
		}

		protected double ReadDouble ()
		{
			return ReadBuffer.readDouble ();
		}

		protected string ReadString ()
		{
			return ReadBuffer.readString ();
		}

		protected void getData ()
		{
			DebugTool.LogYellow ("byts len: " + byts.Length);

			ReadBuffer.StartReadBuffer (byts);
			 
		}
		//=========================

		protected T getData<T> ()
		{
			//using (MemoryStream stream = new MemoryStream(byts))
			//{
			//DebugTool.Log("getData() :: ProtoBuf.Serializer.Deserialize() :: 1");

			//var obj = RuntimeTypeModel.Default.Deserialize(stream, null, typeof(T));

			//T rsp = ProtoBuf.Serializer.Deserialize<T>(stream);

			//------------------------
			T rsp = default(T);
			try {
				rsp = ProtoTool.DeSerialize_mbile<T> (this.byts, ProtoTool.proto);
				//T rsp = ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(stream, ProtoBuf.PrefixStyle.Base128);
				//T rsp = ProtoBuf.Serializer.NonGeneric.Deserialize(typeof(T),stream);
				//DebugTool.Log("getData() :: ProtoBuf.Serializer.Deserialize() :: 2");
				//DebugTool.LogYellow ("<<< 接收 pb : " + rsp.GetType ().FullName);
				return rsp;
			} catch (Exception e) {
				DebugTool.LogError ("DeSerialize_mbile:: " + e.Message);
			}
			//catch (EndOfStreamException e)
			//{
			//    DebugTool.LogError("EndOfStream Error" + e.Message);
			//    //Nothing new on the stream
			//}
			//catch (ObjectDisposedException e)
			//{
			//    DebugTool.LogError("ObjectDisposed Error" + e.Message);
			//}
			//catch (IOException e)
			//{
			//    DebugTool.LogError("IOError" + e.Message);
			//}

			return rsp;//default(T);
			//------------------------

			//}// end using stream
            

		}

	}


	public class ProtoTool
	{

		//public static byte[] Serialize<T>(T rqst)
		//{
		//    var strm = new MemoryStream();
		//    ProtoBuf.Serializer.Serialize<T>(strm, rqst);
		//    return strm.ToArray();
		//}
		//public static T DeSerialize<T>(byte[] btyes)
		//{
		//    using (var strm = new MemoryStream(btyes))
		//    {
		//        return ProtoBuf.Serializer.Deserialize<T>(strm);
		//    }
		//}

		private static ProtoSerializer _proto;

		public static ProtoSerializer proto {
			get {
				if (_proto == null) {
					_proto = new ProtoSerializer ();
				}
				return _proto;
			}
		}
		//-------------------- for iOS below ------------------

		public static byte[] Serialize_mobile<T> (T obj, ProtoBuf.Meta.TypeModel ser)
		{
			//ProtoSerializer ser = new ProtoSerializer();
			byte[] buffer = null;
			using (MemoryStream strm = new MemoryStream ()) {
				ser.Serialize (strm, obj);
				strm.Position = 0;

				int len = (int)strm.Length;
				buffer = new byte[len];
				strm.Read (buffer, 0, len);
//				DebugTool.LogRed ("serialize len: " + len);
				//byte[] bytes = strm.ToArray();
				return buffer;
			}
		}

		public static T DeSerialize_mbile<T> (byte[] buffer, ProtoBuf.Meta.TypeModel ser)
		{

			using (MemoryStream m = new MemoryStream (buffer)) {
				T obj = (T)ser.Deserialize (m, null, typeof(T));
				//			Debug.Log (obj.child.name);
				return obj;
			}
		}
	}
}