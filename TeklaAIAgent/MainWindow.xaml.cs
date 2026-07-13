using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Agents.AI;
using TeklaAIAgent.AgentSetup;

namespace TeklaAIAgent;

public partial class MainWindow : Window
{
    private readonly AIAgent _agent;
    private AgentSession? _session;
    private readonly ObservableCollection<ChatMessage> _messages = new();

    public MainWindow()
    {
        InitializeComponent();
        ChatItemsControl.ItemsSource = _messages;

        var settings = LoadSettings();
        _agent = AgentFactory.CreateTeklaAgent(settings);

        AppendToChat("Betoni Botti",
            "Hei! Olen valmis auttamaan Tekla-mallin kanssa. Kokeile esimerkiksi:\n" +
            "• \"Listaa valittujen kokoonpanojen painot\"\n" +
            "• \"Luo palkki pisteestä 0,0,0 pisteeseen 5000,0,0\"", isUser: false);
    }

    private static AzureOpenAiSettings LoadSettings()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        string json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        return config.AzureOpenAI;
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void UserInputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        string message = UserInputBox.Text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        UserInputBox.Text = "";
        AppendToChat("Sinä", message, isUser: true);

        SendButton.IsEnabled = false;
        UserInputBox.IsEnabled = false;
        try
        {
            // "Session" pitää muistissa keskusteluhistorian. Luodaan se vasta
            // ensimmäisellä viestillä, koska sen luonti on nyt oma asynkroninen
            // Azure-kutsunsa, eikä sitä voi tehdä suoraan konstruktorissa.
            _session ??= await _agent.CreateSessionAsync();

            var response = await _agent.RunAsync(message, _session);
            AppendToChat("Betoni Botti", response.Text, isUser: false);
        }
        catch (Exception ex)
        {
            AppendToChat("Virhe", "Jotain meni pieleen: " + ex.Message, isUser: false);
        }
        finally
        {
            SendButton.IsEnabled = true;
            UserInputBox.IsEnabled = true;
            UserInputBox.Focus();
        }
    }

    private void AppendToChat(string sender, string text, bool isUser)
    {
        _messages.Add(new ChatMessage { Sender = sender, Text = text, IsUser = isUser });
        Dispatcher.InvokeAsync(() => ChatScrollViewer.ScrollToEnd(), DispatcherPriority.Background);
    }
}