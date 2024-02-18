using FileServerMaster.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FileServerMaster.Web.Controllers
{

    public interface ICheckInController
    {
        [HttpPost]
        [Route("checkin")]
        ServerCheckinResponse CheckIn(AvailableFiles request);
    }
}