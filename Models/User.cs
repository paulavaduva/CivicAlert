using Microsoft.AspNetCore.Identity;

namespace CivicAlert.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Issue> Issues { get; set; }
    }
}
