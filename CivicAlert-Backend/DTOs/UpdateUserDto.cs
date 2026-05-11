namespace CivicAlert.DTOs
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; 
        public int? DepartmentId { get; set; } 
    }
}