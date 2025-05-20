namespace Common.com.networking
{
    public interface IPacketListener
    {
        public void HandlePacket(byte[] content,Connection connection);
        public void OnDisconnect(Connection c);
    }
}