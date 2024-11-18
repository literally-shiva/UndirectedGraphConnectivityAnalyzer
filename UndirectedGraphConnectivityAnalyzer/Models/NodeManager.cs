using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ClosedXML.Excel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UndirectedGraphConnectivityAnalyzer.ViewModels;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class NodeManager : DataManager<Node>
    {
        public override async Task AddAsync(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Добавление объектов",
                AllowMultiple = false,
                FileTypeFilter = new[] { ExcelType, FilePickerFileTypes.TextPlain, FilePickerFileTypes.All }
            });

            if (files.Count >= 1)
            {
                var fileType = Path.GetExtension(files[0].Name);

                switch (fileType)
                {
                    case ".txt":
                        await LoadFromTxtAsync(files[0]);
                        break;
                    case ".xlsx":
                        LoadFromXlsx(files[0]);
                        break;
                }
            }
        }

        public override async Task CreateAsync(UserControl view)
        {
            var ownerWindow = view.GetVisualRoot();
            var window = new CreateNodeWindow() { DataContext = new CreateNodeViewModel() };
            var nodeName = await window.ShowDialog<string>((Window)ownerWindow);
            if (nodeName != null)
            {
                Node tempNode = new Node(Elements.Count + 1, nodeName);

                if (!Elements.Any(node => node.Name == tempNode.Name))
                {
                    Elements.Add(tempNode);
                }
            }
        }

        public override async Task ReLoadAsync(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);
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
                    case ".txt":
                        Elements.Clear();
                        await LoadFromTxtAsync(files[0]);
                        break;
                    case ".xlsx":
                        Elements.Clear();
                        LoadFromXlsx(files[0]);
                        break;
                }
            }
        }

        public void BindNodes(ObservableCollection<Link> links)
        {
            for (var i = 0; i < Elements.Count; i++)
            {
                for (var j = 0; j < links.Count; j++)
                {
                    if ((Elements[i].Name == links[j].Nodes[0].Name ||
                        Elements[i].Name == links[j].Nodes[1].Name) &&
                        !Elements[i].Links.Any(link =>
                            link.Nodes[0].Name == links[j].Nodes[0].Name &&
                            link.Nodes[1].Name == links[j].Nodes[1].Name ||
                            link.Nodes[0].Name == links[j].Nodes[1].Name &&
                            link.Nodes[1].Name == links[j].Nodes[0].Name))
                    {
                        Elements[i].AddLink(links[j]);
                    }
                }
            }
        }

        public void UnbindNodes()
        {
            foreach (var node in Elements)
            {
                node.Links.Clear();
                node.ConnectivityComponent = 0;
            }
        }

        public async Task LoadFromTxtAsync(IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                if (line != "")
                {
                    line = line.Trim();
                    var tempNode = new Node(Elements.Count + 1, line);

                    if (!Elements.Any(node => node.Name == tempNode.Name))
                    {
                        Elements.Add(tempNode);
                    }
                }
                else
                {
                    Debug.WriteLine("Обнаружена пустая строка в файле с объектами.");
                }
            }
        }

        public void LoadFromXlsx(IStorageFile file)
        {
            using var workBook = new XLWorkbook(file.Path.LocalPath);
            IXLCells nodeHeaderCells;

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
                        var tempNode = new Node(Elements.Count + 1, nodeName);

                        if (!Elements.Any(node => node.Name == tempNode.Name))
                        {
                            Elements.Add(tempNode);
                        }

                        nodeNameCell = nodeNameCell.CellBelow();
                    }
                    break;
                }
            }
        }

        public override async Task Save(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранение объектов",
                DefaultExtension = ".xlsx",
                SuggestedFileName = "Объекты",
                FileTypeChoices = [ExcelType, FilePickerFileTypes.TextPlain, FilePickerFileTypes.All],
                ShowOverwritePrompt = true
            });

            if (file != null)
            {
                var fileType = Path.GetExtension(file.Name);

                switch (fileType)
                {
                    case ".txt":
                        await SaveToTxt(file);
                        break;
                    case ".xlsx":
                        SaveToXlsx(file);
                        break;
                }
            }
        }

        private async Task SaveToTxt(IStorageFile file)
        {
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);

            foreach (var node in Elements)
            {
                await streamWriter.WriteLineAsync(node.Name);
            }
        }

        private void SaveToXlsx(IStorageFile file)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Объекты");

            var nodes = from node in Elements select new { node.Name };

            worksheet.Cell(1, 1).Value = "Объекты";
            worksheet.Cell(2, 1).InsertData(nodes.AsEnumerable());

            workbook.SaveAs(file.Path.LocalPath);
        }
    }
}

