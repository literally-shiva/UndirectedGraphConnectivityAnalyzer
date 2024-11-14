using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class ReportManager
    {
        public static FilePickerFileType ExcelType { get; } = new("Excel")
        {
            Patterns = new[] { "*.xlsx" }
        };

        public async Task SaveReportAsync(UserControl view, ObservableCollection<Node> nodes, ObservableCollection<Link> links)
        {
            var topLevel = TopLevel.GetTopLevel(view);

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранение отчёта",
                DefaultExtension = ".xlsx",
                SuggestedFileName = "Отчёт анализа связности",
                FileTypeChoices = [ExcelType, FilePickerFileTypes.TextPlain, FilePickerFileTypes.All],
                ShowOverwritePrompt = true
            });

            if (file != null)
            {
                var fileType = Path.GetExtension(file.Name);

                switch (fileType)
                {
                    case ".txt":
                        await SaveToTxt(file, nodes, links);
                        break;
                    case ".xlsx":
                        SaveToXlsx(file, nodes, links);
                        break;
                }
            }
        }

        public async Task SaveToTxt(IStorageFile file, ObservableCollection<Node> nodes, ObservableCollection<Link> links)
        {
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);

            await streamWriter.WriteLineAsync("Объекты и их компоненты связности:");
            foreach (var node in nodes)
            {
                await streamWriter.WriteLineAsync($"{node.Name} : {node.ConnectivityComponent}");
            }

            await streamWriter.WriteLineAsync("\nСвязи и их компоненты связности:");
            foreach (var link in links)
            {
                await streamWriter.WriteLineAsync($"{link.Nodes[0].Name}<->{link.Nodes[1].Name} : {link.ConnectivityComponent}");
            }
        }

        public void SaveToXlsx(IStorageFile file, ObservableCollection<Node> nodes, ObservableCollection<Link> links)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Отчёт");

            var reportNodes = from node in nodes
                              select new { node.Name, node.ConnectivityComponent };
            var reportLinks = from link in links
                              select new { LeftLink = link.Nodes[0].Name, RightLink = link.Nodes[1].Name, link.ConnectivityComponent };

            worksheet.Cell(1, 1).Value = "Объекты";
            worksheet.Range(1, 1, 1, 2).Merge();
            worksheet.Cell(2, 1).InsertData(reportNodes.AsEnumerable());

            worksheet.Cell(1, 4).Value = "Связи";
            worksheet.Range(1, 4, 1, 6).Merge();
            worksheet.Cell(2, 4).InsertData(reportLinks.AsEnumerable());

            workbook.SaveAs(file.Path.LocalPath);
        }
    }
}
