using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
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

            // we need to register the handler for the processor we have created

        }

        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();
        }


        public async Task Stop()
        {
           await _emailCartProcessor.StartProcessingAsync();
           await _emailCartProcessor.DisposeAsync();
        }

        public async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            // This is where you will receive the message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);   
            CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(body);
            try
            {
                //TODO - try to log email
                //the below line tells the queue that message has been processed successfully and you can remove that from your queue
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }


        
    }
}
