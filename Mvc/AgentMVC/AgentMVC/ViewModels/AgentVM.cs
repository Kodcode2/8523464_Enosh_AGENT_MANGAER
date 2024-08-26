namespace AgentMVC.ViewModels
{
    public class AgentVM
    {
        public long Id { get; set; }
        public string NickName { get; set; }
        public string Image { get; set; }
        public int XPosition { get; set; } = -1;
        public int YPosition { get; set; } = -1;
        public string Status { get; set; }
        public string Mission { get; set; } = "";
        public double RemainingTime { get; set; }
        public int killAmount { get; set; }
    }
}
