using CivicAlert.Models;

namespace CivicAlert.DTOs
{
    public class IssueUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IssueStatus Status { get; set; }
        public int Severity { get; set; }
    }
}
