using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    internal class NodeManager : IDataManager
    {
        public static FilePickerFileType ExcelType { get; } = new("Excel")
        {
            Patterns = new[] { "*.xlsx" }
        };

        public List<Node> Nodes { get; set; }
        public UserControl View;

        public NodeManager(UserControl view)
        {
            Nodes = new List<Node>();
            View = view;
        }

        public async Task AddAsync()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void Create()
        {
            throw new System.NotImplementedException();
        }

        public async Task LoadAsync()
        {
            var topLevel = TopLevel.GetTopLevel(View);

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Загрузка объектов",
                AllowMultiple = false,
                FileTypeFilter = new[] { ExcelType, FilePickerFileTypes.TextPlain, FilePickerFileTypes.All }
            });

            if (files.Count >= 1)
            {
                var fileType = Path.GetExtension(files[0].Name);

                switch (fileType)
                {
                    case "txt":
                        await LoadFromTxtAsync(files[0]);
                        break;
                    case "xlsx":
                        LoadFromXlsx(files[0]);
                        break;
                }
            }
        }

        public async Task LoadFromTxtAsync(IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            Nodes.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                line = line.Trim();
                var tempNode = new Node(Nodes.Count + 1, line);

                if (!Nodes.Any(node => node.Name == tempNode.Name))
                {
                    Nodes.Add(tempNode);
                }
            }
        }

        public void LoadFromXlsx(IStorageFile file)
        {
            using var workBook = new XLWorkbook(file.Path.LocalPath);
            IXLCells nodeHeaderCells;

            Nodes.Clear();

            foreach (var worksheet in workBook.Worksheets)
            {
                nodeHeaderCells = worksheet.CellsUsed(cell => cell.GetString() == "Объекты");

                if (nodeHeaderCells.Count() > 0)
                {
                    var nodeNameCell = nodeHeaderCells.First().CellBelow();
                    string? nodeName;

                    while ((nodeName = nodeNameCell.GetValue<string>()) != "")
                    {
                        nodeName = nodeName.Trim();
                        var tempNode = new Node(Nodes.Count + 1, nodeName);

                        if (!Nodes.Any(node => node.Name == tempNode.Name))
                        {
                            Nodes.Add(tempNode);
                        }

                        nodeNameCell = nodeNameCell.CellBelow();
                    }
                    break;
                }
            }
        }
    }
}

