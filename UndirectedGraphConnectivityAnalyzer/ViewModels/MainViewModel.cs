using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using UndirectedGraphConnectivityAnalyzer.Views;

namespace UndirectedGraphConnectivityAnalyzer.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        OpenFileCommand = ReactiveCommand.CreateFromTask<MainView>(OpenFile);
    }

    public ReactiveCommand<MainView, Unit> OpenFileCommand { get; }

    async Task OpenFile(MainView mainView)
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
            var fileContent = await streamReader.ReadToEndAsync();
        }
    }
}
