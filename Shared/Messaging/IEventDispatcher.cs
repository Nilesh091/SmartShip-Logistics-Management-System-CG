using System;

namespace Shared.Messaging
{
    public interface IEventDispatcher
    {
        Task Dispatch(string eventName, string message);
    }
}
