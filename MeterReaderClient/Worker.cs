using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeterReaderClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private ReadingFactory _readingFactory;
        private MeterReadingService.MeterReadingServiceClient _client;

        public Worker(ILogger<Worker> logger, IConfiguration config, ReadingFactory readingFactory)
        {
            _logger = logger;
            _config = config;
            _readingFactory = readingFactory;
        }

        protected MeterReadingService.MeterReadingServiceClient client
        {
            get
            {
                if (_client == null)
                {
                    var channel = GrpcChannel.ForAddress(_config["Service:ServiceUrl"]);
                    _client = new MeterReadingService.MeterReadingServiceClient(channel);
                }
                return _client;
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                counter++;
                var customerId = _config.GetValue<int>("Service:CustomerId");

                if (counter%10==0)
                {
                    Console.WriteLine("Send Diagnostics");
                    var stream=_client.SendDiagnostics();

                    for (int i = 0; i < 5; i++)
                    {
                        var reading=await _readingFactory.GetReadings(customerId);
                        await stream.RequestStream.WriteAsync(reading);
                    }

                    await stream.RequestStream.CompleteAsync();

                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);


                var pkt = new ReadingPacket()
                {
                    Successful = ReadingStatus.Success,
                    Notes = "This is testing message",
                };

                for (int i = 0; i < 5; i++)
                {
                    pkt.Readings.Add(await _readingFactory.GetReadings(customerId));
                }

                var result = await client.AddReadingAsync(pkt);
                if (result.Success == ReadingStatus.Success)
                {
                    _logger.LogInformation("Successfully sent");
                }
                else
                {
                    _logger.LogInformation("Failed to sent");
                }

                await Task.Delay(_config.GetValue<int>("Service:DelayInterval"), stoppingToken);
            }
        }
    }
}
