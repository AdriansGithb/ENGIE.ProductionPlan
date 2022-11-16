using FluentValidation;
using NLog;
using NLog.Web;
using ProductionPlan.Api.Validators;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Services;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.
	builder.Services.AddControllers();

	builder.Services.AddValidatorsFromAssemblyContaining<PayloadValidator>();


    builder.Services.AddScoped<IProductionService, ProductionService>();

	// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel
                (Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseHttpsRedirection();

	app.UseAuthorization();

	app.MapControllers();

	app.Run();

}
catch (Exception exception)
{
	// NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}