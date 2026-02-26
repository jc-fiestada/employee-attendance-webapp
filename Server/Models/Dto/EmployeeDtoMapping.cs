using EmployeeAttendance.Models.Entities;

namespace EmployeeAttendance.Models.Dto;

// ill try to learn and use interface in the future, i do know now why people use it and not just in ts as a type check
// but im not using it here in this project, dont have enough time to optimize
public class EmployeeDtoMapping
{
    public static EmployeeDto MaptoEmployeeDto(Employee employee)
    {
        return new EmployeeDto
        {
            Name = employee.Name,
            Sex = employee.Sex,
            Department = employee.Department,
            Code = employee.Code,
            Gmail = employee.Code
        };
    } 
}