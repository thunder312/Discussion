using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PersonaTraitTest;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<MerkmalZeile> Zeilen { get; } = new()
    {
        new MerkmalZeile { Merkmal = "Alter", Min = "6", Normal = "50", Max = "100" },
        new MerkmalZeile { Merkmal = "Geschlecht", Min = "männlich", Normal = "divers", Max = "weiblich" },
        new MerkmalZeile { Merkmal = "Bildungsstand", Min = "Hauptschule", Normal = "Abitur", Max = "Promotion" },
        new MerkmalZeile { Merkmal = "Politische Ausrichtung", Min = "links", Normal = "unpolitisch", Max = "rechts" },
        new MerkmalZeile { Merkmal = "Weitere Merkmale", Min = "ruhig bedächtig", Normal = "", Max = "cholerisch, fluchend" },
    };

    public ObservableCollection<string> LogZeilen { get; } = new();

    private string _frage = "Sollen zuckerhaltige Snacks verboten werden?";
    public string Frage
    {
        get => _frage;
        set { _frage = value; OnPropertyChanged(); }
    }

    private string _verbindungsInfo = "";
    public string VerbindungsInfo
    {
        get => _verbindungsInfo;
        set { _verbindungsInfo = value; OnPropertyChanged(); }
    }

    private string _status = "Bereit.";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private bool _laeuft;
    public bool Laeuft
    {
        get => _laeuft;
        set { _laeuft = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
