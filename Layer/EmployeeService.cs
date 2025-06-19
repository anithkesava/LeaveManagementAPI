using Microsoft.AspNetCore.Mvc.ModelBinding;
using RealTime_APIDev.DTO;
using RealTime_APIDev.Entity;
using RealTime_APIDev.Model;

namespace RealTime_APIDev.Layer
{
    public interface IEmployeeService
    {
        Task AddEmployeeDetails(EmployeeDTO employee);

        //we need to create a method for apply leave. 

        bool IsEmployeeExists(int id);

        bool IsStartDateEndDateToday(DateOnly? startDate, DateOnly? endDate);

        Task ApplyEmployeeLeave(LeaveRequestDTO leaveRequestDTO);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _appContext;

        public EmployeeService(AppDbContext appContext)
        {
            this._appContext = appContext;
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

        public async Task ApplyEmployeeLeave(LeaveRequestDTO leaveRequestDTO)
        {
            if (leaveRequestDTO.EmployeeID != 0 && leaveRequestDTO.Reason != "string" &&
                leaveRequestDTO.StartDate != null && leaveRequestDTO.Enddate != null)
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

                    await _appContext.LeaveRequests.AddAsync(leaveRequest);
                    await _appContext.SaveChangesAsync();
                }
            }
        }
    }
}
