namespace Backend.Storage
{
    public interface IFileQueueContainer
    {
        void EnQueue(IFormFile formFile);

        IFormFile? DeQueue();
    }
}
