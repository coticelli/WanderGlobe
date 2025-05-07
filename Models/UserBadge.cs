namespace WanderGlobe.Models
{
    public class UserBadge
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int BadgeId { get; set; }
        public Badge Badge { get; set; }

        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    }
}
