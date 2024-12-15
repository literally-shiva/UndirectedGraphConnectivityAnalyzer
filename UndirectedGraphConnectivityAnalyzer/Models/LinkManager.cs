using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ClosedXML.Excel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
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
    public class LinkManager : DataManager<Link>
    {
        public override async Task AddAsync(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Добавление связей",
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
                        LoadFromXlsxAsync(files[0]);
                        break;
                }
            }
        }

        public override async Task CreateAsync(UserControl view)
        {
            var ownerWindow = view.GetVisualRoot();
            var window = new CreateLinkWindow() { DataContext = new CreateLinkViewModel() };
            var nodeNames = await window.ShowDialog<string[]>((Window)ownerWindow);
            if (nodeNames != null)
            {
                Node leftNode = new Node(0, nodeNames[0]);
                Node rightNode = new Node(0, nodeNames[1]);
                Link tempLink = new Link(leftNode, rightNode, Elements.Count + 1);

                if (!Elements.Any(link =>
                    link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                    link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                    link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                    link.Nodes[1].Name == tempLink.Nodes[0].Name))
                {
                    Elements.Add(tempLink);
                }
                else
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", "Создаваемая связь уже существует",
                        ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                    return;
                }
            }
        }

        public override async Task ReLoadAsync(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Загрузка связей",
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
                        LoadFromXlsxAsync(files[0]);
                        break;
                }
            }
        }

        public void BindLinks(ObservableCollection<Node> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                for (var j = 0; j < Elements.Count; j++)
                {
                    if (nodes[i].Name == Elements[j].Nodes[0].Name)
                        Elements[j].Nodes[0] = nodes[i];
                    if (nodes[i].Name == Elements[j].Nodes[1].Name)
                        Elements[j].Nodes[1] = nodes[i];
                }
            }
        }
        public void UnbindLinks()
        {
            for (var i = 0; i < Elements.Count; i++)
            {
                Elements[i].Nodes[0] = new Node(0, Elements[i].Nodes[0].Name);
                Elements[i].Nodes[1] = new Node(0, Elements[i].Nodes[1].Name);
                Elements[i].ConnectivityComponent = 0;
            }
        }

        public async Task LoadFromTxtAsync(IStorageFile file)
        {
            await using var stream = await file.OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;
            int lineNum = 1;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                try
                {
                    linkString[1] = linkString[1].Trim();
                    linkString[0] = linkString[0].Trim();

                    if (linkString[1] != "" && linkString[0] != "")
                    {
                        var leftNode = new Node(0, linkString[0]);
                        var rightNode = new Node(0, linkString[1]);
                        var tempLink = new Link(leftNode, rightNode, Elements.Count + 1);

                        if (!Elements.Any(link =>
                            link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                            link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                            link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                            link.Nodes[1].Name == tempLink.Nodes[0].Name))
                        {
                            Elements.Add(tempLink);
                        }
                        else
                        {
                            var box = MessageBoxManager
                                .GetMessageBoxStandard("Ошибка", $"Создаваемая связь уже существует (строка №{lineNum}).",
                                ButtonEnum.Ok);

                            var result = await box.ShowAsync();
                        }
                    }
                    else
                    {
                        var box = MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", $"Обнаружена строка, с пустым именем объекта, в файле со связями (строка №{lineNum}).",
                            ButtonEnum.Ok);

                        var result = await box.ShowAsync();
                    }
                }
                catch
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Обнаружена строка, не являющаяся связью, в файле со связями (строка №{lineNum}).",
                        ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }

                lineNum++;
            }
        }

        public async Task LoadFromXlsxAsync(IStorageFile file)
        {
            using var wbook = new XLWorkbook(file.Path.LocalPath);
            IXLCells nodeHeaderCells;

            foreach (var worksheet in wbook.Worksheets)
            {
                nodeHeaderCells = worksheet.CellsUsed(cell => cell.GetString() == "Связи");
                if (nodeHeaderCells.Count() > 0)
                {
                    var nodeNameCellLeft = nodeHeaderCells.First().CellBelow();
                    var nodeNameCellRight = nodeNameCellLeft.CellRight();

                    string? nodeLeftName = nodeNameCellLeft.GetValue<string>().Trim();
                    string? nodeRightName = nodeNameCellRight.GetValue<string>().Trim();

                    while ((nodeLeftName != "") | (nodeRightName != ""))
                    {
                        if (nodeLeftName != "" && nodeRightName != "")
                        {
                            var leftNode = new Node(0, nodeLeftName);
                            var rightNode = new Node(0, nodeRightName);

                            var tempLink = new Link(leftNode, rightNode, Elements.Count + 1);

                            if (!Elements.Any(link =>
                                link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                                link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                                link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                                link.Nodes[1].Name == tempLink.Nodes[0].Name))
                            {
                                Elements.Add(tempLink);
                            }
                            else
                            {
                                var box = MessageBoxManager
                                    .GetMessageBoxStandard("Ошибка", $"Создаваемая связь уже существует (строка №{nodeNameCellLeft.Address.RowNumber}).",
                                    ButtonEnum.Ok);

                                var result = await box.ShowAsync();
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Обнаружена строка, с пустым именем объекта, в файле со связями.");
                            var box = MessageBoxManager
                                .GetMessageBoxStandard("Ошибка", $"Обнаружена строка, не являющаяся связью, в файле со связями (строка №{nodeNameCellLeft.Address.RowNumber}).",
                                ButtonEnum.Ok);

                            var result = await box.ShowAsync();
                        }

                        nodeNameCellLeft = nodeNameCellLeft.CellBelow();
                        nodeNameCellRight = nodeNameCellLeft.CellRight();

                        nodeLeftName = nodeNameCellLeft.GetValue<string>();
                        nodeRightName = nodeNameCellRight.GetValue<string>();
                    }
                    break;
                }
            }
        }

        public async override Task Save(UserControl view)
        {
            var topLevel = TopLevel.GetTopLevel(view);

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранение связей",
                DefaultExtension = ".xlsx",
                SuggestedFileName = "Связи",
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

            foreach (var link in Elements)
            {
                await streamWriter.WriteLineAsync($"{link.Nodes[0].Name}<->{link.Nodes[1].Name}");
            }
        }

        private void SaveToXlsx(IStorageFile file)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Связи");

            var reportLinks = from link in Elements select new { LeftLink = link.Nodes[0].Name, RightLink = link.Nodes[1].Name };

            worksheet.Cell(1, 1).Value = "Связи";
            worksheet.Range(1, 1, 1, 2).Merge();
            worksheet.Cell(2, 1).InsertData(reportLinks.AsEnumerable());

            workbook.SaveAs(file.Path.LocalPath);
        }
    }
}
