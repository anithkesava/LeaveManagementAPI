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
        Task AddEmployeeDetails(EmployeeDTO employee);

        bool IsEmployeeNameAlreadyAdded(string name, string pname);

        bool IsEmployeeExists(int id);

        bool IsStartDateEndDateToday(DateOnly? startDate, DateOnly? endDate);

        bool IsStartDateEndDateAlreadyExists(LeaveRequestDTO leaveRequestDTO);

        Task ApplyEmployeeLeave(LeaveRequestDTO leaveRequestDTO);

        int? RemainingLeaveDays(int employeeID);

        LeaveRequest GetLeaveRequestDetails(int RequestID);

        bool IsAdminRequiresAction(int id);

        List<int> GetLeaveRequestIDsRequiredAction();

        Task AdminAction(AdminActionDTO adminActionDTO);

    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _appContext;

        public EmployeeService(AppDbContext appContext)
        {
            this._appContext = appContext;
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
                ProjectName = employee.ProjectName
            };

            await _appContext.Employees.AddAsync(emp);
            await _appContext.SaveChangesAsync();
        }

        public bool IsEmployeeExists(int id)
        {
            var employeeDetails = _appContext.Employees.FirstOrDefault(x => x.EmployeeID == id);

            if (employeeDetails != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsStartDateEndDateToday(DateOnly? startDate, DateOnly? endDate)
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            if (startDate == currentDate && endDate == currentDate)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        public LeaveRequest GetLeaveRequestDetails(int RequestID)
        {
            var leaveRequestDetails = _appContext.LeaveRequests.FirstOrDefault(x => x.RequestID == RequestID);
            if (leaveRequestDetails != null)
            {
                return leaveRequestDetails;
            }
            else
            {
                return new LeaveRequest();
            }
        }

        public bool IsAdminRequiresAction(int id)
        {
            var leaveDetails = _appContext.LeaveRequests.FirstOrDefault(x => x.RequestID == id);
            if (leaveDetails != null)
            {
                if (leaveDetails.AdminAction != "string")
                {
                    return true;
                }
                return false;
            }
            return false;

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
    }
}
