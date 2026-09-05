using System.Collections.Specialized;
using System.Windows;
using Discussion.ViewModels;

namespace Discussion;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        _viewModel.Verlauf.CollectionChanged += Verlauf_CollectionChanged;
    }

    private void Verlauf_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            Dispatcher.BeginInvoke(() => VerlaufScroll.ScrollToEnd());
    }
}
