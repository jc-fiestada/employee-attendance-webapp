using Microsoft.Extensions.FileProviders;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using EmployeeAttendance.ResponseHandler;
using EmployeeAttendance.Services.Database;
using EmployeeAttendance.Models.Dto;


Env.Load("keys.env");
string jwtSecurityKey = Environment.GetEnvironmentVariable("jwt_secret_key") ?? throw new Exception("ERROR: JWT secret key is missing");
byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSecurityKey);
string distDir = Path.GetFullPath(Path.Combine("..", "Web", "dist"));

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    }
);

builder.Services.AddAuthorization();

builder.Services.AddScoped<Response>();
builder.Services.AddScoped<MysqlDb>();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(distDir),
    RequestPath = ""
});

app.UseAuthentication();
app.UseAuthorization();

// Admin Endpoints




app.MapPost("/admin/signin", async (AdminDto admin, Response serverResponse, MysqlDb db) =>
{
    return await serverResponse.SignIn(admin, db, keyBytes);
});


app.MapGet("/setup/database", async (Response serverResponse) =>
{
    await serverResponse.DbAndTableInit();
});

app.MapPost("/insert/employee", async (HttpClient client) =>
{
    
});

/*
app.MapGet("/admin-credentials/insert", async (MysqlDb db) =>
{
    await db.AdminCredentialsInit();
}); 
*/



app.Run();
