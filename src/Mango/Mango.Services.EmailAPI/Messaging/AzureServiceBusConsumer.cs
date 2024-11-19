namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;

        private readonly string? emaiCartQueueName;

        private readonly IConfiguration _configuration;

        public AzureServiceBusConsumer(IConfiguration configuration)
        {
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emaiCartQueueName = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        }
    }
}
