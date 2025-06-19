using System.ComponentModel.DataAnnotations;

namespace RealTime_APIDev.Model
{
    public class LeaveRequest
    {
        [Key]
        public int RequestID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public double TotalDays { get; set; }
        public string Reason { get; set; }
        public DateOnly AppliedDate { get; set; }
        public string? AdminAction { get; set; }
        public DateOnly? ActionTakenDate { get; set; }
    }
}
