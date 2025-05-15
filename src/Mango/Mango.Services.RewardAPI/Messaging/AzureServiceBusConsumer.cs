using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string? serviceBusConnectionString;

        private readonly string? orderCreatedTopic;
        private readonly string? orderCreatedRewardSubscription;

        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;

        private ServiceBusProcessor _rewardProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, RewardService rewardService)
        {
            _rewardService = rewardService;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

            var client = new ServiceBusClient(serviceBusConnectionString);
            _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
            // we need to register the handler for the processor we have created

        }
        public async Task Start()
        {
            _rewardProcessor.ProcessMessageAsync += OnNewOrdersRewardsRequestReceived;
            _rewardProcessor.ProcessErrorAsync += ErrorHandler;
            await _rewardProcessor.StartProcessingAsync();
        }


        public async Task Stop()
        {
           await _rewardProcessor.StopProcessingAsync();
           await _rewardProcessor.DisposeAsync();
        }

        public async Task OnNewOrdersRewardsRequestReceived(ProcessMessageEventArgs args)
        {
            // This is where you will receive the message
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);   
            RewardMessage rewardsMessage = JsonConvert.DeserializeObject<RewardMessage>(body);
            try
            {
                //TODO - try to log email
                await _rewardService.UpdateRewards(rewardsMessage);
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
