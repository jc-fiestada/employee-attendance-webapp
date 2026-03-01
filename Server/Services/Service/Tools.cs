using System.Runtime.CompilerServices;
using EmployeeAttendance.Models.Dto;

using System.Text.RegularExpressions; //
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.Playwright;
using MimeKit;

using MailKit.Net.Smtp; // 
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
        } catch (Exception)
        {
            throw new Exception($"ERROR: PDF ERROR");
        } finally
        {
            File.Delete(tempFile);
        }

        return pdfBytes;
    }

}