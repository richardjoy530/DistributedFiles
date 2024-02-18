
namespace FileDistributor
{
    public interface ISocketmanager
    {
        void EstablishConnection(CancellationToken token);
    }
}