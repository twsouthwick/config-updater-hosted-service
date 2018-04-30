# Hosted services and configuration updates

Some configuration values that come from external sources need to be polled to update periodically. This should not happen within a request, and the value of the configuration should remain the same throughout the whole request. By using the IHostedService interface, we can set up a system that polls to update a value independent of requests and provide each request with a consistent state.

# Set up

## The model

The model is defined to be immutable so we can ensure that we get a clean snapshot when we access it from the provider.

```csharp
public class SomeOptions
{
    public SomeOptions(bool shouldLog)
    {
        ShouldLog = shouldLog;
    }

    public bool ShouldLog { get; }
}
```

## The provider

The provider exposes a thread-safe way of updating the instance.

```csharp
public class SomeOptionsProvider
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    private SomeOptions _options;

    public SomeOptionsProvider()
    {
        _options = new SomeOptions();
    }

    public SomeOptions Options
    {
        get
        {
            _lock.EnterReadLock();

            try
            {
                return _options;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();

            try
            {
                _options = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
```

## The updater

The updater provides an implementation of IHostedService that ASP.NET Core will host an run on as a background task. In ASP.NET Core 2.1, a BackgroundTask will be introduced that simplifies this. Since there are no external dependencies for that type, and ASP.NET Core is open source, we can grab that implementation and derive from it in ASP.NET Core 2.0.

```csharp
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
```

## Wire it together with ServiceCollection

The final step is to wirte it all together in the application. We register the updater as an IHostedService. By doing this, the infrastructure of ASP.NET Core will automatically pick it up and run it accordingly. The provider must be registered as a singleton so that we are updating the same instance in the updater. Finally, we want to ensure that the options we are receiving each request is consistent, so we grab the options from the provider as a scoped instanct.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    services.AddSingleton<IHostedService, SomeOptionsUpdater>();
    services.AddSingleton<SomeOptionsProvider>();
    services.AddScoped<SomeOptions>(ctx => ctx.GetRequiredService<SomeOptionsProvider>().Options);
}
```