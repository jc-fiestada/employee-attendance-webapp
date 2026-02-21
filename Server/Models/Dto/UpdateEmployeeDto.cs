namespace EmployeeAttendance.Models.Dto;

public class UpdateEmployeeDto
{
    public string Column {get; set;} = default!;
    public string Value {get; set;} = default!;
    public int EmployeeId {get; set;} = default!;
}