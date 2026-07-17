using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Yhteinen rajapinta (interface) kaikille agentin työkaluille.
/// Tämä on koko arkkitehtuurin "plugin-pohja".
///
/// UUDEN TYÖKALUN LISÄÄMINEN — 2 vaihetta:
///   1) Luo uusi .cs-tiedosto Tools-kansioon (tai Tools/Private-alikansioon,
///      jos työkalu ei kuulu julkiseen versioon).
///   2) Kirjoita luokka, joka toteuttaa ITeklaTool-rajapinnan
///      (kopioi pohjaksi esim. CreateBeamTool.cs).
///
/// Siinä kaikki — ToolRegistry löytää uuden luokan automaattisesti reflectionilla.
/// Ei tarvitse koskea ToolRegistry.cs-, MainWindow.xaml.cs- tai AgentFactory.cs-tiedostoihin.
/// </summary>
public interface ITeklaTool
{
    /// <summary>
    /// Palauttaa tämän työkalun tarjoamat funktiot agentin käytettäväksi.
    /// Yksi luokka voi tarjota useamman funktion, jos se on tarkoituksenmukaista.
    /// </summary>
    IEnumerable<AITool> GetFunctions();
}