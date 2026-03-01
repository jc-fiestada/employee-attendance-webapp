using EmployeeAttendance.Models.Dto;
using System.Text.RegularExpressions;
using MimeKit;

namespace EmployeeAttendance.Services.ModelValidation;

public abstract class ValidatorBase<T> : IValidator<T>
{
    public abstract void Validate(T model);
    protected void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name must not be empty");
        if (name.Length > 255) throw new ArgumentException("Name must not exceed more than 255 characters");
        if (!Regex.IsMatch(name, @"^[A-Za-z ]+$")) throw new ArgumentException("Name must not contain any symbols");
    }

    protected void ValidateDepartment(string? department)
    {
        string[] validDepartment = ["it", "finance", "marketing", "customer service", "department manager"];
        if (string.IsNullOrWhiteSpace(department)) throw new ArgumentException("Department must not be empty");
        if (!validDepartment.Contains(department)) throw new ArgumentException("Department value is not valid");
    }

    protected void ValidateSex(string? sex)
    {
        if (string.IsNullOrWhiteSpace(sex)) throw new ArgumentException("Sex must not be empty");
        if (sex != "male" && sex != "female") throw new ArgumentException("Sex value is invalid");
    }

    protected void ValidateCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code must not be empty");
    }

    protected void ValidateGmail(string? gmail)
    {
        if (string.IsNullOrWhiteSpace(gmail)) throw new ArgumentException("Gmail must not be empty");
        if (!MailboxAddress.TryParse(gmail, out _)) throw new ArgumentException("Invalid Gmail format"); // still need more validation improvements, maybe later
    }
}