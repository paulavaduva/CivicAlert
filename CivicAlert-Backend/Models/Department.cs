namespace CivicAlert.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
