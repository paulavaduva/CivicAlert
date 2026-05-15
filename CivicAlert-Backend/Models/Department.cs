using System.Text.Json.Serialization;

namespace CivicAlert.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Category> Categories { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
