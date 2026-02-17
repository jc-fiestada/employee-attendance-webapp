using EmployeeAttendance.Services.Database;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Services.Service;


using System.Text.Json;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Sprache;
using MySqlConnector;




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
    public async Task<IResult> InsertEmployeeData(HttpRequest request, MysqlDb db, Tools tool)
    {
        IFormCollection form  = await request.ReadFormAsync();

        if (!form.ContainsKey("employee") || form.Files["img"] == null)
        {
            return Results.BadRequest("Missing dto/s detected");
        }

        string rawEmployeeData = form["employee"].ToString();

        EmployeeDto employee;
        
        try
        {
            employee = JsonSerializer.Deserialize<EmployeeDto>(rawEmployeeData)!;
            tool.ValidateEmployee(employee);
        } catch (FormatException ex)
        {
            return Results.BadRequest(ex.Message);
        } catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            return Results.BadRequest("Model deserialization failed - Data might be corrupted/broken");
        }

        IFormFile imgFile = form.Files["img"]!;

        if (imgFile is null || imgFile.Length == 0)
        {
            return Results.BadRequest("Missing image detected");
        }

        bool inserted = false;

        // try for five times only to avoid infinite loop
        for (int i = 0; i < 5; i++)
        {
            try
            {
                employee.Code = Guid.NewGuid().ToString();
                await db.InsertEmployee(employee);

                inserted = true;
                break;
            } catch (MySqlException ex) when (ex.Number == 1062 && ex.Message.Contains("unique_code"))
            {
                continue;
            } catch (MySqlException ex) when (ex.Number == 1062 && ex.Message.Contains("unique_name"))
            {
                return Results.Conflict($"ERROR: Name already exist's in the database");
            } catch (MySqlException ex) when (ex.Number == 1062 && ex.Message.Contains("unique_gmail"))
            {
                return Results.Conflict($"ERROR: Gmail already exist's in the database");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Results.InternalServerError("ERROR: Something went wrong from the database");
            }
        }

        if (!inserted) return Results.InternalServerError("ERROR: Server failed to create unique employee code");

        string filepath = Path.Combine(_resourcesDirPath, $"{employee.Code}.jpeg");

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


