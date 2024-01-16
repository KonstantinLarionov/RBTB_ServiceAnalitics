using Microsoft.EntityFrameworkCore;
using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Database.Entities;
using RBTB_ServiceAnalitics.Integration;

namespace RBTB_ServiceAnalitics.Background
{
    public class LevelBuilder : BackgroundService
    {
        private readonly AnaliticContext _context;
        private Timer _timer;
        private TelegramClient _tg;
        Mutex mutexObj = new();
        private Object locker = new();

        private readonly string _connectionString;
        public LevelBuilder(AnaliticContext context, TelegramClient telegramClient, IConfiguration configuration)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _tg = telegramClient;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DBConnection")!;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunLeveing();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                }
                catch (Exception e)
                {
                    // log exception
                }
                await Task.Delay(TimeSpan.FromSeconds(3)); // To prevent restarting too often
            }
            await Task.CompletedTask;
        }

        private async Task RunLeveing()
        {
            TimerCallback tm = new TimerCallback(Leveling);
            _timer = new Timer(tm, null, 0, 20);
        }

        public void Leveling(object obj)
        {
            if (Monitor.TryEnter(locker))
            {
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<AnaliticContext>();

                    var options = optionsBuilder.UseNpgsql(_connectionString).Options;

                    using (AnaliticContext context = new AnaliticContext(options))
                    {
                        try
                        {
                            var levels = context.Ticks
                                    .GroupBy(t => t.Price)
                                    .Select(x =>
                                     new Level()
                                     {
                                         Price = x.Key,
                                         Symbol = "BTCUSDT",
                                         DateCreate = DateTime.Now,
                                         Volume =
                                     x.Sum(v => v.Volume)
                                     })
                                    .ToList();

                            context.Levels.RemoveRange(context.Levels);
                            context.Levels.AddRange(levels);

                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _tg.SendMessage("[ServiceAnalitic] - Упал в левелинге");
                            _tg.SendMessage($"[Leveling] - {ex.Message}");
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
        }
    }
}
