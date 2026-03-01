using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Models.Entities;

namespace EmployeeAttendance.Services.ModelValidation;

public class UpdateEmployeeValidator : ValidatorBase<UpdateEmployeeDto>
{
    private void ValidateColumn(string column)
    {
        string[] columns = ["name", "sex", "department"];
        if (!columns.Contains(column)) throw new ArgumentException("Invalid column value");
    }

    public override void Validate(UpdateEmployeeDto employee)
    {
        ValidateColumn(employee.Column);
        if (employee.Column == "name") ValidateName(employee.Value);
        if (employee.Column == "sex") ValidateSex(employee.Value);
        if (employee.Column == "department") ValidateDepartment(employee.Value);
    }
}