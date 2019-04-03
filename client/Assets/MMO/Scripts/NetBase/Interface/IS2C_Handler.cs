namespace ghbc.Net.Interface
{
    public interface IS2C_Handler
    {
		void execute(short cmd, short ErrCode, byte[] payloads);
//		void execute(int cmd, ushort ret);
    }
}
