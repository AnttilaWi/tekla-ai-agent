using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.AI;
using Tekla.Structures.Model;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Työkalu: Etsii mallista kokoonpanoja (assembly) annetuilla kriteereillä ja
/// asettaa ne valituiksi Tekla Structuresin mallinäkymässä — täsmälleen niin kuin
/// käyttäjä olisi klikannut ne itse. Valinta tehdään aina assembly-tasolla, jotta
/// muut työkalut (esim. painolistaus) voivat toimia sen päällä suoraan.
/// </summary>
public class SelectObjectsTool : ITeklaTool
{
    public IEnumerable<AITool> GetFunctions()
    {
        yield return AIFunctionFactory.Create(SelectObjectsByCriteria);
    }

    [Description(
        "Etsii Tekla Structures -mallista kokoonpanoja (assembly) annetuilla " +
        "kriteereillä ja valitsee ne mallinäkymässä assembly-tasolla — sopii " +
        "suoraan jatkoketjutukseen muiden työkalujen kanssa (esim. painolistaus). " +
        "Käytä tätä ennen muita työkaluja, kun käyttäjä pyytää toimintoa objekteille, " +
        "joita ei ole vielä valittu käsin. Jos käyttäjä ei mainitse objektityyppiä, " +
        "jätä objectType antamatta — haetaan silloin kaikista tuetuista tyypeistä.")]
    private string SelectObjectsByCriteria(
        [Description(
            "Valinnainen objektityyppi: 'Beam' (palkki), 'Column' (pilari), tai " +
            "'Plate' (laatta/levy/slab). Jätä antamatta, jos käyttäjä ei mainitse tyyppiä.")]
        string? objectType = null,
        [Description("Osan nimen sisältämä teksti, esim. 'PARVEKELAATTA'. Jätä tyhjäksi, jos nimeä ei rajata.")]
        string? namePattern = null,
        [Description("Elementtitunnuksen (assembly position) etuliite, esim. 'CX' täsmää 'CX-403' mutta ei 'CXU-105'. Jätä tyhjäksi, jos ei rajata.")]
        string? positionPattern = null,
        [Description("Profiilin sisältämä teksti, esim. 'HEA200'. Jätä tyhjäksi, jos profiilia ei rajata.")]
        string? profilePattern = null,
        [Description("Materiaalin sisältämä teksti, esim. 'S355'. Jätä tyhjäksi, jos materiaalia ei rajata.")]
        string? materialPattern = null,
        [Description("Tekla-luokka (Class), haetaan kokoonpanon main partilta täsmällisenä arvona, esim. '2'. Jätä tyhjäksi, jos luokkaa ei rajata.")]
        string? classValue = null,
        [Description("Alin Z-korkeus millimetreinä (esim. kerroksen alaraja). Jätä tyhjäksi, jos ei rajata.")]
        double? minElevation = null,
        [Description("Ylin Z-korkeus millimetreinä (esim. kerroksen yläraja). Jätä tyhjäksi, jos ei rajata.")]
        double? maxElevation = null)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var model = new Model();
            if (!model.GetConnectionStatus())
            {
                return "Tekla Structuresiin ei saada yhteyttä. Tarkista, että " +
                       "Tekla Structures on auki ja malli on ladattu.";
            }

            Beam.BeamTypeEnum? requiredBeamType = null;
            Type[] teklaTypes;

            switch (objectType?.Trim().ToLowerInvariant())
            {
                case "column":
                case "pilari":
                    teklaTypes = new[] { typeof(Beam) };
                    requiredBeamType = Beam.BeamTypeEnum.COLUMN;
                    break;
                case "plate":
                case "slab":
                case "laatta":
                    teklaTypes = new[] { typeof(ContourPlate) };
                    break;
                case "beam":
                case "palkki":
                    teklaTypes = new[] { typeof(Beam) };
                    requiredBeamType = Beam.BeamTypeEnum.BEAM;
                    break;
                default:
                    teklaTypes = new[] { typeof(Beam), typeof(ContourPlate) };
                    break;
            }

            var allOfType = model.GetModelObjectSelector().GetAllObjectsWithType(teklaTypes);
            var matchedAssemblies = new Dictionary<int, Assembly>();

            while (allOfType.MoveNext())
            {
                if (allOfType.Current is not Part part)
                {
                    continue;
                }

                if (requiredBeamType.HasValue && part is Beam beam && beam.Type != requiredBeamType.Value)
                {
                    continue;
                }

                if (!MatchesFilters(part, namePattern, positionPattern, profilePattern, materialPattern,
                        classValue, minElevation, maxElevation))
                {
                    continue;
                }

                if (part.GetAssembly() is Assembly assembly)
                {
                    matchedAssemblies[assembly.Identifier.ID] = assembly;
                }
            }

            if (matchedAssemblies.Count == 0)
            {
                return "Kriteereillä ei löytynyt yhtään kokoonpanoa mallista.";
            }

            var matches = new ArrayList();
            foreach (var assembly in matchedAssemblies.Values)
            {
                matches.Add(assembly);
            }

            new Tekla.Structures.Model.UI.ModelObjectSelector().Select(matches);
            model.CommitChanges();

            return $"Löytyi ja valittiin {matchedAssemblies.Count} kokoonpanoa mallista. " +
                   "Voit jatkaa pyytämällä esim. niiden painoja.";
        });
    }

    private static bool MatchesFilters(
        Part part,
        string? namePattern,
        string? positionPattern,
        string? profilePattern,
        string? materialPattern,
        string? classValue,
        double? minElevation,
        double? maxElevation)
    {
        if (!string.IsNullOrWhiteSpace(namePattern) &&
            (part.Name == null || part.Name.IndexOf(namePattern, StringComparison.OrdinalIgnoreCase) < 0))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(positionPattern))
        {
            string position = "";
            part.GetAssembly()?.GetReportProperty("ASSEMBLY_POS", ref position);

            if (string.IsNullOrWhiteSpace(position) || !MatchesPositionPrefix(position, positionPattern!))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(profilePattern) &&
            (part.Profile?.ProfileString == null ||
             part.Profile.ProfileString.IndexOf(profilePattern, StringComparison.OrdinalIgnoreCase) < 0))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(materialPattern) &&
            (part.Material?.MaterialString == null ||
             part.Material.MaterialString.IndexOf(materialPattern, StringComparison.OrdinalIgnoreCase) < 0))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(classValue))
        {
            string effectiveClass = part.Class;
            if (part.GetAssembly()?.GetMainPart() is Part mainPart)
            {
                effectiveClass = mainPart.Class;
            }

            if (effectiveClass != classValue)
            {
                return false;
            }
        }

        if (minElevation.HasValue || maxElevation.HasValue)
        {
            var solid = part.GetSolid();
            double partMinZ = solid.MinimumPoint.Z;
            double partMaxZ = solid.MaximumPoint.Z;

            if (minElevation.HasValue && partMaxZ < minElevation.Value)
            {
                return false;
            }

            if (maxElevation.HasValue && partMinZ > maxElevation.Value)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesPositionPrefix(string position, string pattern)
    {
        if (!position.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return position.Length == pattern.Length || !char.IsLetter(position[pattern.Length]);
    }
}