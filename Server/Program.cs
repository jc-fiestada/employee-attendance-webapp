using Microsoft.Extensions.FileProviders;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using EmployeeAttendance.ResponseHandler;
using EmployeeAttendance.Services.Database;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Services.Service;
using System.ComponentModel.DataAnnotations;


string gmailEnvFilepath = Path.GetFullPath(Path.Combine("..", "..", "DummyGmail", "gmail.env"));

Env.Load("keys.env");
Env.Load(gmailEnvFilepath);

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});

builder.Services.AddScoped<Response>();
builder.Services.AddScoped<MysqlDb>();
builder.Services.AddScoped<Tools>();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(distDir),
    RequestPath = ""
});

app.UseAuthentication();
app.UseAuthorization();

// Admin Endpoints



// tested
app.MapPost("/admin/signin", async (AdminDto admin, Response serverResponse, MysqlDb db) =>
{
    return await serverResponse.SignIn(admin, db, keyBytes);
});

app.MapPost("/authenticate/page-access", () =>
{
    return Results.Ok("Welcome Admin");
}).RequireAuthorization("AdminOnly");


app.MapGet("/setup/database", async (Response serverResponse) =>
{
    await serverResponse.DbAndTableInit();
});

app.MapPost("/insert/employee", async (Response response, HttpRequest request, MysqlDb db, Tools tool) =>
{
    return await response.InsertEmployeeData(request, db, tool);
}).RequireAuthorization("AdminOnly");


app.MapPost("/delete/employee", async (EmployeeId employee, Response response, MysqlDb db) =>
{
    return await response.DeleteEmployee(employee.id, db);
});

app.MapPost("/update/employee", async (EmployeeId id) =>
{
    
});





app.Run();

record EmployeeId (int id);
