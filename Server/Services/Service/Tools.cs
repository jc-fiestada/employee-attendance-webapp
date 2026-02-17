using System.Runtime.CompilerServices;
using EmployeeAttendance.Models.Dto;

using System.Text.RegularExpressions;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Net.Mail;

namespace EmployeeAttendance.Services.Service;

public class Tools
{
    public async Task SaveEmployeeJpeg(IFormFile image, string filepath)
    {
        Image jpeg = await Image.LoadAsync(image.OpenReadStream());
        await jpeg.SaveAsync(filepath, new JpegEncoder {Quality = 100});
    }

    public Byte[] GenerateQrCode(string code)
    {
        using QRCodeGenerator qrGenerator = new QRCodeGenerator();
        using QRCodeData qrData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);

        using PngByteQRCode qrCode = new PngByteQRCode(qrData);

        byte[] qrBytes = qrCode.GetGraphic(20);
        return qrBytes;
    }

    public void ValidateEmployee(EmployeeDto employee)
    {
        // Name Check
        if (string.IsNullOrWhiteSpace(employee.Name)) throw new FormatException("Name must not be empty");
        if (employee.Name.Length > 255) throw new ArgumentException("Name must not exceed more than 255 characters");
        if (!Regex.IsMatch(employee.Name, @"^[A-Za-z]+$")) throw new ArgumentException("Name must not contain any symbols");

        // Sex Check
        if (string.IsNullOrWhiteSpace(employee.Sex)) throw new FormatException("Sex must not be empty");
        if (employee.Sex != "male" && employee.Sex != "female") throw new FormatException("Sex value is invalid");

        // department
        string[] validDepartment = ["it", "finance", "marketing", "customer service", "department manager"];
        if (string.IsNullOrWhiteSpace(employee.Department)) throw new FormatException("Department must not be empty");
        if (!validDepartment.Contains(employee.Department)) 
        throw new FormatException("Department value is not valid");

        // gmail
        if (string.IsNullOrWhiteSpace(employee.Gmail)) throw new FormatException("Gmail must not be empty");
        if (!MailAddress.TryCreate(employee.Gmail, out _)) throw new FormatException("Invalid Gmail format");
    }

    public async void CreateEmployeeId(string filepath, EmployeeDto employee, Byte[] qrCodeGraphic)
    {
        string html = await File.ReadAllTextAsync(filepath);

        html = html
                .Replace("{{Name}}", employee.Name)
                .Replace("{{Department}}", employee.Department);

    }

}