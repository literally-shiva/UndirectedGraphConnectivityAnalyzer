using Avalonia.Controls;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Reactive;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels
{
    internal class CreateNodeViewModel : ViewModelBase
    {
        public ReactiveCommand<object, Unit> GetNodeNameCommand { get; }

        public CreateNodeViewModel()
        {
            GetNodeNameCommand = ReactiveCommand.Create<object>(GetNodeName);
        }

        void GetNodeName(object state)
        {
            var vm = state as CreateNodeView;
            var window = (Window)vm.GetVisualRoot();
            window.Close(vm.NodeName.Text);
        }
    }
}
