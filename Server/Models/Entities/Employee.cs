namespace EmployeeAttendance.Models.Entities;

public class Employee
{
    public int Id {get; set;}
    public string Name {get; set;} = default!;
    public string Sex {get; set;} = default!;
    public string Department {get; set;} = default!;
    public DateTime DateTime {get; set;} = default!;
    public string Code {get; set;} = default!;
    public string Img_Filename = "";
}