using System.Runtime.CompilerServices;
using EmployeeAttendance.Models.Dto;

using System.Text.RegularExpressions;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Playwright;
using MimeKit;

using MailKit.Net.Smtp;
using MailKit.Security;

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
        if (!Regex.IsMatch(employee.Name, @"^[A-Za-z ]+$")) throw new ArgumentException("Name must not contain any symbols");

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
        if (!MailboxAddress.TryParse(employee.Gmail, out _)) throw new FormatException("Invalid Gmail format"); // still need more validation improvements, maybe later
    }

    public void ValidateUpdate(UpdateEmployeeDto employee) 
    {
        string[] columns = ["name", "sex", "department"];

        if (!columns.Contains(employee.Column))
        {
            throw new ArgumentException("Invalid column value");
        }

        if (employee.Column.Contains("sex"))
        {
            if (employee.Value != "male" && employee.Value != "female") throw new ArgumentException();
        }

        if (columns.Contains("department"))
        {
            string[] departments = ["it", "finance", "marketing", "customer service", "department manager"];
            if (!departments.Contains(employee.Value)) throw new ArgumentException(); 
        }
    }

    public async Task SendIdViaGmail(byte[] pdfBytes, string employeeName, string employeeEmail)
    {
        string gmailAccount = Environment.GetEnvironmentVariable("gmail_account") ?? throw new InvalidOperationException("Gmail Account is Missing");
        string gmailAppPassword = Environment.GetEnvironmentVariable("app_password") ?? throw new InvalidOperationException("Gmail App Password is Missing");

        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("Employee Management", gmailAccount));
        message.To.Add(MailboxAddress.Parse(employeeEmail));
        message.Subject = "Employee ID - Issuance Notice";

        TextPart textPart = new TextPart("plain")
        {
            Text = $"Hello {employeeName}, Attached is you official ID. \nPlease ensure to bring this id with you and use it for attendance scanning"  
        };

        MimePart pdfAttachment = new MimePart("application", "json")
        {
            Content = new MimeContent(new MemoryStream(pdfBytes)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = "employee-id.pdf"
        };

        Multipart multipart = new Multipart("mixed");
        multipart.Add(textPart);
        multipart.Add(pdfAttachment);

        message.Body = multipart;

        using SmtpClient smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(gmailAccount, gmailAppPassword);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    public async Task<Byte[]> CreateEmployeeIdPdf(string htmlFilepath, string imgFilepath, EmployeeDto employee, Byte[] qrCodeGraphic)
    {
        string html = await File.ReadAllTextAsync(htmlFilepath);

        byte[] imgBytes = await File.ReadAllBytesAsync(imgFilepath);

        string imgSrc = $"data:image/jpeg;base64,{Convert.ToBase64String(imgBytes)}"; 
        string qrSrc = $"data:image/jpeg;base64,{Convert.ToBase64String(qrCodeGraphic)}";

        html = html
                .Replace("{{Name}}", employee.Name!.ToUpper())
                .Replace("{{Department}}", employee.Department!.ToUpper())
                .Replace("{{QrCode}}", qrSrc)
                .Replace("{{Photo}}", imgSrc);

        string tempFile = Path.Combine(Path.GetTempPath(), $"{employee.Code}.html");

        byte[] pdfBytes;
        File.WriteAllText(tempFile, html);
        
        try
        {
            using IPlaywright playwright = await Playwright.CreateAsync();
            await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            IPage page = await browser.NewPageAsync();

            await page.GotoAsync($"file:///{tempFile.Replace("\\", "/")}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.Load
            });

            pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                PrintBackground = true,
                Width = "60mm",
                Height = "92.6mm",
                Margin = new Margin
                {
                    Top = "0mm",
                    Bottom = "0mm",
                    Left = "0mm",
                    Right = "0mm"
                }
            });
        } catch (Exception ex)
        {
            throw new Exception($"ERROR: {ex}");
        } finally
        {
            File.Delete(tempFile);
        }

        return pdfBytes;
    }

}