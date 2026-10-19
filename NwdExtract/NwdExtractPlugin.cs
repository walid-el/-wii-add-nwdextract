using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace NwdExtract
{
    [Plugin("NwdExtractPlugin", "WII", DisplayName = "NwdExtract", ToolTip = "Export element rows to CSV")]
    public class NwdExtractPlugin : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            Document doc = Autodesk.Navisworks.Api.Application.ActiveDocument;
            if (doc == null)
            {
                MessageBox.Show("No document is open in Navisworks.", "NwdExtract", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 1;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "Save NwdExtract CSV";
                dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dlg.FileName = "nwd_elements.csv";

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                string output = dlg.FileName;
                try
                {
                    Extractor.Extract(doc, output);
                    MessageBox.Show($"Export complete: {output}", "NwdExtract", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Extraction failed: {ex.Message}", "NwdExtract", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
            }
        }
    }

    internal static class Extractor
    {
        private static readonly string[][] ElementIdProps =
        {
            new[] { "Element ID", "Value" },
            new[] { "Element", "Id" },
            new[] { "Element", "Element ID" },
        };

        private static readonly string[][] GuidProps =
        {
            new[] { "Item", "GUID" },
            new[] { "Element", "UniqueId" },
        };

        private static readonly string[][] WorksetProps =
        {
            new[] { "Element", "Workset" },
            new[] { "Workset", "Name" },
        };

        public static void Extract(Document doc, string outputCsv)
        {
            if (doc == null) throw new ArgumentNullException(nameof(doc));
            if (string.IsNullOrEmpty(outputCsv)) throw new ArgumentNullException(nameof(outputCsv));

            using (var csv = new StreamWriter(outputCsv, false, new UTF8Encoding(true)))
            {
                csv.WriteLine("NwdFile,GUID,ElementId,Workset,SourceFile");

                foreach (ModelItem item in doc.Models.RootItemDescendantsAndSelf)
                {
                    string elementId = First(item, ElementIdProps);
                    if (!item.HasGeometry && elementId == null) continue;

                    string guid = First(item, GuidProps);
                    if (guid == null && item.InstanceGuid != Guid.Empty)
                        guid = item.InstanceGuid.ToString();

                    string workset = First(item, WorksetProps);
                    string source = item.Model != null ? (item.Model.SourceFileName ?? item.Model.FileName) : null;
                    string nwdFile = doc.FileName ?? doc.ActiveFileName ?? "";

                    csv.WriteLine(string.Join(",", Esc(nwdFile), Esc(guid), Esc(elementId), Esc(workset), Esc(source)));
                }
            }
        }

        private static string First(ModelItem item, string[][] candidates)
        {
            foreach (var c in candidates)
            {
                DataProperty p = item.PropertyCategories.FindPropertyByDisplayName(c[0], c[1]);
                if (p == null) continue;
                string v = p.Value.ToDisplayString();
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return null;
        }

        private static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
    }
}
