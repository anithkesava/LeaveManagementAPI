using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTime_APIDev.DTO;
using RealTime_APIDev.JWT;
using RealTime_APIDev.Layer;
using RealTime_APIDev.Model;
using System;

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
        private readonly GenerateToken _generateToken;

        public HomeController(IEmployeeService employeeService, GenerateToken generateToken)
        {
            this._employeeService = employeeService;
            this._generateToken = generateToken;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(EmployeeDTO employeeDTO)
        {
            if (_employeeService.ValidEmployee(employeeDTO))
            {
                var token = _generateToken.generateToken(employeeDTO);
                return Ok(token);
            }
            return NotFound(new { message = "Employee not Found. Invalid data" });
        }

        [Authorize(Roles ="Manager")]
        [HttpPost("CreateEmployee")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDTO employee)
        {
            /*
             check whether the same employee added twice ??             
             */

            if (_employeeService.IsEmployeeNameAlreadyAdded(employee.EmployeeName, employee.ProjectName))
            {
                return Conflict(new { msg = "Employee already exists" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Missing of Required Data");
            }
            /*
             need to check whether the user aleady exists in the database or not
             */
            await _employeeService.AddEmployeeDetails(employee);
            return Ok(new { message = "Employee Details Created" });
        }

        [Authorize]
        [HttpPost("ApplyLeave")]
        public IActionResult ApplyLeave([FromBody] LeaveRequestDTO leaveRequestDTO)
        {
            if (!_employeeService.IsEmployeeExists(leaveRequestDTO.EmployeeID))
            {
                return NotFound(new { message = $"Employee with ID: {leaveRequestDTO.EmployeeID} not exist in the database" });
            }

            if (_employeeService.IsStartDateEndDateAlreadyExists(leaveRequestDTO))
            {
                return Conflict(new { msg = "Leave already has been taken by you in the same date" });
            }

            if (_employeeService.IsStartDateEndDateToday(leaveRequestDTO.StartDate, leaveRequestDTO.Enddate))
            {
                return BadRequest(new { message = "Start date and End date Cannot be today" });
            }
            _employeeService.ApplyEmployeeLeave(leaveRequestDTO);
            return Ok(new { msg = "Leave Applied Successfully" });
        }

        [Authorize]
        [HttpGet("GetLeaveBalance")]
        public IActionResult GetLeaveBalance(int employeeID)
        {
            if (_employeeService.IsEmployeeExists(employeeID))
            {

                int? remainingDays = _employeeService.RemainingLeaveDays(employeeID);
                return Ok(new { leavebalance = $"{remainingDays}" });
            }
            else
            {
                return Conflict(new { message = $"invalid employeeID: {employeeID}" });
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("GetRequestIDRequiresAction")]
        public IActionResult GetRequestIDRequiresAction()
        {
            var actionRequireRequestIDs = _employeeService.GetLeaveRequestIDsRequiredAction();
            if (actionRequireRequestIDs.Count > 0)
            {
                return Ok(actionRequireRequestIDs);
            }
            return Ok(new { msg = "no action is pending from admin " });
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("GetLeaveDetails")]
        public IActionResult GetLeaveDetails(int requestid)
        {
            if (_employeeService.GetLeaveRequestDetails(requestid) != null)
            {
                var leaveDetails = _employeeService.GetLeaveRequestDetails(requestid);
                return Ok(leaveDetails);
            }
            else
            {
                return Conflict(new { msg = $"RequestID : {requestid} is invalid" });
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("AdminActionForLeaveRequest")]
        public IActionResult AdminActionForLeaveRequest([FromBody] AdminActionDTO adminActionDTO)
        {
            if (_employeeService.IsAdminRequiresAction(adminActionDTO.RequestID))
            {
                if (_employeeService.IsAdminActionValid(adminActionDTO.AdminAction))
                {
                    _employeeService.AdminAction(adminActionDTO);
                    return Ok(new { msg = "Required Action Taken by Admin" });
                }
                return Conflict(new { msg = "invalid action. approve or reject needs to be perform" });
            }
            return BadRequest(new { msg = "No Action Required by Admin for this Request ID" });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("FilterLeaveRequest")]
        public IActionResult FilterLeaveRequest(LeaveRequestFilterDTO leaveRequestFilterDTO)
        {
            var leaveRequestDetails = _employeeService.GetLeaveRequestByFilters(leaveRequestFilterDTO);
            if (leaveRequestDetails.Count > 0)
            {
                return Ok(leaveRequestDetails);
            }
            else
            {
                return Conflict("There will be no data in the Selected Range");
            }
        }
    }
}
