using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using DecompilerGui;  // already same namespace, but makes it explicit

namespace DecompilerGui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private async void OpenAssembly_Click(object? sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Title = "Open .NET assembly",
            AllowMultiple = false,
            Filters =
            {
                new FileDialogFilter(){ Name = "Assemblies", Extensions = { "dll", "exe" } },
                new FileDialogFilter(){ Name = "All files", Extensions = { "*" } }
            }
        };

        var paths = await ofd.ShowAsync(this);
        if (paths is { Length: > 0 })
        {
            if (DataContext is MainViewModel vm)
            {
                await Task.Run(() => vm.LoadAssembly(paths[0]));
            }
        }
    }

    private void Exit_Click(object? sender, RoutedEventArgs e) => Close();
}
