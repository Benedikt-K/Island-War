using Common.com.networking;

namespace Server.net.networking
{
    public interface IConnectionListener
    {
        /// <summary>
        /// Invoked every time a tcp connection is requested
        /// </summary>
        /// <param name="c">The new Connection</param>
        /// <returns>if the connection is to be ignored</returns>
        public void OnConnection(Connection c);
    }
}