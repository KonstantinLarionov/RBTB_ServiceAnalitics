using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Integration;

namespace RBTB_ServiceAnalitics.Background
{
    public class Cleaner : BackgroundService
    {
        private readonly AnaliticContext _context;
        private Timer? _timer;
        private TelegramClient _tg;
        private object locker = new();
        private readonly string _connectionString;
        public Cleaner(AnaliticContext context, TelegramClient telegramClient, IConfiguration configuration)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _tg = telegramClient;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DBConnection")!;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimerCallback tm = new TimerCallback(Cleaning!);
            _timer = new Timer(tm, null, 0, 20);
            if (stoppingToken.IsCancellationRequested)
            {
                try
                {
                }
                catch
                {
                    Console.WriteLine("Timed Hosted Service is stopping.");
                }
            }
            await Task.CompletedTask;
        }
        public void Cleaning(object obj)
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
                            var dateFilter = DateTime.Now.AddDays(-1);
                            var ticks = context.Ticks
                                .Where(x => x.DateTime < dateFilter)
                            .ToList();
                            context.Ticks.RemoveRange(ticks);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _tg.SendMessage("[ServiceAnalitic] - Упал в клининге");
                            _tg.SendMessage("[Cleaning] - " + ex.StackTrace);
                            _tg.SendMessage("[Cleaning] - " + ex.Message);
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