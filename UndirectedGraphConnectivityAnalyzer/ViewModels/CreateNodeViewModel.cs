using Avalonia.Controls;
using Avalonia.VisualTree;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Reactive;
using UndirectedGraphConnectivityAnalyzer.Views;
using System.Threading.Tasks;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels
{
    internal class CreateNodeViewModel : ViewModelBase
    {
        public ReactiveCommand<object, Unit> GetNodeNameCommand { get; }

        public CreateNodeViewModel()
        {
            GetNodeNameCommand = ReactiveCommand.CreateFromTask<object>(GetNodeNameAsync);
        }

        async Task GetNodeNameAsync(object state)
        {
            var vm = state as CreateNodeView;
            try
            {
                var window = (Window)vm.GetVisualRoot();
                window.Close(vm.NodeName.Text.Trim());
            }
            catch
            {
                var box = MessageBoxManager.
                    GetMessageBoxStandard("Ошибка", "Отсутствуют имя объекта",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
                return;
            }
        }
    }
}
