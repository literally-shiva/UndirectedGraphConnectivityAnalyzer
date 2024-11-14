using ReactiveUI;
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
    public ReactiveCommand<MainView, Unit> CreateNodeCommand { get; }
    public ReactiveCommand<MainView, Unit> CreateLinkCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearNodesCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearLinksCommand { get; }
    public CombinedReactiveCommand<Unit, Unit> ClearNodesAndLinksCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyzeConnectivityCommand { get; }
    public ReactiveCommand<MainView, Unit> SaveReportCommand { get; }

    public  NodeManager MainNodeManager { get; }
    public LinkManager MainLinkManager { get; }
    public ReportManager MainReportManager { get; }

    public MainViewModel()
    {
        LoadNodesCommand = ReactiveCommand.CreateFromTask<MainView>(LoadNodes);
        LoadLinksCommand = ReactiveCommand.CreateFromTask<MainView>(LoadLinks);
        AddNodesCommand = ReactiveCommand.CreateFromTask<MainView>(AddNodes);
        AddLinksCommand = ReactiveCommand.CreateFromTask<MainView>(AddLinks);
        CreateNodeCommand = ReactiveCommand.CreateFromTask<MainView>(CreateNode);
        CreateLinkCommand = ReactiveCommand.CreateFromTask<MainView>(CreateLink);
        ClearNodesCommand = ReactiveCommand.Create(ClearNodes);
        ClearLinksCommand = ReactiveCommand.Create(ClearLinks);
        ClearNodesAndLinksCommand = ReactiveCommand.CreateCombined(new ReactiveCommand<Unit, Unit>[] { ClearNodesCommand, ClearLinksCommand });
        AnalyzeConnectivityCommand = ReactiveCommand.Create(AnalyzeConnectivity);
        SaveReportCommand = ReactiveCommand.CreateFromTask<MainView>(SaveReport);

        MainNodeManager = new NodeManager();
        MainLinkManager = new LinkManager();
        MainReportManager = new ReportManager();
    }
    // Логичнее убрать методы по связыванию и отвязыванию в модели
    async Task LoadNodes(MainView mainView)
    {
        MainLinkManager.UnbindLinks();
        MainNodeManager.Clear();

        await MainNodeManager.ReLoadAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }

    async Task LoadLinks(MainView mainView)
    {
        MainNodeManager.UnbindNodes();
        MainLinkManager.Clear();

        await MainLinkManager.ReLoadAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }

    async Task AddNodes(MainView mainView)
    {
        await MainNodeManager.AddAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }

    async Task AddLinks(MainView mainView)
    {
        await MainLinkManager.ReLoadAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }

    public async Task CreateNode(MainView mainView)
    {
        MainNodeManager.UnbindNodes();
        MainLinkManager.UnbindLinks();

        await MainNodeManager.CreateAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }

    public async Task CreateLink(MainView mainView)
    {
        MainNodeManager.UnbindNodes();
        MainLinkManager.UnbindLinks();

        await MainLinkManager.CreateAsync(mainView);

        MainNodeManager.BindNodes(MainLinkManager.Elements);
        MainLinkManager.BindLinks(MainNodeManager.Elements);
    }
    // Связи остаются привязаны (?)
    void ClearNodes()
    {
        MainNodeManager.Clear();
    }
    // Узлы остаются привязаны (?)
    void ClearLinks()
    {
        MainLinkManager.Clear();
    }
    void AnalyzeConnectivity()
    {
        Node.GetConnectedComponents(MainNodeManager.Elements);
    }

    async Task SaveReport(MainView mainView)
    {
        await MainReportManager.SaveReportAsync(mainView, MainNodeManager.Elements, MainLinkManager.Elements);
    }
}
