using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FleetCommander.App.Server.Hubs;
using FleetCommander.App.Shared;
using FleetCommander.Simulation;
using FleetCommander.Simulation.Framework.GridSystem;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FleetCommander.App.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR().AddJsonProtocol();

            services.AddMvc();
            services.AddHostedService<GameAdvancer>();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Program>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gamehub");
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
            });

            
        }


    }

    public static class GameManager
    {
        private static Game CurrentGame { get; set; }
        public static Game GetGame()
        {
            if (CurrentGame == null)
            {
                CurrentGame = new Game(1);
                var ship1 = CurrentGame.Spawn(new FederationHeavyCruiserSpecification(), new Position
                {
                    Hex = new Hex(0, -1, 1),
                    Rotation = 0
                });
                var ship2 = CurrentGame.Spawn(new FederationHeavyCruiserSpecification(), new Position
                {
                    Hex = new Hex(1, -1, 0),
                    Rotation = 0
                });

            }

            return CurrentGame;
        }
    }


    //public class GameAdvancerBackgroundService : IHostedService, IDisposable
    //{
    //    public static IHubContext<GameHub> HubContext;
    //    private Timer _timer;

    //    public GameAdvancerBackgroundService(IHubContext<GameHub> hubContext)
    //    {
    //        HubContext = hubContext;
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {

    //        _timer = new Timer(DoWork, null, TimeSpan.Zero,
    //            TimeSpan.FromSeconds(5));

    //        return Task.CompletedTask;
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        _timer?.Change(Timeout.Infinite, 0);

    //        return Task.CompletedTask;
    //    }

    //    private void DoWork(object state)
    //    {
    //        GameManager.GetGame().AdvanceGame();


    //        HubContext.Clients.All.SendAsync()
    //    }

    //    public void Dispose()
    //    {
    //    }
    //}

        

    public class GameAdvancer:TimedHostedService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public GameAdvancer(ILogger<TimedHostedService> logger, IHubContext<GameHub> hubContext) : base(logger,4)
        {
            _hubContext = hubContext;
        }

        protected override async Task RunJobAsync(CancellationToken stoppingToken)
        {
            var game = GameManager.GetGame();
            game.AdvanceGame();

            var board = new GameBoard();
            board.TimeStamp = game.SimulationTimeStamp;
            foreach (var ship in game.AllShips)
            {
                var offset = ship.Position.Hex.ToOffsetCoord();
                board.ShipTokens.Add(new ShipToken
                {
                    Col = offset.col,
                    Row = offset.row,
                    Rot = ship.Position.Rotation
                });
            }

            await _hubContext.Clients.All.SendAsync("gameStateChanged", board,stoppingToken);

        }
    }

    /// <summary>
    /// Based on Microsoft.Extensions.Hosting.BackgroundService  https://github.com/aspnet/Extensions/blob/master/src/Hosting/Abstractions/src/BackgroundService.cs
    /// Additional info: - https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2&tabs=visual-studio#timed-background-tasks
    ///                  - https://stackoverflow.com/questions/53844586/async-timer-in-scheduler-background-service
    /// </summary>

    public abstract class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TimedHostedService> _logger;
        private readonly int _interval;
        private Timer _timer;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public TimedHostedService(ILogger<TimedHostedService> logger, int interval)
        {
            _logger = logger;
            this._interval = interval;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(ExecuteTask, null, TimeSpan.FromSeconds(_interval), TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }

        private void ExecuteTask(object state)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _executingTask = ExecuteTaskAsync(_stoppingCts.Token);
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            await RunJobAsync(stoppingToken);
            _timer.Change(TimeSpan.FromSeconds(_interval), TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task 
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        protected abstract Task RunJobAsync(CancellationToken stoppingToken);

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
        }
    }
}
