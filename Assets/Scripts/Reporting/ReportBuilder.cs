using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;
using static ConsoleBase;

// Utility class for creating a design report when using the tool
public class ReportBuilder
{
    public ModuleLayout layout;
    public ModuleLayout baselineLayout;

    private XDocument report;

    public void SaveReport()
    {
        var defaultFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, System.DateTime.Now.Date.ToString("yyyy-MM-dd") + ".xml");
        var fileIndex = 0;
        while (File.Exists(defaultFilePath))
        {
            var m = Regex.Match(defaultFilePath, @"( \(\d+\))?\.xml$");

            defaultFilePath = defaultFilePath.Substring(0, m.Index) + $" ({++fileIndex}).xml";
        }
        SaveReportToFile(defaultFilePath);
    }

    public void SaveReportToFile(string filepath)
    {
        ModulePool mp = Object.FindObjectOfType<ModulePool>();

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        report = new XDocument();

        // Attach a stylesheet to the xml file, this is done by creating a procesing instruction.
        report.Add(new XProcessingInstruction("xml-stylesheet", "href='report.css' title='Compact' type='text/css'"));

        var reportElement = new XElement("report", BuildReportDescription());
        report.Add(reportElement);

        var panelElement = new XElement("panel", new XAttribute("name", "Upper panel"),
            from p in layout._panelLayouts
            where p.panelId == 0
            let entry = BuildModuleDescription(p, mp)
            select entry);
        reportElement.Add(panelElement);

        panelElement = new XElement("panel", new XAttribute("name", "Lower panel"),
            from p in layout._panelLayouts
            where p.panelId == 1
            let entry = BuildModuleDescription(p, mp)
            select entry);
        reportElement.Add(panelElement);

        report.Save(filepath);
        Debug.Log($"Saved file to {filepath}");
    }

    private XElement BuildReportDescription()
    {
        var reportDescription = new XElement("description",
            new XElement("layout_name", layout._layoutName),
            new XElement("version", layout._version),
            new XElement("date", System.DateTime.Now.ToShortDateString()),
            new XElement("time", System.DateTime.Now.ToShortTimeString()));
        return reportDescription;
    }

    private static XElement BuildModuleDescription(ModuleLayout.ModuleConfig p, ModulePool mp)
    {
        // We need some data from the ConsoleModule itself, so grab one from the pool then return it once we're done
        var moduleData = mp.GetPrefab(p.id);
        var entry = new XElement("entry",
            new XElement("name", moduleData.gameObject.name),
            new XElement("id", p.id.ToString()),
            new XElement("position", GetPositionString(ref p)),
            new XElement("scale", p.scale),
            new XElement("rotation", p.eulerAngles),
            new XElement("description", moduleData._description),
            new XElement("product_id", moduleData._productCode),
            new XElement("design_notes", moduleData._designNotes));
        return entry;
    }

    private static string GetPositionString(ref ModuleLayout.ModuleConfig p) => $"X: {p.position.x * 100:0.00} cm\nY: {p.position.y * 100:0.00} cm"; //return p.position.ToString("F3");

    public void GenerateDiffReport()
    {
        var r = (edits: new[] { "" }, removals: new[] { "" }, additions: new[] { "" });

        var markedComponents = new System.Collections.Generic.HashSet<int>();

        // match every component in baseline with a component in the current layout
        foreach (var p in baselineLayout._panelLayouts)
        {
            ModuleLayout.ModuleConfig? modified = null;
            if (layout._panelLayouts.Exists(_p => _p.id == p.id))
                modified = layout._panelLayouts.Find(_p => _p.id == p.id);
            if (modified != null)
            {
                r.edits[0] = "found";
                markedComponents.Add(p.id);
            }
            else
            {
                r.removals[0] = "found";
                markedComponents.Add(p.id);
            }
        }
        // all remaining components in baseline are marked as removals
        // all unmatched components in current layout are additions
        foreach (var p in layout._panelLayouts)
        {
            if (markedComponents.Contains(p.id) == false)
            {
                r.additions[0] = "found";
                markedComponents.Add(p.id);
            }
        }

        Debug.Log($"Edits - {r.edits[0]}; Additions - {r.additions[0]}; Removals - {r.removals[0]}");
    }
}
