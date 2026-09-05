using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Discussion.Models;

public class PersonaProfil : INotifyPropertyChanged
{
    private string _name = "";
    public string Name { get => _name; set => SetField(ref _name, value); }

    private string _alter = "";
    public string Alter { get => _alter; set => SetField(ref _alter, value); }

    private string _geschlecht = "";
    public string Geschlecht { get => _geschlecht; set => SetField(ref _geschlecht, value); }

    private string _bildungsstand = "";
    public string Bildungsstand { get => _bildungsstand; set => SetField(ref _bildungsstand, value); }

    private string _politischeAusrichtung = "";
    public string PolitischeAusrichtung { get => _politischeAusrichtung; set => SetField(ref _politischeAusrichtung, value); }

    private string _zusatz = "";
    public string Zusatz { get => _zusatz; set => SetField(ref _zusatz, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T feld, T wert, [CallerMemberName] string? name = null)
    {
        if (Equals(feld, wert))
            return;
        feld = wert;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
