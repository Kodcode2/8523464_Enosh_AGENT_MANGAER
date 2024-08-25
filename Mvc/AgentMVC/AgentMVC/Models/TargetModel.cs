using System.ComponentModel.DataAnnotations;

namespace AgentMVC.Models
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
        public string Name { get; set; }
        public string Image { get; set; }
        public string Role { get; set; }
        public int XPosition { get; set; } = -1;
        public int YPosition { get; set; } = -1;
        public TargetStatus TargetStatus { get; set; } = TargetStatus.Alive;
    }
}