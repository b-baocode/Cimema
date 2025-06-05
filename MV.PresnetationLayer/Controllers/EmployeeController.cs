using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;
using System.ComponentModel.DataAnnotations;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetEmployees(
            [FromQuery] EmployeeSearchRequest request)
        {
            var result = await _employeeService.GetEmployeesAsync(request);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<EmployeeResponse>>> SearchEmployees(
            [FromQuery] EmployeeSearchRequest request)
        {
            // Both Admin and Manager can see all employees
            //request.IsManager = false; // Show all employees (both managers and regular employees)

            var result = await _employeeService.GetEmployeesAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployee(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound();

            // Check if current user is Manager and trying to view a Manager
            if (User.IsInRole("Manager") && employee.Role == "Manager")
            {
                return Forbid("Managers can only view regular employees");
            }

            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployee(
            [FromBody] EmployeeCreateRequest request)
        {
            try
            {
                // Check if current user is Manager and trying to create a Manager
                if (User.IsInRole("Manager") && request.RoleId == 2) // Assuming 2 is Manager role ID
                {
                    return Forbid("Managers can only create regular employees");
                }

                var employee = await _employeeService.CreateEmployeeAsync(request);
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.UserId }, employee);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<EmployeeResponse>> UpdateEmployee(
            string id, [FromBody] EmployeeUpdateRequest request)
        {
            try
            {
                // Get current employee to check role
                var currentEmployee = await _employeeService.GetEmployeeByIdAsync(id);
                if (currentEmployee == null)
                    return NotFound();

                // Check if current user is Manager
                if (User.IsInRole("Manager"))
                {
                    // Manager can't update Managers
                    if (currentEmployee.Role == "Manager")
                        return Forbid("Managers can only update regular employees");

                    // Manager can't change role to Manager
                    if (request.RoleId == 2)
                        return Forbid("Managers can't change role to Manager");
                }

                var employee = await _employeeService.UpdateEmployeeAsync(id, request);
                return Ok(employee);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteEmployee(string id)
        {
            try
            {
                // Get current employee to check if exists
                var currentEmployee = await _employeeService.GetEmployeeByIdAsync(id);
                if (currentEmployee == null)
                    return NotFound();

                await _employeeService.DeleteEmployeeAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}