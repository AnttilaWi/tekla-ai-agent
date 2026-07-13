namespace TeklaAIAgent;

public class ChatMessage
{
    public string Sender { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsUser { get; set; }
}