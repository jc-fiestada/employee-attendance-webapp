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

        using MySqlCommand command = new MySqlCommand(query, conn);
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

        using MySqlCommand command = new MySqlCommand(query, conn);
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
        using MySqlCommand command = new MySqlCommand(query, conn);
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

        using MySqlCommand command = new MySqlCommand(query, conn);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<string> SelectEmployeeCode(int userId)
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = "SELECT code FROM employees WHERE id = @id";

        using MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@id", userId);

        using MySqlDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new Exception("ERROR: User id not found");
        }

        return reader.GetString("code");
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

    

    public async Task<List<Employee>> SelectAllEmployee()
    {
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        List<Employee> employees = new List<Employee>();

        string query = "SELECT * FROM employees";

        using MySqlCommand command = new MySqlCommand(query, conn);
        using MySqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Employee employee = new Employee()
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                Sex = reader.GetString("sex"),
                Department = reader.GetString("department"),
                Code = reader.GetString("code"),
                Gmail = reader.GetString("gmail")
            };

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

        using MySqlCommand command = new MySqlCommand(query, conn);
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
        using MySqlCommand command = new MySqlCommand(query, conn);
        using MySqlDataReader reader = await command.ExecuteReaderAsync();

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

    public async Task<int> UpdateEmployee(string column, object value, int employeeId)
    {
        string[] columns = ["name", "sex", "department"];

        if (!columns.Contains(column))
        {
            throw new ArgumentException("Invalid column value");
        }

        if (column.Contains("sex"))
        {
            if (column != "male" && column != "female") throw new ArgumentException();
        }

        if (columns.Contains("department"))
        {
            string[] departments = ["it", "finance", "marketing", "customer service", "department manager"];
            if (!departments.Contains(column)) throw new ArgumentException(); 
        }

        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();

        string query = $@"
            UPDATE employees
            SET {column} = @value
            WHERE id = @id
        ";

        using MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@value", value);
        command.Parameters.AddWithValue("@id", employeeId);

        int affected = await command.ExecuteNonQueryAsync();
        return affected;
    }
    

    public async Task<string> DeleteEmployee(int employeeId)
    {
        string employeeCode = await SelectEmployeeCode(employeeId);
        using MySqlConnection conn = new MySqlConnection(_dbConn);
        await conn.OpenAsync();
        string query = "DELETE FROM employees WHERE id = @id";

        using MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@id", employeeId);

        int affected = await command.ExecuteNonQueryAsync();

        if (affected == 0) throw new InvalidOperationException("User not found");

        return employeeCode;
    }

    


}