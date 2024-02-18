namespace FileServerSlave.Web.Controllers
{
    public interface IFileController
    {
        byte[] DownLoadFile(string filename);
    }
}