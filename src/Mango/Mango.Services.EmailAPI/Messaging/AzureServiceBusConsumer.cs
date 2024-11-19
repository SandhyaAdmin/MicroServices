using Azure.Messaging.ServiceBus;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;

        private readonly string? emaiCartQueueName;

        private readonly IConfiguration _configuration;

        private ServiceBusProcessor _emailCartProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration)
        {
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emaiCartQueueName = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailCartProcessor = client.CreateProcessor(emaiCartQueueName);

        }
    }
}
