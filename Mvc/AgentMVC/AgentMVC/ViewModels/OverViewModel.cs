namespace AgentMVC.ViewModels
{
    public class OverViewModel
    {
        public int AgentAmount { get; set; }
        public int ActiveAgents { get; set; }
        public int TargetsAmount { get; set; }
        public int ActiveTargets { get; set; }
        public int MissionsAmount { get; set; }
        public int ActiveMissions { get; set; }
        public double RelativeAgentsToTargets { get; set; }
        public double RelativeAvailableAgentsToTargets { get; set; }

    }
}
