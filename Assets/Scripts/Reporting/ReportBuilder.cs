using System.IO;
using static ConsoleBase;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using UnityEngine;

// Utility class for creating a design report when using the tool
public class ReportBuilder
{
    public ModuleLayout layout;

    public void SaveReport()
    {
        var defaultFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, System.DateTime.Now.Date.ToString("yyyy-MM-dd") + ".xml");
        SaveReportToFile(defaultFilePath);
    }

    public void SaveReportToFile(string filepath)
    {
        ModulePool mp = UnityEngine.GameObject.FindObjectOfType<ModulePool>();

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        //var doc = new System.Xml.XmlDocument();
        var doc = new XDocument();

        // Attach a stylesheet to the xml file, this is done by creating a procesing instruction.
        doc.Add(new XProcessingInstruction("xml-stylesheet", "href='report.css' title='Compact' type='text/css'"));

        var reportElement = new XElement("report", new XElement("layout_name", layout._layoutName));
        doc.Add(reportElement);

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

        doc.Save(filepath);
        Debug.Log($"Saved file to {filepath}");
    }

    private static XElement BuildModuleDescription(ModuleLayout.ModuleConfig p, ModulePool mp)
    {
        // We need some data from the ConsoleModule itself, so grab one from the pool then return it once we're done
        var moduleData = mp.RequestModule(p.id);
        var entry = new XElement("entry",
            new XElement("id", p.id.ToString()),
            new XElement("position", p.position.ToString("F3")),
            new XElement("scale", p.scale),
            new XElement("rotation", p.eulerAngles),
            new XElement("description", moduleData._description),
            new XElement("product_id", moduleData._productCode),
            new XElement("design_notes", moduleData._designNotes));
        mp.ReturnModule(moduleData);
        return entry;
    }
}
