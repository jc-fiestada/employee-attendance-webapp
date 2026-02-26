# Employee Attendance WebApp (QR Code and Gmail Integration)

A web application designed to simplify employee attendance management.  
It features automated ID generation and QR code scanning.  
Admin can manage employee records, generate unique IDs which are also used in generating Qr Code, 
and track attendance efficiently while automating delivery of IDs via Gmail.  

## Features

- **CRUD Operations** – Easily view, add, update, and remove employee records.  

- **Automated ID Generation** – Create unique IDs for employees, including QR codes.  

- **QR Code Attendance** – Employees can mark attendance quickly by scanning their QR codes in their ID.  

- **Email Integration** – Generated IDs are automatically sent to employees via Gmail.  

- **Security (Authentication/Authorization)** – JWT tokens ensure that the only authorized users like admin can access sensitive operations unlike public API endpoints.

- **Separation of UI and Server** – Frontend is built with TypeScript and bundled using Parcel for easier code management.  

## Packages & Tools Used

- **Backend / C# Minimal API**:  
    BCrypt.Net-Next, dotnetenv, MailKit, Microsoft.AspNetCore.Authentication.JwtBearer,  
    Microsoft.Playwright, MySqlConnector, QRCoder, SixLabors.ImageSharp, System.IdentityModel.Tokens.Jwt  

- **Frontend**:  
    TypeScript, qr-scanner, Parcel bundler  