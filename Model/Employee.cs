using System.ComponentModel.DataAnnotations;

namespace RealTime_APIDev.Model
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        [Required]
        public string ProjectName { get; set; }

        public int? TotalLeave { get; set; }

        public int? LeaveTaken { get; set; }

        public int? LeaveRemaining { get; set; }
    }
}
