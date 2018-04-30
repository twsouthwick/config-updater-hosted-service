using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundQuerySample
{
    public class SomeOptionsUpdater : BackgroundService
    {
        private readonly SomeOptionsProvider _provider;
        private readonly ILogger<SomeOptionsUpdater> _log;

        public SomeOptionsUpdater(SomeOptionsProvider provider, ILogger<SomeOptionsUpdater> log)
        {
            _provider = provider;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);

                var newValue = !_provider.Options.ShouldLog;

                _log.LogDebug("Updating should log to {NewValue}", newValue);

                _provider.Options = new SomeOptions(newValue);
            }
        }
    }
}
