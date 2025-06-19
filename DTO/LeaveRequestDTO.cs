using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace RealTime_APIDev.DTO
{
    public class LeaveRequestDTO
    {
        [Required]
        public int EmployeeID { get; set; }
        [Required]
        public DateOnly? StartDate { get; set; }
        [Required]
        public DateOnly? Enddate { get; set; }
        [Required]
        public string Reason { get; set; }
    }
}
