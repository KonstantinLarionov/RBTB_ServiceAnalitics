using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Integration;
using RBTB_ServiceAnalitics.Markets.Binance;

namespace RBTB_ServiceAnalitics.Background
{
	public class TickCollector : BackgroundService
	{
		private readonly AnaliticContext _context;
		private readonly BinanceWebSocket _binanceSocket;
		private TelegramClient _tg;

		public TickCollector(AnaliticContext context)
		{
			_context = context;
			_context.Database.EnsureCreated();
			_binanceSocket = new BinanceWebSocket();
			_binanceSocket.Symbol = "BTCUSDT";
			_binanceSocket.TradeEv += _binanceSocket_TradeEv;
			_tg = new TelegramClient();

		}

		protected override Task ExecuteAsync( CancellationToken stoppingToken )
		{
			_binanceSocket.Start();
			return Task.CompletedTask;
		}

		private void _binanceSocket_TradeEv( BinanceMapper.Spot.MarketWS.Events.TradeEvent tradesEvent )
		{
			try
			{
				_context.Ticks.Add( new Database.Entities.Tick()
				{
					Price = tradesEvent.Price,
					Volume = tradesEvent.Quantity,
					Milliseconds = tradesEvent.TradeTime,
					DateTime = DateTime.Now,
					Symbol = tradesEvent.Symbol
				} );
				_context.SaveChanges();
			}
			catch ( Exception ex )
			{
				_tg.SendMessage( "[ServiceAnalitic] - Упал в тикере" );
				_tg.SendMessage( "[Тикер] - " + ex.StackTrace );
			}
		}
	}
}
