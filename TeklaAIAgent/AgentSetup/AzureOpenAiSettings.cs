namespace TeklaAIAgent.AgentSetup;

public class AppConfig
{
    public AzureOpenAiSettings AzureOpenAI { get; set; } = new();
}

public class AzureOpenAiSettings
{
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string DeploymentName { get; set; } = "";
}