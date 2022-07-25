using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KockpitPortal.Service
{
    public interface IMyScopedService
    {
        Task DoWork(CancellationToken cancellationToken);
    }

    public class ScopedService : IMyScopedService
    {
        private readonly ILogger<ScopedService> _logger;

        public ScopedService(ILogger<ScopedService> logger)
        {
            _logger = logger;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} MyScopedService is working.");
            await Task.Delay(1000 * 20, cancellationToken);
        }
    }
}
