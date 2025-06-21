using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RealTime_APIDev.DTO;
using RealTime_APIDev.Entity;
using RealTime_APIDev.Model;
namespace RealTime_APIDev.Layer
{
    public interface IEmployeeService
    {
        bool ValidEmployee(EmployeeDTO employee);
        Task AddEmployeeDetails(EmployeeDTO employee);
        bool IsEmployeeNameAlreadyAdded(string name, string pname);
        bool IsEmployeeExists(int id);
        bool IsStartDateEndDateToday(DateOnly? startDate, DateOnly? endDate);
        bool IsStartDateEndDateAlreadyExists(LeaveRequestDTO leaveRequestDTO);
        Task ApplyEmployeeLeave(LeaveRequestDTO leaveRequestDTO);
        int? RemainingLeaveDays(int employeeID);
        LeaveRequest? GetLeaveRequestDetails(int RequestID);
        bool IsAdminRequiresAction(int id);
        List<int> GetLeaveRequestIDsRequiredAction();
        Task AdminAction(AdminActionDTO adminActionDTO);
        bool IsAdminActionValid(string? adminAction);
        List<LeaveRequest> GetLeaveRequestByFilters(LeaveRequestFilterDTO leaveRequestFilterDTO);
    }
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _appContext;
        public EmployeeService(AppDbContext appContext)
        {
            this._appContext = appContext;
        }
        public bool ValidEmployee(EmployeeDTO employee)
        {
            var employeeDetails = _appContext.Employees.ToList();
            if (employeeDetails != null)
            {
                return employeeDetails.Any(x => x.EmployeeName == employee.EmployeeName && x.EmployeeRole == employee.EmployeeRole && x.ProjectName == employee.ProjectName);
            }
            return false;
        }
        public bool IsEmployeeNameAlreadyAdded(string name, string pname)
        {
            var employeeDetails = _appContext.Employees.ToList();
            return employeeDetails.Exists(x => x.EmployeeName == name && x.ProjectName == pname);
        }
        public async Task AddEmployeeDetails(EmployeeDTO employee)
        {
            Employee emp = new Employee
            {
                EmployeeName = employee.EmployeeName,
                ProjectName = employee.ProjectName,
                EmployeeRole = employee.EmployeeRole
            };
            await _appContext.Employees.AddAsync(emp);
            await _appContext.SaveChangesAsync();
        }
        public bool IsEmployeeExists(int id) => _appContext.Employees.Any(x => x.EmployeeID == id);
        public bool IsStartDateEndDateToday(DateOnly? startDate, DateOnly? endDate)
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            return (startDate == currentDate && endDate == currentDate) ? true : false;
        }
        public bool IsStartDateEndDateAlreadyExists(LeaveRequestDTO leaveRequestDTO)
        {
            List<DateOnly?> leaveDates = new List<DateOnly?>();
            var employeeLeaveDetails = _appContext.LeaveRequests.Where(x => x.EmployeeID == leaveRequestDTO.EmployeeID).ToList();
            foreach (var leave in employeeLeaveDetails)
            {
                leaveDates.Add(leave.StartDate);
                leaveDates.Add(leave.EndDate);
            }
            leaveDates = leaveDates.Distinct().ToList();
            if (leaveDates.Contains(leaveRequestDTO.StartDate) || leaveDates.Contains(leaveRequestDTO.Enddate))
            {
                return true;
            }
            return false;
        }
        public async Task ApplyEmployeeLeave(LeaveRequestDTO leaveRequestDTO)
        {
            if (leaveRequestDTO.EmployeeID > 0 && leaveRequestDTO.Reason != "string" &&
                leaveRequestDTO.StartDate <= leaveRequestDTO.Enddate)
            {
                /*our goal is to insert all the non nullable values from the LeaveRequest*/
                var employeeDetails = _appContext.Employees.FirstOrDefault(x => x.EmployeeID == leaveRequestDTO.EmployeeID);
                if (employeeDetails != null)
                {
                    LeaveRequest leaveRequest = new LeaveRequest
                    {
                        EmployeeID = leaveRequestDTO.EmployeeID,
                        EmployeeName = employeeDetails.EmployeeName,
                        StartDate = leaveRequestDTO.StartDate,
                        EndDate = leaveRequestDTO.Enddate,
                        TotalDays = (leaveRequestDTO.Enddate.Value.Day - leaveRequestDTO.StartDate.Value.Day) + 1,
                        Reason = leaveRequestDTO.Reason,
                        AppliedDate = DateOnly.FromDateTime(DateTime.Now)
                    };
                    employeeDetails.TotalLeave = 20;
                    if (employeeDetails.LeaveTaken == null)
                    {
                        employeeDetails.LeaveTaken = 0;
                    }
                    employeeDetails.LeaveTaken += (int)leaveRequest.TotalDays;
                    employeeDetails.LeaveRemaining = employeeDetails.TotalLeave - employeeDetails.LeaveTaken;
                    await _appContext.LeaveRequests.AddAsync(leaveRequest);
                    await _appContext.SaveChangesAsync();
                }
            }
        }
        public int? RemainingLeaveDays(int employeeID)
        {
            var employee = _appContext.Employees.FirstOrDefault(x => x.EmployeeID == employeeID);
            return employee?.LeaveRemaining;
        }
        public LeaveRequest? GetLeaveRequestDetails(int RequestID)
        {
            return _appContext.LeaveRequests.FirstOrDefault(x => x.RequestID == RequestID);
        }
        public bool IsAdminRequiresAction(int id)
        {
            var leaveDetails = _appContext.LeaveRequests.FirstOrDefault(x => x.RequestID == id);
            return (leaveDetails?.AdminAction != "string") ? true : false;
        }
        public List<int> GetLeaveRequestIDsRequiredAction()
        {
            List<int> RequestIDs = new List<int>();
            var leaveRequest = _appContext.LeaveRequests.ToList();
            var leaveRequestDetails = leaveRequest.Where(x => x.AdminAction == null).ToList();
            if (leaveRequestDetails.Count > 0)
            {
                foreach (var leave in leaveRequestDetails)
                {
                    RequestIDs.Add(leave.RequestID);
                }
                return RequestIDs;
            }
            return new List<int>();
        }
        public bool IsAdminActionValid(string? adminAction) => adminAction == "approve" || adminAction == "reject";
        public async Task AdminAction(AdminActionDTO adminActionDTO)
        {
            var leaveRequest = _appContext.LeaveRequests.FirstOrDefault(x => x.RequestID == adminActionDTO.RequestID);
            if (leaveRequest != null)
            {
                leaveRequest.AdminAction = adminActionDTO.AdminAction;
                leaveRequest.ActionTakenDate = DateOnly.FromDateTime(DateTime.Now);
                await _appContext.SaveChangesAsync();
            }
        }
        public List<LeaveRequest> GetLeaveRequestByFilters(LeaveRequestFilterDTO leaveRequestFilterDTO)
        {
            var leaveRequestDetails = _appContext.LeaveRequests.AsQueryable();
            if (leaveRequestFilterDTO != null)
            {
                if (leaveRequestFilterDTO.Month.HasValue)
                {
                    leaveRequestDetails = leaveRequestDetails.Where(x => x.StartDate.HasValue && x.StartDate.Value.Month == leaveRequestFilterDTO.Month.Value);
                }
                if (leaveRequestFilterDTO.Year.HasValue)
                {
                    leaveRequestDetails = leaveRequestDetails.Where(x => x.StartDate.HasValue && x.StartDate.Value.Year == leaveRequestFilterDTO.Year.Value);
                }
                if (!string.IsNullOrWhiteSpace(leaveRequestFilterDTO.AdminStatus))
                {
                    leaveRequestDetails = leaveRequestDetails.Where(x => x.AdminAction == leaveRequestFilterDTO.AdminStatus);
                }
                return leaveRequestDetails.ToList();
            }
            else
            {
                return new List<LeaveRequest>();
            }
        }
    }
}
