using System.ComponentModel.DataAnnotations;

namespace AgentRest.Models
{
        public enum TargetStatus
        {
            Alive,
            Hunted,
            Dead
        }
    public class TargetModel
    {
        public long Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string Name { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 3)]
        public required string Image { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string Role { get; set; }
        public int XPosition { get; set; } = -1;
        public int YPosition { get; set; } = -1;
        public TargetStatus TargetStatus { get; set; } = TargetStatus.Alive;
    }
}
