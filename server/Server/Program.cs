using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Serilog;
using Server.Api;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            string baseAddress = "http://localhost:9123/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                // Let's wire up a SignalR client here to easily inspect what
                //  calls are happening
                //
                var hubConnection = new HubConnection(baseAddress);
                IHubProxy eventHubProxy = hubConnection.CreateHubProxy("EventHub");
                eventHubProxy.On<string, ChannelEvent>("OnEvent", (channel, ev) => Log.Information("Event received on {channel} channel - {@ev}", channel, ev));
                hubConnection.Start().Wait();

                // Join the channel for task updates in our console window
                //
                eventHubProxy.Invoke("Subscribe", Constants.AdminChannel);
                eventHubProxy.Invoke("Subscribe", Constants.TaskChannel);
                eventHubProxy.Invoke("Subscribe", Constants.MessageChannel);

                Console.WriteLine($"Server is running on {baseAddress}");
                Console.WriteLine("Press <enter> to stop server");
                Console.ReadLine();

            }
        }
    }
}
