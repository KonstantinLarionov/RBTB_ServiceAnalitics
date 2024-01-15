using Microsoft.Extensions.Options;
using RBTB_ServiceAnalitics.Doamin.Options;
using System.Net;

namespace RBTB_ServiceAnalitics.Integration;
public class TelegramClient
{
    private readonly TelegramOption _option;
    private readonly string _token;
    private readonly string _chat;
    private readonly HttpClient HttpClient;

    public TelegramClient(IOptions<TelegramOption> options)
    {
        _option = options.Value ?? throw new ArgumentException(nameof(options));

        HttpClient = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _token = _option.Token;
        _chat = _option.ChatId;
    }
    public void SendMessage(string mess) =>
        HttpClient
            .GetAsync($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={_chat}&text={mess}");
}
