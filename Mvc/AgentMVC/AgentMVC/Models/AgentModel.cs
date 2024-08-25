using System.ComponentModel.DataAnnotations;

namespace AgentMVC.Models
{
    public enum AgentStatus
    {
        Active,
        InActive
    }
    public class AgentModel
    {
        public long Id { get; set; }
        public string NickName { get; set; }
        public string Image { get; set; }
        public int XPosition { get; set; } = -1;
        public int YPosition { get; set; } = -1;
        public AgentStatus AgentStatus { get; set; } = AgentStatus.InActive;
    }
}