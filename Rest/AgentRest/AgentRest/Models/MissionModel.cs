namespace AgentRest.Models
{
    public enum Status
    {
        Propose,
        Active,
        Ended
    }
    public class MissionModel
    {
        public long Id { get; set; }
        public long TargetId { get; set; }
        public long AgentId { get; set; }
        public AgentModel? Agent { get; set; }
        public TargetModel? Target { get; set; }
        public double RemainingTime { get; set; }
        public double ExecutionTime { get; set; }
        public Status Status { get; set; } = Status.Propose;
    }
}
