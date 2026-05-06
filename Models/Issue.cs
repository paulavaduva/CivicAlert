namespace CivicAlert.Models
{
    public enum IssueStatus
    {
        New,
        Solved
    }

    public class Issue
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public IssueStatus Status { get; set; }

        public string? ImageUrl { get; set; }
        public int Severity { get; set; } = 1;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }


    }
}
