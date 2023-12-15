using _247.Travels.Ng.Apis.Server;
using Dna;
using Dna.AspNet;
using Xown.Travels.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure DnaFramework
builder.WebHost.UseDnaFramework(construct =>
{
    // Add configuration
    construct.AddConfiguration(builder.Configuration);
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddRazorPages();

builder.Services.AddResponseCaching();

builder.Services.ConfigureDbContext(builder.Configuration)
    // .ConfigureApiBehavior()
    .ConfigureCors()
    .AddRouteHelpers()
    .AddHttpClient()
    .AddAmadeusService()
    .AddAmadeusAuthorizationService()
    .AddBrightSunService()
    .AddIdentity()
    .AddClientAuthorization()
    .AddDomainServices()
    .AddInfrastructureProcess(builder.Configuration["PayStack:Secret"])
    .AddServiceOptions(builder.Configuration)
    .AddSmtpEmailSender();

builder.Services.AddGrpc();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDnaFramework();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.UseResponseCaching();

app.MapGrpcService<FlightDistributionService>();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
