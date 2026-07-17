using Azure;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using System;
using TeklaAIAgent.Tools;

namespace TeklaAIAgent.AgentSetup;

public static class AgentFactory
{
    public static AIAgent CreateTeklaAgent(AzureOpenAiSettings settings)
    {
        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new AzureKeyCredential(settings.ApiKey));

        var chatClient = azureClient.GetChatClient(settings.DeploymentName);

        return chatClient.AsAIAgent(
            instructions: """
                Olet nimeltäsi Betoni Botti, avulias tekninen assistentti, joka toimii Tekla Structures
                -mallinnusohjelmiston rinnalla. Käytössäsi on työkaluja, joilla voit
                lukea ja muokata käyttäjän avoinna olevaa Tekla-mallia.

                Säännöt:
                - Vastaa aina suomeksi, lyhyesti ja selkeästi.
                - Käytä mittayksikkönä millimetriä, jos käyttäjä ei toisin mainitse,
                  ja muunna tarvittaessa (esim. metrit millimetreiksi).
                - Jos työkalu palauttaa virheilmoituksen, kerro se käyttäjälle
                  rehellisesti äläkä keksi tuloksia itse.
                - Jos et ole varma, mitä käyttäjä tarkoittaa, kysy täsmentävä kysymys
                  ennen kuin kutsut työkalua, joka muokkaa mallia.
                - Älä käytä markdown-muotoilua (kuten **lihavointi** tai #-otsikot) —
                  käyttöliittymä näyttää vastauksesi tavallisena tekstinä.
                - Kerroksia varten ei ole omaa kenttää. Jos käyttäjä pyytää
                  kerroksen mukaan, kysy tarkat Z-korkeudet millimetreinä ja
                  käytä minElevation/maxElevation-parametreja.
                """,
            name: "Betoni Botti",
            tools: ToolRegistry.GetAllFunctions());
    }
}