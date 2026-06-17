namespace RestService.DependencyInjection
{
    /// <summary>
    /// Class containing the structure of the expected config section
    /// </summary>
    public class UnleashConfig
    {
        public static readonly string SectionName = "Unleash";
        
        public string Project { get; set; }
        public string AppName { get; set; }
        public string InstanceTag { get; set; }
        public string UnleashApi { get; set; }
        public string Token { get; set; }
        public int FetchTogglesInterval { get; set; }
    }

}