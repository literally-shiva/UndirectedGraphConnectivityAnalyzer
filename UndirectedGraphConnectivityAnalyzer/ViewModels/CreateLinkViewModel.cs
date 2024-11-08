using Avalonia.Controls;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Reactive;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels
{
    internal class CreateLinkViewModel : ViewModelBase
    {
        public ReactiveCommand<object, Unit> GetNodeNamesCommand { get; }

        public CreateLinkViewModel()
        {
            GetNodeNamesCommand = ReactiveCommand.Create<object>(GetNodeNames);
        }

        void GetNodeNames(object state)
        {
            var vm = state as CreateLinkView;
            var window = (Window)vm.GetVisualRoot();
            string[] resultArr = [vm.NodeName1.Text.Trim(), vm.NodeName2.Text.Trim()];
            window.Close(resultArr);
        }
    }
}
