using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using UndirectedGraphConnectivityAnalyzer.Models;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ReactiveCommand<MainView, Unit> LoadNodesCommand { get; }
    public ReactiveCommand<MainView, Unit> LoadLinksCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyzeConnectivityCommand { get; }
    public ObservableCollection<Node> Nodes { get; }
    public ObservableCollection<Link> Links { get; }

    public MainViewModel()
    {
        LoadNodesCommand = ReactiveCommand.CreateFromTask<MainView>(LoadNodes);
        LoadLinksCommand = ReactiveCommand.CreateFromTask<MainView>(LoadLinks);
        AnalyzeConnectivityCommand = ReactiveCommand.Create(AnalyzeConnectivity);
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
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;
            int index = 1;

            Link.UnbindLinks(Links);
            Nodes.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                Nodes.Add(new Node(index, line));
                index++;
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
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;
            int index = 1;

            Node.UnbindNodes(Nodes);
            Links.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                Links.Add(new Link(new Node(0, linkString[0]), new Node(0, linkString[1]), index));
                index++;
            }

            Node.BindNodes(Nodes, Links);
            Link.BindLinks(Nodes, Links);
        }
    }
}
