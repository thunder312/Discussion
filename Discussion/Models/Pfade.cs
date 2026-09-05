using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Discussion.Models;

public class Pfade : INotifyPropertyChanged
{
    private string _personaVorlagenOrdner = "";
    public string PersonaVorlagenOrdner { get => _personaVorlagenOrdner; set => SetField(ref _personaVorlagenOrdner, value); }

    private string _logOrdner = "";
    public string LogOrdner { get => _logOrdner; set => SetField(ref _logOrdner, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T feld, T wert, [CallerMemberName] string? name = null)
    {
        if (Equals(feld, wert))
            return;
        feld = wert;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
