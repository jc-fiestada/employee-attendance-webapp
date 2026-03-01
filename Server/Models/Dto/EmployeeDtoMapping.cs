using EmployeeAttendance.Models.Entities;

namespace EmployeeAttendance.Models.Dto;

// im trying to optimize my code right now using interface, abstraction and polymorphism
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