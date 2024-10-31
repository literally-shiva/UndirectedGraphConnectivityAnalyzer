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
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;
            int index = 1;

            Links.Clear();
            Nodes.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                Nodes.Add(new Node(index, line));
                index++;
            }
        }
    }
    async Task LoadLinks(MainView mainView)
    {
        var topLevel = TopLevel.GetTopLevel(mainView);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string? line;
            int index = 1;

            Links.Clear();

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                var linkString = line.Split("<->");
                Node leftNode = null, rightNode = null;

                foreach (var node in Nodes)
                {
                    if (node.Name == linkString[0])
                    {
                        leftNode = node;
                    }
                    if (node.Name == linkString[1])
                    {
                        rightNode = node;
                    }
                }

                Link tempLink = new Link(leftNode, rightNode, index);
                Links.Add(tempLink);
                index++;
            }

            for (var i = 0; i < Nodes.Count; i++)
            {
                for (var j = 0; j < Links.Count; j++)
                {
                    if (Nodes[i].Equals(Links[j].Nodes[0]) || Nodes[i].Equals(Links[j].Nodes[1]))
                    {
                        Nodes[i].AddLink(Links[j]);
                    }
                }
            }
        }
    }
}
