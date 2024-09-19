using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Quartz;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Diagnostics;

namespace azure_web_service_quartz_worker
{
    public class Worker(ServiceBusClient serviceBusClient, IConfiguration configuration) : IJob
    {
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
        private readonly IConfiguration _configuration = configuration;
        private const string QueueName = "my-queue"; 

        public async Task Execute(IJobExecutionContext context)
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            var client = new ServiceBusClient(_configuration["ServiceBusConnectionString"], clientOptions);

            var processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;

                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }     
        }

        public static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        public static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
