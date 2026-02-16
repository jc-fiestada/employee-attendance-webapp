using MySqlConnector;
using EmployeeAttendance.Models.Dto;
using EmployeeAttendance.Models.Entities;

namespace EmployeeAttendance.Services.Database;

public class MysqlDb
{

    private readonly string _serverConn; // use only to generate db
    private readonly string _dbConn; // use this for db connection

    public MysqlDb(string key)
    {
        string _dbPassword = Environment.GetEnvironmentVariable(key) ?? throw new Exception("ERROR: database key missing");

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
                name VARCHAR(255) NOT NULL UNIQUE,
                sex VARCHAR(255) NOT NULL,
                department VARCHAR(255) NOT NULL,
                code VARCHAR(255) NOT NULL,

                CONSTRAINT unique_code UNIQUE(code)
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

    // put this to an endpoint and use it once
    public async Task InitializeDbAndTable()
    {
        await DatabaseInit();
        await EmployeeTableInit();
        await AttendanceTableInit();
        await AdminTableInit();
    }

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
            INSERT INTO employees (name, sex, department, code)
            VALUES (@name, @sex, @department, @code)
        ";

        MySqlCommand command = new MySqlCommand(query, conn);
        command.Parameters.AddWithValue("@name", employee.Name);
        command.Parameters.AddWithValue("@sex", employee.Sex);
        command.Parameters.AddWithValue("@department", employee.Department);
        command.Parameters.AddWithValue("@code", employee.Code);

        await command.ExecuteNonQueryAsync();
    }

    


}