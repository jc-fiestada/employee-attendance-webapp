using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string distDir = Path.GetFullPath(Path.Combine("..", "Web", "dist"));

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(distDir),
    RequestPath = ""
});


app.MapGet("/", () => "Hello World!");

app.Run();
