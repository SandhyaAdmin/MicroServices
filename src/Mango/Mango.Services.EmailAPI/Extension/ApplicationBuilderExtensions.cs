using Mango.Services.EmailAPI.Messaging;
using System.Reflection.Metadata;

namespace Mango.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtensions
    {
        private static IAzureServiceBusConsumer serviceBusConsumer { get; set; }
        public static IApplicationBuilder UseServiceBusConsumer(this IApplicationBuilder app) 
        {
            // we are asking for an implentation here
            serviceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();

            // The below line notifies the application life time when it is started and when it is stopped
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            // When the application starts we want to register a new methis OnStart()
            hostApplicationLife.ApplicationStarted.Register(OnStart);

            // When the application stops we want to register a new methis OnStart()
            hostApplicationLife.ApplicationStopping.Register(OnStop);

            return app;
        }

        private static void OnStart()
        {
            serviceBusConsumer.Start();
        }

        private static void OnStop()
        {
            serviceBusConsumer.Stop();
        }
    }
}
