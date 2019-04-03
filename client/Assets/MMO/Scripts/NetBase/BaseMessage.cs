
namespace ghbc.Net
{
	public class BaseMessage
	{
		public short opcode;
		//(ushort)"WO" + (uint)size + (ushort) cmd
		public int msgSize;

		public short cmd;

		/// <summary>
		/// 获取数据比特流
		/// </summary>
		/// <returns></returns>
		public virtual void WriteMSG ()
		{
//			SendBuf buf = BufferManager.getSendBuffer(opcode, true).writeUshort(cmd).writeBytes(bytes_strm).last();
			SendBuffer.writeShort (opcode);//"WO"
			SendBuffer.writeInt (msgSize);//uint
			SendBuffer.writeShort (cmd);
//			SendBuffer.sendbufIndex = 6;
		}

		public virtual void WriteAll ()
		{
			WriteMSG ();
			///
			WriteMSGSize ();
		}

		public void WriteMSGSize ()
		{
//			msgSize = SendBuffer.sendbufIndex;
//			SendBuffer.sendbufIndex = 0; 
//			SendBuffer.writeUint (msgSize);
			SendBuffer.last ();
		}

		/// <summary>
		/// 设置由比特流而得到的数据
		/// </summary>
		/// <param name="data"></param>
		public virtual void Read ()
		{
			opcode = ReadBuffer.readShort ();
			msgSize = ReadBuffer.readInt ();
			cmd = ReadBuffer.readShort ();
		}
	}
}
