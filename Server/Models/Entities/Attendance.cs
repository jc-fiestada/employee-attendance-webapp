namespace EmployeeAttendance.Models.Entities;

public class Attendance
{
    public required string Code {get; set;}
    public required string Name {get ; set;}
    public required DateTime DateAndTime {get; set;}
}
