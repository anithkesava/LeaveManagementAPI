using Microsoft.AspNetCore.Mvc;
using RealTime_APIDev.DTO;
using RealTime_APIDev.Layer;
using RealTime_APIDev.Model;

namespace RealTime_APIDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        /*
         constraint: there will no direct logic will present inside the controller's action method everything will be done in a separate class like service
         */
        private readonly IEmployeeService _employeeService;

        public HomeController(IEmployeeService employeeService)
        {
            this._employeeService = employeeService;
        }

        [HttpPost("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDTO employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Missing of Required Data");
            }
            /*
             need to check whether the user aleady exists in the database or not
             */
            await _employeeService.AddEmployeeDetails(employee);
            return Ok(new { message = "Employee Details Added Successfully" });
        }

        [HttpPost("ApplyLeave")]
        public IActionResult ApplyLeave([FromBody] LeaveRequestDTO leaveRequestDTO)
        {
            if (!_employeeService.IsEmployeeExists(leaveRequestDTO.EmployeeID))
            {
                return NotFound(new { message = $"Employee with ID: {leaveRequestDTO.EmployeeID} not exist in the database" });
            }
            if (_employeeService.IsStartDateEndDateToday(leaveRequestDTO.StartDate, leaveRequestDTO.Enddate))
            {
                return BadRequest(new { message = "Start date and End date Cannot be today" });
            }
            _employeeService.ApplyEmployeeLeave(leaveRequestDTO);
            return Ok(new { msg = "Leave Applied Successfully" });
        }

        [HttpPost("AdminActionForLeaveRequest")]
        public IActionResult AdminActionForLeaveRequest([FromBody] )
        {

        }
    }
}
