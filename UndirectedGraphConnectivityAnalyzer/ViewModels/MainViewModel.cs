using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using UndirectedGraphConnectivityAnalyzer.Models;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ReactiveCommand<MainView, Unit> LoadNodesCommand { get; }
    public ReactiveCommand<MainView, Unit> LoadLinksCommand { get; }
    public ReactiveCommand<MainView, Unit> AddNodesCommand { get; }
    public ReactiveCommand<MainView, Unit> AddLinksCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearNodesCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearLinksCommand { get; }
    public CombinedReactiveCommand<Unit, Unit> ClearNodesAndLinksCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyzeConnectivityCommand { get; }
    public ReactiveCommand<MainView, Unit> SaveDataCommand { get; }
    public ReactiveCommand<MainView, Unit> CreateNodeCommand {  get; }
    public ObservableCollection<Node> Nodes { get; }
    public ObservableCollection<Link> Links { get; }

    public MainViewModel()
    {
        LoadNodesCommand = ReactiveCommand.CreateFromTask<MainView>(LoadNodes);
        LoadLinksCommand = ReactiveCommand.CreateFromTask<MainView>(LoadLinks);
        AddNodesCommand = ReactiveCommand.CreateFromTask<MainView>(AddNodes);
        AddLinksCommand = ReactiveCommand.CreateFromTask<MainView>(AddLinks);
        ClearNodesCommand = ReactiveCommand.Create(ClearNodes);
        ClearLinksCommand = ReactiveCommand.Create(ClearLinks);
        ClearNodesAndLinksCommand = ReactiveCommand.CreateCombined(new ReactiveCommand<Unit, Unit>[] { ClearNodesCommand, ClearLinksCommand });
        AnalyzeConnectivityCommand = ReactiveCommand.Create(AnalyzeConnectivity);
        SaveDataCommand = ReactiveCommand.CreateFromTask<MainView>(SaveData);
        CreateNodeCommand = ReactiveCommand.CreateFromTask<MainView>(CreateNode);
        Nodes = new ObservableCollection<Node>();
        Links = new ObservableCollection<Link>();
    }

    void AnalyzeConnectivity()
    {
        Node.GetConnectedComponents(Nodes);
    }

    async Task LoadNodes(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Загрузка объектов",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });
        // Возможно, в соответствии с MVVM, правильнее вынести эту логику в отдельный класс вроде "FileService"
        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            Link.UnbindLinks(Links);
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

            Node.BindNodes(Nodes, Links);
            Link.BindLinks(Nodes, Links);
        }
    }
    async Task LoadLinks(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Загрузка связей",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });
        // Возможно, в соответствии с MVVM, правильнее вынести эту логику в отдельный класс вроде "FileService"
        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            Node.UnbindNodes(Nodes);
            Links.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                try
                {
                    var leftNode = new Node(0, linkString[0].Trim());
                    var rightNode = new Node(0, linkString[1].Trim());
                    var tempLink = new Link(leftNode, rightNode, Links.Count + 1);

                    if (!Links.Any(link =>
                        link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                        link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[0].Name))
                    {
                        Links.Add(tempLink);
                    }
                }
                catch
                {
                    Debug.WriteLine("Не удалось прочитать связь из файла");
                }
            }

            Node.BindNodes(Nodes, Links);
            Link.BindLinks(Nodes, Links);
        }
    }
    async Task AddNodes(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Добавление объектов",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });
        // Возможно, в соответствии с MVVM, правильнее вынести эту логику в отдельный класс вроде "FileService"
        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                line = line.Trim();
                var tempNode = new Node(Nodes.Count + 1, line);

                if (!Nodes.Any(node => node.Name == tempNode.Name))
                {
                    Nodes.Add(tempNode);
                }
            }

            Node.BindNodes(Nodes, Links);
            Link.BindLinks(Nodes, Links);
        }
    }
    async Task AddLinks(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Добавление связей",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });
        // Возможно, в соответствии с MVVM, правильнее вынести эту логику в отдельный класс вроде "FileService"
        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                try
                {
                    var leftNode = new Node(0, linkString[0].Trim());
                    var rightNode = new Node(0, linkString[1].Trim());
                    var tempLink = new Link(leftNode, rightNode, Links.Count + 1);

                    if (!Links.Any(link =>
                        link.Nodes[0].Name == tempLink.Nodes[0].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[1].Name ||
                        link.Nodes[0].Name == tempLink.Nodes[1].Name &&
                        link.Nodes[1].Name == tempLink.Nodes[0].Name))
                    {
                        Links.Add(tempLink);
                    }
                }
                catch
                {
                    Debug.WriteLine("Не удалось прочитать связь из файла");
                }
            }

            Node.BindNodes(Nodes, Links);
            Link.BindLinks(Nodes, Links);
        }
    }
    void ClearNodes()
    {
        Link.UnbindLinks(Links);
        Nodes.Clear();
    }
    void ClearLinks()
    {
        Node.UnbindNodes(Nodes);
        Links.Clear();
    }
    async Task SaveData(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var dir = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранение данных",
            DefaultExtension = ".txt",
            SuggestedFileName = "Отчёт анализа связности",
            FileTypeChoices = [FilePickerFileTypes.TextPlain],
            ShowOverwritePrompt = true
        });
        // Возможно, в соответствии с MVVM, правильнее вынести эту логику в отдельный класс вроде "FileService"
        if (dir != null)
        {
            await using var stream = await dir.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);

            await streamWriter.WriteLineAsync("Объекты и их компоненты связности:");
            foreach (var node in Nodes)
            {
                await streamWriter.WriteLineAsync($"{node.Name} : {node.ConnectivityComponent}");
            }

            await streamWriter.WriteLineAsync("\nСвязи и их компоненты связности:");
            foreach (var link in Links)
            {
                await streamWriter.WriteLineAsync($"{link.Nodes[0].Name}<->{link.Nodes[1].Name} : {link.ConnectivityComponent}");
            }
        }
    }
    public async Task CreateNode(MainView mainView)
    {
        var ownerWindow = mainView.GetVisualRoot();
        var window = new CreateNodeWindow() { DataContext = new CreateNodeViewModel() };
        var nodeName = await window.ShowDialog<string>((Window)ownerWindow);
        Nodes.Add(new Node(Nodes.Count + 1, nodeName));
    }
}
