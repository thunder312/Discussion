using System.Windows.Input;

namespace Discussion.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Func<Task>? _executeAsync;
    private readonly Action? _execute;
    private readonly Action<object?>? _executeParam;
    private readonly Func<object?, Task>? _executeParamAsync;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object?> executeParam, Func<bool>? canExecute = null)
    {
        _executeParam = executeParam;
        _canExecute = canExecute;
    }

    public RelayCommand(Func<object?, Task> executeParamAsync, Func<bool>? canExecute = null)
    {
        _executeParamAsync = executeParamAsync;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        if (_executeAsync != null)
            await _executeAsync();
        else if (_executeParamAsync != null)
            await _executeParamAsync(parameter);
        else if (_executeParam != null)
            _executeParam(parameter);
        else
            _execute?.Invoke();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
