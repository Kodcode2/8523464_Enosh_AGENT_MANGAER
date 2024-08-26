namespace AgentMVC.ViewModels
{
    public class MissionVM
    {
        public long Id { get; set; }
        public string AgentName { get; set; }
        public int XAgent {  get; set; }
        public int YAgent {  get; set; }
        public string TargetName { get; set; }
        public int XTarget { get; set; } = -1;
        public int YTarget { get; set; } = -1;
        public string Status { get; set; }
        public double Distance { get; set; }
        public double RemainingTime { get; set; }
    }
}
