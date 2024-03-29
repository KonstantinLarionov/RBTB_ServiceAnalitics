﻿using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Integration;

namespace RBTB_ServiceAnalitics.Background
{
	public class Cleaner : BackgroundService
	{
		private readonly AnaliticContext _context;
		private Timer _timer;
		private TelegramClient _tg;

        public Cleaner(AnaliticContext context)
		{

            _context = context;
			_context.Database.EnsureCreated();
			_tg = new TelegramClient();
		}
		protected override async Task ExecuteAsync( CancellationToken stoppingToken )
		{
            TimerCallback tm = new TimerCallback(Cleaning);
            _timer = new Timer( tm, null, 0, 20);
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
            try
            {
                var dateFilter = DateTime.Now.AddDays(-1);
                var ticks = _context.Ticks
                    .Where(x => x.DateTime < dateFilter)
                .ToList();

                _context.Ticks.RemoveRange(ticks);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _tg.SendMessage("[ServiceAnalitic] - Упал в клининге");
                _tg.SendMessage("[Cleaning] - " + ex.StackTrace);
            }
        }

    }
}
