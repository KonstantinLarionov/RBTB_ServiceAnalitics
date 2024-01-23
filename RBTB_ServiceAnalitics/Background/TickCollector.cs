using BybitMapper.UTA.MarketStreamsV5.Events;
using Microsoft.Extensions.Options;
using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Doamin.Options;
using RBTB_ServiceAnalitics.Integration;
using RBTB_ServiceAnalitics.Markets.Bybit;
using WebSocketSharp;

namespace RBTB_ServiceAnalitics.Background
{
    public class TickCollector : BackgroundService
    {
        private readonly AnaliticContext _context;
        private readonly BybitWebSocket _bybitWebSocket;
        private TelegramClient _tg;
        private readonly ServiceAnaliticsOption _option;
        private readonly string _symbol = "BTCUSDT";
        private Timer? _pingSender;

        public TickCollector(AnaliticContext context, TelegramClient telegramClient, IOptions<ServiceAnaliticsOption> options)
        {
            _context = context;
            _context.Database.EnsureCreated();

            _option = options.Value ?? throw new ArgumentException(nameof(options));

            _bybitWebSocket = new BybitWebSocket(_option.WsUrl, _option.CounterReconnectWS);
            _bybitWebSocket.Symbol = _option.Symbol;
            _bybitWebSocket.TradeEvent += _bybitSocket_TradeEvent;
            _bybitWebSocket.ErrorEvent += _bybitSocket_ErrorEvent;
            _bybitWebSocket.OpenEvent += _bybitSocket_OpenEvent;
            _bybitWebSocket.CloseEvent += _bybitSocket_CloseEvent;

            _tg = telegramClient;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _bybitWebSocket.Start();
            //_bybitWebSocket.PublicSubscribe(_symbol, BybitMapper.UTA.MarketStreamsV5.Data.Enums.PublicEndpointType.Trade, BybitMapper.UTA.RestV5.Data.Enums.IntervalType.TwoHour);

            return Task.CompletedTask;
        }

        private void _bybitSocket_TradeEvent(TradeEvent tradesEvent)
        {
            try
            {
                if (tradesEvent.Data.Count <= 0)
                {
                    return;
                }
                _context.Ticks.Add(new Database.Entities.Tick()
                {
                    Price = tradesEvent.Data[0].Price!.Value,
                    Volume = tradesEvent.Data[0].Volume!.Value,
                    Milliseconds = tradesEvent.Data[0].TimestampDateTime!.Value.Millisecond,
                    DateTime = DateTime.Now,
                    Symbol = tradesEvent.Data[0].Symbol
                });
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _tg.SendMessage("[ServiceAnalitic] - Упал в тикере");
                _tg.SendMessage("[Тикер] - " + ex.StackTrace);
            }
        }

        private void _bybitSocket_ErrorEvent(object sender, Exception exception, int countReconnect = 0, bool reconnect = false)
        {
            _pingSender = null;
            _tg.SendMessage($"[bybitWebSocket] - Упал в сокете");

            if (reconnect)
            {
                _tg.SendMessage($"[bybitWebSocket] - Попытка переподключения {countReconnect}");
            }
        }

        private void _bybitSocket_OpenEvent(object sender, EventArgs baseEvent)
        {
            _pingSender = new Timer((_) => _bybitWebSocket.Ping(), null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            _bybitWebSocket.PublicSubscribe(_symbol, BybitMapper.UTA.MarketStreamsV5.Data.Enums.PublicEndpointType.Trade, BybitMapper.UTA.RestV5.Data.Enums.IntervalType.TwoHour);

            _tg.SendMessage($"[bybitWebSocket] - подключение установлено");
        }

        private void _bybitSocket_CloseEvent(object sender, CloseEventArgs baseEvent)
        {
        }
    }
}
