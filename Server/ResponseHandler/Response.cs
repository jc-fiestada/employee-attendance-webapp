using EmployeeAttendance.Services.Database;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Models.Entities;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;




namespace EmployeeAttendance.ResponseHandler;

public class Response
{
    private readonly string _resourcesDirPath = Path.Combine("..", "Resources");
    public async Task<IResult> DbAndTableInit()
    {
        try
        {
            await new MysqlDb().InitializeDbAndTable();
        } catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        }
        return Results.Ok();
    }

    // unfinished
    public async Task<IResult> InsertEmployeeData(HttpRequest request)
    {
        IFormCollection form  = await request.ReadFormAsync();

        if (!form.ContainsKey("employee") || !form.ContainsKey("img"))
        {
            
        }

        string rawEmployeeData = form["employee"].ToString();

        EmployeeDto employee = default!;
        
        try
        {
            employee = JsonSerializer.Deserialize<EmployeeDto>(rawEmployeeData)!;
        }
        catch (Exception ex)
        {
            
        }

        IFormFile imgFile = form.Files["img"]!;

        if (imgFile is null || imgFile.Length == 0)
        {
            return Results.BadRequest();
        }

        employee.Code = Guid.NewGuid().ToString();

        string filename = $"${employee.Code}.jpeg";
        string filepath = Path.Combine(_resourcesDirPath, filename);

        using Image image = await Image.LoadAsync(imgFile.OpenReadStream());

        await image.SaveAsync(filepath, new JpegEncoder {Quality = 100});

        return Results.Ok();
    }

    public async Task<IResult> SignIn(AdminDto admin, MysqlDb db, Byte[] keyBytes)
    {
        try
        {
            if (!await db.IsAdminValid(admin)) return Results.Unauthorized();
        } catch (InvalidOperationException ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        } catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        }

        Claim[] userClaims = new Claim[]
        {
            new Claim(ClaimTypes.Name, "ManagementAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        JwtSecurityToken token = new JwtSecurityToken(
            claims: userClaims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Json(new {token = jwt}, statusCode: 200);
    }

}


