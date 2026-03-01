using EmployeeAttendance.Models.Dto;

namespace EmployeeAttendance.Services.ModelValidation;

public class FilterEmployeeValidator : ValidatorBase<FilteredEmployeeDto>
{
    private void ValidateColumn(string? column)
    {
        if (string.IsNullOrWhiteSpace(column)) throw new ArgumentNullException("Filter field for column is null");
        if (column != "name" && column != "department") throw new ArgumentException("Filter field for column value is invalid");
    }
    public override void Validate(FilteredEmployeeDto filter)
    {
        ValidateColumn(filter.Column);
        if (filter.Column == "department") ValidateDepartment(filter.Value);
    }
}