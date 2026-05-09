using CivicAlert.Models;

namespace CivicAlert.DTOs
{
    public class IssueCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IssueSeverity Severity { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; } 
    }
}
