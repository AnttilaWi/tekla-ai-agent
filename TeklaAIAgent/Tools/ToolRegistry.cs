using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Kerää kaikki ITeklaTool-toteutukset yhdeksi listaksi, jonka AgentFactory
/// antaa agentille käyttöön.
/// </summary>
public static class ToolRegistry
{
    // <-- LISÄÄ UUDET TYÖKALUT TÄHÄN LISTAAN -->
    private static readonly ITeklaTool[] AllTools =
    [
        new AssemblyWeightTool(),
        new CreateBeamTool(),
        // new OmaUusiTyokalusi(),   <-- esimerkki siitä, miten seuraava työkalu lisätään
    ];

    public static IList<AITool> GetAllFunctions()
    {
        var all = new List<AITool>();
        foreach (var tool in AllTools)
        {
            all.AddRange(tool.GetFunctions());
        }
        return all;
    }
}