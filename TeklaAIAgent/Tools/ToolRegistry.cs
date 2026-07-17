using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.AI;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Kokoaa automaattisesti kaikki ITeklaTool-rajapinnan toteuttavat luokat
/// (mukaan lukien Tools/Private-kansiossa mahdollisesti olevat, gitignoretut
/// työkalut) yhdeksi listaksi agentille. Uuden työkalun lisääminen ei enää
/// vaadi mitään muutosta tähän tiedostoon — riittää, että luokka toteuttaa
/// ITeklaTool-rajapinnan jossain päin Tools-kansiota.
/// </summary>
public static class ToolRegistry
{
    public static IList<AITool> GetAllFunctions()
    {
        var toolTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ITeklaTool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        var all = new List<AITool>();
        foreach (var type in toolTypes)
        {
            if (Activator.CreateInstance(type) is ITeklaTool tool)
            {
                all.AddRange(tool.GetFunctions());
            }
        }

        return all;
    }
}