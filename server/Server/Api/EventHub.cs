using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Serilog;

namespace Server.Api
{
    /// <summary>
    /// A signalR hub that provides channel-based event broadcasting
    /// that clients can subscribe to
    /// </summary>
    public class EventHub : Hub
    {
        public async Task Subscribe(string channel)
        {
            await Groups.Add(Context.ConnectionId, channel);

            var ev = new ChannelEvent
            {
                ChannelName = Constants.AdminChannel,
                Name = "user.subscribed",
                Data = new
                {
                    Context.ConnectionId,
                    ChannelName = channel
                }
            };

            await Publish(ev);
        }

        public async Task Unsubscribe(string channel)
        {
            await Groups.Remove(Context.ConnectionId, channel);

            var ev = new ChannelEvent
            {
                ChannelName = Constants.AdminChannel,
                Name = "user.unsubscribed",
                Data = new
                {
                    Context.ConnectionId,
                    ChannelName = channel
                }
            };

            await Publish(ev);
        }


        public Task Publish(ChannelEvent channelEvent)
        {
            Clients.Group(channelEvent.ChannelName).OnEvent(channelEvent.ChannelName, channelEvent);

            if (channelEvent.ChannelName != Constants.AdminChannel)
            {
                // Push this out on the admin channel
                //
                Clients.Group(Constants.AdminChannel).OnEvent(Constants.AdminChannel, channelEvent);
            }

            return Task.FromResult(0);
        }


        public override Task OnConnected()
        {
            var ev = new ChannelEvent
            {
                ChannelName = Constants.AdminChannel,
                Name = "user.connected",
                Data = new
                {
                    Context.ConnectionId,
                }
            };

            Publish(ev);

            return base.OnConnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            var ev = new ChannelEvent
            {
                ChannelName = Constants.AdminChannel,
                Name = "user.disconnected",
                Data = new
                {
                    Context.ConnectionId,
                }
            };

            Publish(ev);

            return base.OnDisconnected(stopCalled);
        }

    }



    /// <summary>
    /// A generic object to represent a broadcasted event in our SignalR hubs
    /// </summary>
    public class ChannelEvent
    {
        /// <summary>
        /// The name of the event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the channel the event is associated with
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The date/time that the event was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The data associated with the event
        /// </summary>
        public object Data
        {
            get { return _data; }
            set
            {
                _data = value;
                this.Json = JsonConvert.SerializeObject(_data);
            }
        }
        private object _data;

        /// <summary>
        /// A JSON representation of the event data. This is set automatically
        /// when the Data property is assigned.
        /// </summary>
        public string Json { get; private set; }

        public ChannelEvent()
        {
            Timestamp = DateTimeOffset.Now;
        }
    }
}
