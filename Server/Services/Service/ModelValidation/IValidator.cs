using Microsoft.VisualBasic;

namespace EmployeeAttendance.Services.ModelValidation;

public interface IValidator<T>
{
    void Validate(T model);
}