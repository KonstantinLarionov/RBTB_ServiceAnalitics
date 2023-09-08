using System.Net;

namespace RBTB_ServiceAnalitics.Integration
{
	public class TelegramClient
	{
		private string Token { get; set; }
		private string Chat { get; set; }
		private HttpClient HttpClient { get; set; }

		public TelegramClient( string token = "6072379432:AAFGUfuPxwu6l6rTgsozMiNwHxZGx44mdTM", string chat = "711186760")
		{
			HttpClient = new HttpClient();
			System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			Token = token;
			Chat = chat;
		}
		public void SendMessage( string mess ) =>
			HttpClient
				.GetAsync( $"https://api.telegram.org/bot{Token}/sendMessage?chat_id={Chat}&text={mess}" )
				.GetAwaiter()
				.GetResult();
	}
}
