using Google.Protobuf.WellKnownTypes;
using MeterReaderWeb.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MeterReaderClient
{
    public class ReadingFactory
    {
        private readonly ILogger<ReadingFactory> Logger;
        public ReadingFactory(ILogger<ReadingFactory> logger)
        {
            Logger = logger;
        }

        public Task<ReadingMessage> GetReadings(int customerId)
        {
            var readingMessage = new ReadingMessage
            {
                CustomerId = customerId,
                ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow),
                ReadingValue = new Random().Next(10000)
            };
            return Task.FromResult(readingMessage);
        }
    }
}
