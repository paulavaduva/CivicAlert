namespace CivicAlert.DTOs
{
    public class IssueDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; } 
        public string Severity { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserDto Reporter { get; set; }
        public UserDto? AssignedToUser { get; set; }
        public string CategoryName { get; set; }
        public string? ResolvedImageUrl { get; set; }
    }
}
