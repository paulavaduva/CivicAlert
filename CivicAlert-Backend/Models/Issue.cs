namespace CivicAlert.Models
{
    public enum IssueStatus
    {
        Pending,   
        Validated,  
        Assigned,   
        InProgress, 
        Solved,     
        Rejected    
    }

    public enum IssueSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }

    public class Issue
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address {  get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public IssueStatus Status { get; set; }

        public string? ImageUrl { get; set; }
        public IssueSeverity Severity { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string UserId { get; set; }
        public User Reporter { get; set; }

        public string? DispatcherId { get; set; }
        public User? Dispatcher { get; set; }
        public string? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }
        public string? ResolvedImageUrl { get; set; } 

    }
}
