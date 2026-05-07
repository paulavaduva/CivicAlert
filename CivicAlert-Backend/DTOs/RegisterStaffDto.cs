namespace CivicAlert.DTOs
{
    public class RegisterStaffDto : RegisterDto
    {
        public string Role { get; set; } 
        public int? DepartmentId { get; set; }
    }
}
