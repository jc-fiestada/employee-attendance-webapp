using EmployeeAttendance.Services.Database;
using Sprache;

namespace EmployeeAttendance.Response;

public class Response
{
    public async Task<IResult> DbAndTableInit()
    {
        try
        {
            await new MysqlDb("db_password").InitializeDbAndTable();
        } catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex}");
            return Results.InternalServerError();
        }
        return Results.Ok();
    }
}