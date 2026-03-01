using EmployeeAttendance.Models.Dto;

namespace EmployeeAttendance.Services.ModelValidation;

public class InsertEmployeeValidator : ValidatorBase<EmployeeDto>
{
    public override void Validate(EmployeeDto employee)
    {
        ValidateName(employee.Name);
        ValidateSex(employee.Sex);
        ValidateDepartment(employee.Department);
        ValidateGmail(employee.Gmail);
    }
}