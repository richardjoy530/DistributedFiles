using Backend.Web.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{

    public interface ICheckInController
    {
        [HttpPost]
        [Route("checkin")]
        ServerCheckinResponse CheckIn(AvailableFiles request);
    }
}