using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ClosedXML.Excel;
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
                    case "txt":
                        await LoadFromTxtAsync(files[0]);
                        break;
                    case "xlsx":
                        LoadFromXlsx(files[0]);
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
                    case "txt":
                        Elements.Clear();
                        await LoadFromTxtAsync(files[0]);
                        break;
                    case "xlsx":
                        Elements.Clear();
                        LoadFromXlsx(files[0]);
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

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                try
                {
                    var leftNode = new Node(0, linkString[0].Trim());
                    var rightNode = new Node(0, linkString[1].Trim());
                    var tempLink = new Link(leftNode, rightNode, Elements.Count + 1);

                    if (!Elements.Any(link =>
                        link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                        link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[0].Name))
                    {
                        Elements.Add(tempLink);
                    }
                }
                catch
                {
                    Debug.WriteLine("Не удалось прочитать связь из файла");
                }
            }
        }

        public void LoadFromXlsx(IStorageFile file)
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

                    string? nodeLeftName;
                    string? nodeRightName;

                    while ((nodeLeftName = nodeNameCellLeft.GetValue<string>()) != "" &&
                        (nodeRightName = nodeNameCellRight.GetValue<string>()) != "")
                    {
                        var leftNode = new Node(0, nodeLeftName.Trim());
                        var rightNode = new Node(0, nodeRightName.Trim());

                        var tempLink = new Link(leftNode, rightNode, Elements.Count + 1);

                        if (!Elements.Any(link =>
                            link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                            link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                            link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                            link.Nodes[1].Name == tempLink.Nodes[0].Name))
                        {
                            Elements.Add(tempLink);
                        }
                        nodeNameCellLeft = nodeNameCellLeft.CellBelow();
                        nodeNameCellRight = nodeNameCellLeft.CellRight();
                    }
                    break;
                }
            }
        }
    }
}
