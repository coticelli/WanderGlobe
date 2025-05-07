namespace WanderGlobe.Models
{
    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Criteria { get; set; }
        public List<UserBadge> Users { get; set; } = new List<UserBadge>();
    }
}
