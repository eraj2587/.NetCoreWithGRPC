using Grpc.Core;
using MeterReaderWeb.Data;
using MeterReaderWeb.Data.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeterReaderWeb.Services
{
    public class MeterService : MeterReadingService.MeterReadingServiceBase
    {
        private readonly ILogger<MeterService> logger;
        private readonly IReadingRepository readingRepository;

        public MeterService(ILogger<MeterService> logger, IReadingRepository readingRepository)
        {
            this.logger = logger;
            this.readingRepository = readingRepository;
        }

        public async override Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
        {
            var result = new StatusMessage()
            {
                Success = ReadingStatus.Failure
            };

            try
            {
                if (request.Successful == ReadingStatus.Success)
                {
                    foreach (var reading in request.Readings)
                    {
                        var read = new MeterReading
                        {
                            CustomerId = reading.CustomerId,
                            Value = reading.ReadingValue,
                            ReadingDate = reading.ReadingTime.ToDateTime()
                        };
                    readingRepository.AddEntity(read);
                    }
                   if(await readingRepository.SaveAllAsync())
                    {
                        result.Success = ReadingStatus.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = "Exception throwing during process";
                logger.LogError($"Error occurred while processing : {ex}");
            }

            return result;
            
        }
    }
}
