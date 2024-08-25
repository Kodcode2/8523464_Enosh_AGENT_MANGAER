namespace AgentMVC.ViewModels
{
    public class MissionVM
    {
        public string AgentName { get; set; }
        public int XAgent {  get; set; }
        public int YAgent {  get; set; }
        public string TargetName { get; set; }
        public int XTarget { get; set; }
        public int YTarget { get; set; }
        public double Distance { get; set; }
        public double RemainingTime { get; set; }
    }
}
