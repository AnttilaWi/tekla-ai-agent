using System.Collections.Generic;
using Microsoft.Extensions.AI;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Yhteinen rajapinta (interface) kaikille agentin työkaluille.
/// Tämä on koko arkkitehtuurin "plugin-pohja".
///
/// UUDEN TYÖKALUN LISÄÄMINEN — 3 vaihetta:
///   1) Luo uusi .cs-tiedosto tähän Tools-kansioon.
///   2) Kirjoita luokka, joka toteuttaa ITeklaTool-rajapinnan
///      (kopioi pohjaksi esim. CreateBeamTool.cs).
///   3) Lisää uusi luokka ToolRegistry.cs-tiedoston AllTools-listaan.
///
/// Muuta ei tarvita — agentti löytää ja osaa käyttää uutta työkalua automaattisesti,
/// eikä esim. MainWindow.xaml.cs-tiedostoa tai AgentFactory.cs:ää tarvitse koskea.
/// </summary>
public interface ITeklaTool
{
    /// <summary>
    /// Palauttaa tämän työkalun tarjoamat funktiot agentin käytettäväksi.
    /// Yksi luokka voi tarjota useamman funktion, jos se on tarkoituksenmukaista.
    /// </summary>
    IEnumerable<AITool> GetFunctions();
}