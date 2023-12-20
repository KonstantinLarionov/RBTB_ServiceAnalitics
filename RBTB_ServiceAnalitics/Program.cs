using Microsoft.EntityFrameworkCore;
using RBTB_ServiceAnalitics.Background;
using RBTB_ServiceAnalitics.Database;
using RBTB_ServiceAnalitics.Doamin.Options;
using RBTB_ServiceAnalitics.Integration;

var builder = WebApplication
	.CreateBuilder( args );

// Add services to the container.

#region [Options]

builder.Services.Configure<TelegramOption>(builder.Configuration.GetSection("TelegramOption"));
builder.Services.Configure<ServiceAnaliticsOption>(builder.Configuration.GetSection("ServiceAnaliticsOption"));

#endregion

#region [Hosteds]

builder.Services.AddHostedService<LevelBuilder>();
builder.Services.AddHostedService<TickCollector>();
builder.Services.AddHostedService<Cleaner>();

#endregion

#region [Services]

builder.Services.AddTransient<TelegramClient>();

#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<AnaliticContext>(
	options => { options.LogTo(Console.WriteLine, LogLevel.Error); options.UseNpgsql( builder.Configuration.GetConnectionString( "DbConnection" ) ); }, ServiceLifetime.Transient ) ;

AppContext.SetSwitch( "Npgsql.EnableLegacyTimestampBehavior", true );

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
