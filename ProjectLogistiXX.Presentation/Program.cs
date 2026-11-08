using ProjectLogistiXX.Application;
using ProjectLogistiXX.Infrastructure;
using ProjectLogistiXX.Presentation.Extensions;
using ProjectLogistiXX.Presentation.ExceptionHandlers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddExceptionHandler<IdempotencyExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapEndpoints();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program { }
