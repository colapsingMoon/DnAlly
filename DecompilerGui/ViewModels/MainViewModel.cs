using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace DecompilerGui;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ObservableCollection<TypeNode> Types { get; private set; } = new();

    private TypeNode? _selectedType;
    public TypeNode? SelectedType
    {
        get => _selectedType;
        set { _selectedType = value; OnPropertyChanged(); UpdateDecompiledCode(); }
    }

    public string HeaderText { get; private set; } = "Open an assembly to begin";

    private string _decompiledCode = string.Empty;
    public string DecompiledCode
    {
        get => _decompiledCode;
        private set { _decompiledCode = value; OnPropertyChanged(); }
    }

    private CSharpDecompiler? _decompiler;

    public void LoadAssembly(string path)
    {
        var settings = new DecompilerSettings(LanguageVersion.Latest);
        _decompiler = new CSharpDecompiler(path, settings);

        var root = BuildTree(_decompiler.TypeSystem);
        Types = new ObservableCollection<TypeNode>(root.Children);
        OnPropertyChanged(nameof(Types));

        HeaderText = System.IO.Path.GetFileName(path);
        OnPropertyChanged(nameof(HeaderText));
        DecompiledCode = $"// Loaded: {path}\n// Select a type on the left to decompile.";
        SelectedType = null;
    }

    private static TypeNode BuildTree(ICompilation compilation)
    {
        var root = new TypeNode("(root)", null, isNamespace: true);

        foreach (var ns in compilation.RootNamespace.ChildNamespaces)
        {
            var nsNode = new TypeNode(ns.FullName, null, isNamespace: true);
            root.Children.Add(nsNode);
            AddNamespaceRecursive(ns, nsNode);
        }

        return root;
    }
        private static void AddNamespaceRecursive(INamespace ns, TypeNode node)
{
    // child namespaces
    foreach (var sub in ns.ChildNamespaces)
    {
        var subNode = new TypeNode(sub.FullName, null, isNamespace: true);
        node.Children.Add(subNode);
        AddNamespaceRecursive(sub, subNode);
    }

    // types in this namespace
    foreach (var t in ns.Types)
    {
        node.Children.Add(new TypeNode(t.FullName, t, isNamespace: false));
    }
}

    private void UpdateDecompiledCode()
    {
        if (_decompiler is null || SelectedType?.TypeDef is null)
        {
            DecompiledCode = string.Empty;
            return;
        }
        DecompiledCode = _decompiler!.DecompileTypeAsString(SelectedType!.TypeDef!.FullTypeName);
        
        HeaderText = SelectedType.DisplayName;
        OnPropertyChanged(nameof(HeaderText));
    }
}

public class TypeNode
{
    public string DisplayName { get; }
    public ITypeDefinition? TypeDef { get; }
    public bool IsNamespace { get; }
    public ObservableCollection<TypeNode> Children { get; } = new();

    public TypeNode(string displayName, ITypeDefinition? typeDef, bool isNamespace)
    {
        DisplayName = displayName;
        TypeDef = typeDef;
        IsNamespace = isNamespace;
    }
    public override string ToString() => DisplayName;
}
