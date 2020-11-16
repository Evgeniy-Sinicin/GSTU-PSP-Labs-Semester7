using LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2
{
    public class Logic
    {
        private static SqlConnection _con;

        static Logic()
        {
            _con = new SqlConnection("Data Source=DESKTOP-6EDE3MB;Initial Catalog=employeesdb;Integrated Security=False;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=true;Trusted_Connection=true;");
            _con.Open();
        }

        public static string Insert(Employee emp)
        {
            var msg = string.Empty;

            var cmd = new SqlCommand("Insert into EmployeeTable (Name, Age, ExperienceYears, CompanyAddress) values(@Name, @Age, @ExperienceYears, @CompanyAddress)", _con);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Age", emp.Age);
            cmd.Parameters.AddWithValue("@ExperienceYears", emp.ExperienceYears);
            cmd.Parameters.AddWithValue("@CompanyAddress", emp.CompanyAddress);

            if (cmd.ExecuteNonQuery() == 1)
            {
                msg = "Successfully Inserted";
            }
            else
            {
                msg = "Failed to insert";
            }

            return msg;
        }

        public static List<Employee> GetAll()
        {
            var cmd = new SqlCommand("Select * from EmployeeTable", _con);
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable("Employees");
            adapter.Fill(table);

            var employees = new List<Employee>();

            foreach (DataRow row in table.Rows)
            {
                employees.Add(new Employee()
                {
                    Name = (string)row.ItemArray[1],
                    Age = (int)row.ItemArray[2],
                    ExperienceYears = (int)row.ItemArray[3],
                    CompanyAddress = (string)row.ItemArray[4]
                });
            }

            return employees;
        }

        public static Employee Get(string name)
        {
            return GetAll().FirstOrDefault(x => x.Name.Equals(name));
        }

        public static string Update(Employee emp)
        {
            var msg = string.Empty;
            var cmd = new SqlCommand("Update EmployeeTable set Age = @Age, ExperienceYears = @ExperienceYears, CompanyAddress = @CompanyAddress where Name = @Name", _con);
            cmd.Parameters.AddWithValue("@Name", emp.Name);
            cmd.Parameters.AddWithValue("@Age", emp.Age);
            cmd.Parameters.AddWithValue("@ExperienceYears", emp.ExperienceYears);
            cmd.Parameters.AddWithValue("@CompanyAddress", emp.CompanyAddress);

            if (cmd.ExecuteNonQuery() > 0)
            {
                msg = "Successfully Updated";
            }
            else
            {
                msg = "Failed to update. Couldn't find employee by name";
            }

            return msg;
        }

        public static string Delete(string name)
        {
            var msg = string.Empty;
            var cmd = new SqlCommand("Delete EmployeeTable where Name = @Name", _con);
            cmd.Parameters.AddWithValue("@Name", name);

            if (cmd.ExecuteNonQuery() > 0)
            {
                msg = "Successfully Deleted";
            }
            else
            {
                msg = "Failed to delete. Couldn't find employee by name";
            }

            return msg;
        }
    }
}
