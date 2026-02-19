using MySqlConnector;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Models.Entities;
using BCrypt.Net;

namespace EmployeeAttendance.Services.Database;

public class MysqlDb
{

    private readonly string _serverConn; // use only to generate db
    private readonly string _dbConn; // use this for db connection

    public MysqlDb()
    {
        string _dbPassword = Environment.GetEnvironmentVariable("db_password") ?? throw new Exception("ERROR: database key missing");

        _serverConn = $"Server=localhost;User=root;Password={_dbPassword}";
        _dbConn = $"Server=localhost;Database=management;User=root;Password={_dbPassword}";
    }

    private async Task DatabaseInit()
    {
        using MySqlConnection conn = new MySqlConnection(_serverConn);

        await conn.OpenAsync();

        string query = @"CREATE DATABASE IF NOT EXISTS management";

        MySqlCommand command = new MySqlCommand(query, conn);
        await command.ExecuteNonQueryAsync();
    }

    private async Task EmployeeTableInit()
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = @"
            CREATE TABLE IF NOT EXISTS employees (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                sex VARCHAR(255) NOT NULL,
                department VARCHAR(255) NOT NULL,
                gmail VARCHAR(255) NOT NULL,
                code VARCHAR(255) NOT NULL,

                CONSTRAINT unique_gmail UNIQUE(gmail),
                CONSTRAINT unique_code UNIQUE(code),
                CONSTRAINT unique_name UNIQUE(name)
            )";

        MySqlCommand command = new MySqlCommand(query, conn);
        await command.ExecuteNonQueryAsync();
    }

    private async Task AttendanceTableInit()
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = @"
            CREATE TABLE IF NOT EXISTS attendance (
                id INT PRIMARY KEY AUTO_INCREMENT,
                fk_code VARCHAR(255) NOT NULL,
                attendance_date DATETIME NOT NULL,

                FOREIGN KEY (fk_code) REFERENCES employees(code)
                ON DELETE CASCADE
            )";
        MySqlCommand command = new MySqlCommand(query, conn);
        await command.ExecuteNonQueryAsync();
    }

    private async Task AdminTableInit()
    {
        MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = @"
            CREATE TABLE IF NOT EXISTS admin (
                username VARCHAR(255) NOT NULL,
                password VARCHAR(255) NOT NULL
            )
        ";

        MySqlCommand command = new MySqlCommand(query, conn);
        await command.ExecuteNonQueryAsync();
    }

    /*
    private async Task InsertAdminCredentials()
    {
        string username = Environment.GetEnvironmentVariable("admin_username") ?? throw new InvalidOperationException("ERROR: Missing admin username credentials");
        string password = Environment.GetEnvironmentVariable("admin_pass") ?? throw new InvalidOperationException("ERROR: Missing admin password credentials");

        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = "INSERT INTO admin VALUES (@username, @password);";

        MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(password).ToString());

        await command.ExecuteNonQueryAsync();
    }  */

    // put this to an endpoint and use it once
    public async Task InitializeDbAndTable()
    {
        await DatabaseInit();
        await EmployeeTableInit();
        await AttendanceTableInit();
        await AdminTableInit();
    }


    // just use once to initiate the admin credentials 

    /*
    public async Task AdminCredentialsInit()
    {
        await InsertAdminCredentials();
    }
    */

    

    public async Task<List<Employee>> SelectEmployeeData()
    {
        using MySqlConnection conn = new MySqlConnection();
        await conn.OpenAsync();

        List<Employee> employees = new List<Employee>();

        string query = @"SELECT * FROM employees";

        MySqlCommand command = new MySqlCommand(query, conn);
        var reader = await command.ExecuteReaderAsync();

        while (reader.Read())
        {
            Employee employee = new Employee();

            employee.Id = reader.GetInt32("id");
            employee.Name = reader.GetString("name");
            employee.Sex = reader.GetString("sex");
            employee.Department = reader.GetString("department");
            employee.Code = reader.GetString("code");

            employees.Add(employee);
        }

        return employees;
    }

    public async Task InsertEmployee(EmployeeDto employee) 
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = $@"
            INSERT INTO employees (name, sex, gmail, department, code)
            VALUES (@name, @sex, @gmail, @department, @code)
        ";

        MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@name", employee.Name);
        command.Parameters.AddWithValue("@sex", employee.Sex);
        command.Parameters.AddWithValue("@gmail", employee.Gmail);
        command.Parameters.AddWithValue("@department", employee.Department);
        command.Parameters.AddWithValue("@code", employee.Code);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsAdminValid(AdminDto unverifiedAdmin)
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = "SELECT * FROM admin";

        MySqlCommand command = new MySqlCommand(query, conn);
        var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException("ERROR: No admin credential has been found");
        }

        Admin admin = new Admin()
        {
            Username = reader.GetString("username"),
            Password = reader.GetString("password")
        };

        if (admin.Username != unverifiedAdmin.Username 
        || !BCrypt.Net.BCrypt.Verify(unverifiedAdmin.Password, admin.Password)) return false;

        return true;
    }

    


}