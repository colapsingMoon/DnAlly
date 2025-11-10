using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace DecompilerGui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private async void OpenAssembly_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    {
        Title = "Open .NET assembly",
        AllowMultiple = false,
        FileTypeFilter = new[]
        {
            new FilePickerFileType("Assemblies"){ Patterns = new[] { "*.dll", "*.exe" } },
            FilePickerFileTypes.All
        }
    });

    if (files is { Count: > 0 } && DataContext is MainViewModel vm)
    {
        var path = files[0].Path.LocalPath;
        await Task.Run(() => vm.LoadAssembly(path));
    }
}

    private void Exit_Click(object? sender, RoutedEventArgs e) => Close();
}
