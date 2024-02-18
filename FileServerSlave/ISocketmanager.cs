
namespace FileServerSlave
{
    public interface ISocketManager
    {
        void EstablishConnection(CancellationToken token);
    }
}