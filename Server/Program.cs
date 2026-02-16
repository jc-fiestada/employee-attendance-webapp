using Microsoft.Extensions.FileProviders;
using DotNetEnv;
using EmployeeAttendance.Response;

Env.Load("keys.env");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<Response>();

var app = builder.Build();

string distDir = Path.GetFullPath(Path.Combine("..", "Web", "dist"));

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(distDir),
    RequestPath = ""
});

app.MapGet("/setup/database", async (Response serverResponse) =>
{
    await serverResponse.DbAndTableInit();
});



app.Run();
