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

		public LevelBuilder( AnaliticContext context )
		{
			_context = context;
			_context.Database.EnsureCreated();
			_tg = new TelegramClient();

		}
		protected override Task ExecuteAsync( CancellationToken stoppingToken )
		{
			TimerCallback call = new TimerCallback( Leveling );
			_timer = new Timer( call, null, 0, 7200000 ); //2 часа
			return Task.CompletedTask;
		}

		public void Leveling( object obj )
		{
			try
			{
				var levels = _context.Ticks
					.GroupBy( t => t.Price )
					.Select( x =>
					 new Level()
					 {
						 Price = x.Key,
						 Symbol = "BTCUSDT",
						 DateCreate = DateTime.Now,
						 Volume =
					 x.Sum( v => v.Volume )
					 } )
					.ToList();

				_context.Levels.RemoveRange( _context.Levels );
				_context.SaveChanges();

				_context.Levels.AddRange( levels );
				_context.SaveChanges();
			}
			catch ( Exception ex )
			{
				_tg.SendMessage( "[ServiceAnalitic] - Упал в левелинге" );
				_tg.SendMessage( "[Leveling] - " + ex.StackTrace );
			}
		}
	}
}
