using RBTB_ServiceAnalitics.Database;
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
		protected override Task ExecuteAsync( CancellationToken stoppingToken )
		{
			TimerCallback call = new TimerCallback(Cleaning);
			_timer = new Timer(call, null, 0, 3600000 ); // 1 час
			return Task.CompletedTask;
		}
		public void Cleaning(object obj)
		{
			try
			{
				var dateFilter = DateTime.Now.AddDays( -1 );
				var ticks = _context.Ticks
					.Where( x => x.DateTime < dateFilter )
					.ToList();

				_context.Ticks.RemoveRange( ticks );
				_context.SaveChanges();
			}
			catch ( Exception ex ) 
			{
				_tg.SendMessage("[ServiceAnalitic] - Упал в клининге");
				_tg.SendMessage("[Cleaning] - " + ex.StackTrace);
			}
		}
	}
}
