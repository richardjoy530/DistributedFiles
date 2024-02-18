﻿using FileServerMaster.Web.Controllers;
using FileServerSlave.Events;
using FileServerSlave.Interceptor;

namespace FileServerSlave.EventHandlers
{
    public class CheckInEventHandler : IEventHandler
    {
        private readonly ILogger<CheckInEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICheckInController _checkInController;

        public CheckInEventHandler(ILogger<CheckInEventHandler> logger, IEventDispatcher eventDispatcher)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;

            _checkInController = ApiInterceptor.GetController<ICheckInController>(new HostString("192.168.18.87:7180"), false);// todo configuraiton manager
        }

        public void HandleEvent(EventBase e)
        {
            var resp = _checkInController.CheckIn(new FileServerMaster.Web.Contracts.AvailableFiles { AvailableFileNames = [], HostString = "192.168.18.87:7180" });
            _logger.LogInformation("Handled checkin event");
        }
    }
}