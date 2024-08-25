using System.ComponentModel.DataAnnotations;

namespace AgentRest.Models
{
        public enum AgentStatus
        {
            Active,
            InActive
        }
    public class AgentModel
    {
        public long Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string NickName { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 3)]
        public required string Image { get; set; }
        public int XPosition { get; set; } = -1;
        public int YPosition { get; set; } = -1;
        public AgentStatus AgentStatus { get; set; } = AgentStatus.InActive;
    }
}
