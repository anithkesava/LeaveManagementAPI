namespace RealTime_APIDev.DTO
{
    public class AdminActionDTO
    {
        public int RequestID { get; set; }

        public string AdminAction { get; set; }

        public DateOnly? ActionTakenDate { get; set; }
    }
}
