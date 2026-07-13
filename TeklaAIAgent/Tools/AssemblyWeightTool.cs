using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Microsoft.Extensions.AI;
using Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;

namespace TeklaAIAgent.Tools;

/// <summary>
/// Työkalu 1: Listaa käyttäjän Tekla-mallista valitsemien kokoonpanojen
/// (assembly) painot.
/// </summary>
public class AssemblyWeightTool : ITeklaTool
{
    public IEnumerable<AITool> GetFunctions()
    {
        yield return AIFunctionFactory.Create(ListSelectedAssemblyWeights);
    }

    [Description(
        "Listaa painot kilogrammoina kaikille Tekla Structures -mallista käyttäjän " +
        "valitsemille kokoonpanoille (assembly). Käytä tätä työkalua, kun käyttäjä " +
        "kysyy valittujen kappaleiden, osien tai kokoonpanojen painoja.")]
    private string ListSelectedAssemblyWeights()
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var model = new Model();
            if (!model.GetConnectionStatus())
            {
                return "Tekla Structuresiin ei saada yhteyttä. Tarkista, että " +
                       "Tekla Structures on auki ja malli on ladattu.";
            }

            var selector = new TSMUI.ModelObjectSelector();
            var selectedObjects = selector.GetSelectedObjects();

            var report = new StringBuilder();
            int assemblyCount = 0;
            double totalWeight = 0;

            while (selectedObjects.MoveNext())
            {
                if (selectedObjects.Current is Assembly assembly)
                {
                    double weight = GetAssemblyWeightKg(assembly);
                    string label = GetAssemblyLabel(assembly);

                    report.AppendLine($"- {label}: {weight:F0} kg");
                    totalWeight += weight;
                    assemblyCount++;
                }
            }

            if (assemblyCount == 0)
            {
                return "Mallista ei ole valittuna yhtään kokoonpanoa. Valitse " +
                       "ensin haluamasi kokoonpanot Tekla Structuresin " +
                       "mallinäkymässä ja pyydä sitten uudelleen.";
            }

            report.AppendLine();
            report.AppendLine($"Yhteensä {assemblyCount} kokoonpanoa, " +
                               $"kokonaispaino {totalWeight:F0} kg.");
            return report.ToString();
        });
    }

    private static double GetAssemblyWeightKg(Assembly assembly)
    {
        double totalWeight = 0;

        if (assembly.GetMainPart() is ModelObject mainPart)
        {
            double mainWeight = 0;
            mainPart.GetReportProperty("WEIGHT", ref mainWeight);
            totalWeight += mainWeight;
        }

        foreach (var secondaryObj in assembly.GetSecondaries())
        {
            if (secondaryObj is ModelObject secondaryPart)
            {
                double secondaryWeight = 0;
                secondaryPart.GetReportProperty("WEIGHT", ref secondaryWeight);
                totalWeight += secondaryWeight;
            }
        }

        return totalWeight;
    }

    private static string GetAssemblyLabel(Assembly assembly)
    {
        string name = string.IsNullOrWhiteSpace(assembly.Name) ? "Kokoonpano" : assembly.Name;

        string position = "";
        assembly.GetReportProperty("ASSEMBLY_POS", ref position);

        if (string.IsNullOrWhiteSpace(position))
        {
            return $"{name} (#{assembly.Identifier.ID})";
        }

        return $"{name} {position}";
    }
}