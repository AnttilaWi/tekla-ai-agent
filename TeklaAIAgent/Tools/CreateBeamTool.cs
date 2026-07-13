using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.AI;
using Tekla.Structures.Model;
using TSGeo = Tekla.Structures.Geometry3d;

namespace TeklaAIAgent.Tools;

public class CreateBeamTool : ITeklaTool
{
    public IEnumerable<AITool> GetFunctions()
    {
        yield return AIFunctionFactory.Create(CreateBeam);
    }

    [Description(
        "Luo uuden suoran palkin Tekla Structures -malliin kahden pisteen välille. " +
        "Koordinaatit ovat millimetrejä mallin globaalissa koordinaatistossa.")]
    private string CreateBeam(
        [Description("Alkupisteen X-koordinaatti (mm).")] double startX,
        [Description("Alkupisteen Y-koordinaatti (mm).")] double startY,
        [Description("Alkupisteen Z-koordinaatti (mm).")] double startZ,
        [Description("Loppupisteen X-koordinaatti (mm).")] double endX,
        [Description("Loppupisteen Y-koordinaatti (mm).")] double endY,
        [Description("Loppupisteen Z-koordinaatti (mm).")] double endZ,
        [Description(
            "Tekla-profiilimerkintä, esim. 'HEA200' tai '200*200'. Käytä " +
            "'HEA200', jos käyttäjä ei anna profiilia.")]
        string profile = "HEA200",
        [Description(
            "Tekla-materiaalimerkintä, esim. 'S355J2' teräkselle tai 'C30/37' " +
            "betonille. Käytä 'S355J2', jos käyttäjä ei anna materiaalia.")]
        string material = "S355J2")
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var model = new Model();
            if (!model.GetConnectionStatus())
            {
                return "Tekla Structuresiin ei saada yhteyttä. Tarkista, että " +
                       "Tekla Structures on auki ja malli on ladattu.";
            }

            var beam = new Beam
            {
                StartPoint = new TSGeo.Point(startX, startY, startZ),
                EndPoint = new TSGeo.Point(endX, endY, endZ)
            };
            beam.Profile.ProfileString = profile;
            beam.Material.MaterialString = material;

            bool inserted = beam.Insert();
            if (!inserted)
            {
                return "Palkin luonti epäonnistui. Tarkista profiilin ja " +
                       "materiaalin merkinnät sekä pisteiden koordinaatit.";
            }

            model.CommitChanges();

            return $"Palkki luotu onnistuneesti. Alkupiste ({startX}, {startY}, " +
                   $"{startZ}), loppupiste ({endX}, {endY}, {endZ}), profiili " +
                   $"{profile}, materiaali {material}. Tekla-tunniste: " +
                   $"{beam.Identifier.ID}.";
        });
    }
}