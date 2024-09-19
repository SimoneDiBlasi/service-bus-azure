using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace azure_webservice_api.Controllers
{
    [ApiController]
    [Route("api")]



    public class ProducerController(ServiceBusClient serviceBusClient, IConfiguration configuration, ServiceBusAdministrationClient adminClient) : ControllerBase
    {
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
        private readonly ServiceBusAdministrationClient _adminClient = adminClient;
        private readonly IConfiguration _configuration = configuration;


        [HttpPost]
        [Route("producer")]
        public async Task<IActionResult> Producer([FromBody] string producerMessage)
        {
            try
            {
                if (!await _adminClient.QueueExistsAsync(_configuration["ServiceBus:Queue"]))
                {
                    return NotFound($"Queue '{_configuration["ServiceBus:Queue"]}' does not exist.");
                }

                var sender = _serviceBusClient.CreateSender(_configuration["ServiceBus:Queue"]);
                var message = new ServiceBusMessage(JsonSerializer.Serialize(producerMessage));
                await sender.SendMessageAsync(message);
                return Ok();
            }
            catch (ServiceBusException ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}