using EmployeeAttendance.Services.Database;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Services.Service;


using System.Text.Json;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Sprache;
using MySqlConnector;
using EmployeeAttendance.Models.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

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

    public async Task<IResult> InsertEmployeeData(HttpRequest request, MysqlDb db, Tools tool)
    {

        IFormCollection form  = await request.ReadFormAsync();

        // checks for keys if exists

        if (!form.ContainsKey("employee") || form.Files["img"] == null)
        {
            return Results.BadRequest("Missing dto/s detected");
        }

        string rawEmployeeData = form["employee"].ToString(); // raw data
        EmployeeDto employee;
        
        try
        {
            // deserialize in a case insensitive way then validates it
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            employee = JsonSerializer.Deserialize<EmployeeDto>(rawEmployeeData, options) ?? throw new Exception();
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

        // check for image content

        IFormFile imgFile = form.Files["img"]!; // raw img file

        if (imgFile is null || imgFile.Length == 0)
        {
            return Results.BadRequest("Missing image detected");
        }

        bool inserted = false;

        // try to insert data to db

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
        string imgFilepath = Path.Combine(_resourcesDirPath, $"{employee.Code}.jpeg");
        await tool.SaveEmployeeJpeg(imgFile, imgFilepath); // tested until right here
        byte[] qrCode = tool.GenerateQrCode(employee.Code!);

        Byte[] pdfByte = await tool.CreateEmployeeId(Path.Combine("id-template", "template.html"), imgFilepath, employee, qrCode);

        try
        {
            await tool.SendIdViaGmail(pdfByte, employee.Name!, employee.Gmail!);
        } catch (Exception ex)
        {
            return Results.InternalServerError(ex);
        }

        // send via gmail
        return Results.File(pdfByte, contentType: "application/pdf");
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
            expires: DateTime.UtcNow.AddMinutes(45),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
        );

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Json(new {token = jwt}, statusCode: 200);
    }

    public async Task<IResult> DeleteEmployee(int employeeId, MysqlDb db)
    {
        try
        {
            string code = await db.DeleteEmployee(employeeId);
            string imgFilepath = Path.GetFullPath(Path.Combine("..", "Resources", $"{code}.jpeg"));
            File.Delete(imgFilepath);
        } catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
         catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError("Something went wrong - Internal server error");
        }
        return Results.Ok();
    }

    public async Task<IResult> SelectAllEmployee(MysqlDb db)
    {
        List<Employee> employees;
        try
        {
            employees = await db.SelectAllEmployee();
        } catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        }
        if (employees is null || employees.Count() == 0) return Results.NotFound();
        return Results.Json(employees, statusCode: 200);
    }

    public async Task<IResult> UpdateEmployee(UpdateEmployeeDto update, MysqlDb db)
    {
        try
        {
            int affected = await db.UpdateEmployee(update.Column, update.Value, update.EmployeeId);
            if (affected == 0)
            {
                return Results.NotFound();
            }
        } catch (ArgumentException ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.BadRequest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        }
        return Results.Ok();
    }

}


