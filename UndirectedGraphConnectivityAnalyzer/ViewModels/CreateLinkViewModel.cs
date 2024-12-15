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
    internal class CreateLinkViewModel : ViewModelBase
    {
        public ReactiveCommand<object, Unit> GetNodeNamesCommand { get; }

        public CreateLinkViewModel()
        {
            GetNodeNamesCommand = ReactiveCommand.CreateFromTask<object>(GetNodeNamesAsync);
        }

        async Task GetNodeNamesAsync(object state)
        {
            var vm = state as CreateLinkView;
            var window = (Window)vm.GetVisualRoot();
            try
            {
                string[] resultArr = [vm.NodeName1.Text.Trim(), vm.NodeName2.Text.Trim()];
                window.Close(resultArr);
            }
            catch
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", "Отсутствуют имя объекта в связи",
                    ButtonEnum.Ok);

                var result = await box.ShowAsync();
                return;
            }
        }
    }
}
