
namespace PlayerService.Models
{
    public class Player
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Rating { get; set; } = 1000;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
