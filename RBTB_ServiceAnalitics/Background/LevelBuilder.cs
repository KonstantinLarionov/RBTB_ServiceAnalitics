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
        public LevelBuilder(AnaliticContext context, TelegramClient telegramClient)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _tg = telegramClient;

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
            _timer = new Timer(tm, null, 0, 2000);
        }

        public void Leveling(object obj)
        {
            try
            {
                var levels = _context.Ticks
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

                _context.Levels.RemoveRange(_context.Levels);
                _context.SaveChanges();

                _context.Levels.AddRange(levels);
                _context.SaveChanges();


            }
            catch (Exception ex)
            {
                _tg.SendMessage("[ServiceAnalitic] - Упал в левелинге");
                _tg.SendMessage("[Leveling] - " + ex.StackTrace);
            }
        }
    }
}
