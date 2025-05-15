using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;

        private readonly string? emaiCartQueueName;
        private readonly string? registerUserQueueName;

        private readonly string? orderCreatedTopicName;
        private readonly string? orderCreatedEmailSubscriptionName;

        private readonly IConfiguration _configuration;

        private readonly EmailService _emailService;

        private ServiceBusProcessor _emailCartProcessor;
        private ServiceBusProcessor _registerUserProcessor;

        private ServiceBusProcessor _orderCreatedEmailProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _emailService = emailService;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            emaiCartQueueName = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            registerUserQueueName = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");

            orderCreatedTopicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedEmailSubscriptionName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Email_Subscription");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _emailCartProcessor = client.CreateProcessor(emaiCartQueueName);
            _registerUserProcessor = client.CreateProcessor(registerUserQueueName);

            _orderCreatedEmailProcessor = client.CreateProcessor(orderCreatedTopicName, orderCreatedEmailSubscriptionName);

            // we need to register the handler for the processor we have created

        }
        public async Task Start()
        {
            _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            _emailCartProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailCartProcessor.StartProcessingAsync();

            _registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
            _registerUserProcessor.ProcessErrorAsync += ErrorHandler;
            await _registerUserProcessor.StartProcessingAsync();

            _orderCreatedEmailProcessor.ProcessMessageAsync += onOrderCreatedEmailRequestReceived;
            _orderCreatedEmailProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderCreatedEmailProcessor.StartProcessingAsync();
        }


        public async Task Stop()
        {
           await _emailCartProcessor.StopProcessingAsync();
           await _emailCartProcessor.DisposeAsync();

           await _registerUserProcessor.StopProcessingAsync();
           await _registerUserProcessor.DisposeAsync();

            await _orderCreatedEmailProcessor.StopProcessingAsync();
            await _orderCreatedEmailProcessor.DisposeAsync();
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
                await _emailService.EmailCartAndLog(cartDto);
                //the below line tells the queue that message has been processed successfully and you can remove that from your queue
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
        {
            // This is where you will receive the message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            string? email = JsonConvert.DeserializeObject<string>(body);
            try
            {
                //TODO - try to log email
                await _emailService.RegisterUserEmailAndLog(email);
                //the below line tells the queue that message has been processed successfully and you can remove that from your queue
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task onOrderCreatedEmailRequestReceived(ProcessMessageEventArgs args)
        {
            // This is where we are receiving the message from orderCreatedEmailSubscription
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            //Deserialize the message as rewardmessage object
            RewardMessage rewardMessage = JsonConvert.DeserializeObject<RewardMessage>(body);

            try
            {
                // Save the rewardMessage data once orderCreated to EmailLoggers table which is used to send email notification to users
                await _emailService.LogOrderPlaced(rewardMessage);

                //the below line is used to notify topic that the message has been processed successfully. you can remove it from the topic
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
