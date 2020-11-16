using System.Collections.Generic;
using LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Dtos;
using LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LAB_8_EMPLOYEE_SERVICE_ASP_CORE_V2.Controllers
{
    [Route("EmployeesService/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        // GET EmployeesService/employees
        [HttpGet]
        public ActionResult<IEnumerable<Employee>> Get()
        {
            return Logic.GetAll();
        }

        // GET EmployeesService/employees/Name1
        [HttpGet("{name}")]
        public ActionResult<Employee> Get(string name)
        {
            return Logic.Get(name);
        }

        // POST EmployeesService/employees
        [HttpPost]
        public void Post([FromBody] EmployeeDto employeeDto)
        {
            var age = 0;
            var experienceYears = 0;

            if (!int.TryParse(employeeDto.AgeStr, out age) ||
                !int.TryParse(employeeDto.ExperienceYearsStr, out experienceYears))
            {
                return;
            }

            var employee = new Employee()
            {
                Name = employeeDto.Name,
                Age = age,
                ExperienceYears = experienceYears,
                CompanyAddress = employeeDto.CompanyAddress
            };

            Logic.Insert(employee);
        }

        // PUT EmployeesService/employees
        [HttpPut]
        public void Put([FromBody] EmployeeDto employeeDto)
        {
            var age = 0;
            var experienceYears = 0;

            if (!int.TryParse(employeeDto.AgeStr, out age) ||
                !int.TryParse(employeeDto.ExperienceYearsStr, out experienceYears))
            {
                return;
            }

            var employee = new Employee()
            {
                Name = employeeDto.Name,
                Age = age,
                ExperienceYears = experienceYears,
                CompanyAddress = employeeDto.CompanyAddress
            };

            Logic.Update(employee);
        }

        // DELETE EmployeesService/employees/Name1
        [HttpDelete("{name}")]
        public void Delete(string name)
        {
            var result = Logic.Delete(name);
        }
    }
}
