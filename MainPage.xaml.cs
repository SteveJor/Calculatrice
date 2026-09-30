using System.Globalization;

namespace Calculatrice;

public partial class MainPage : ContentPage
{
    string _entry = "0";     // nombre en cours de saisie 
    string _expr = "";       // ligne du haut : l'opération en cours
    decimal? _left;          // opérande de gauche mémorisé
    string? _op;             // opérateur en attente
    bool _fresh = true;      // true : la prochaine saisie repart d'un nombre neuf
    bool _error;             // true : un message d'erreur est affiché

    public MainPage()
    {
        InitializeComponent();
        Afficher();
    }

    decimal Courant => decimal.Parse(_entry, CultureInfo.InvariantCulture);

    static string Format(decimal v) => Math.Round(v, 10).ToString("0.##########", CultureInfo.InvariantCulture);

    static string Fr(string s) => s.Replace('.', ',');   // affichage avec la virgule

    void Reset()
    {
        _entry = "0"; _expr = ""; _left = null; _op = null; _fresh = true; _error = false;
    }

    void Erreur(string message)
    {
        Reset();
        _error = true;
        _entry = message;
        Afficher();
    }

    void Afficher()
    {
        ExprLabel.Text = Fr(_expr);
        ResultLabel.Text = Fr(_entry);
        ResultLabel.FontSize = _error ? 22 : _entry.Length switch { > 14 => 30, > 10 => 40, _ => 56 };
    }

    void OnDigit(object? sender, EventArgs e)
    {
        if (_error) Reset();
        var d = ((Button)sender!).Text;
        if (_fresh)
        {
            if (_op is null) _expr = "";
            _entry = d;
            _fresh = false;
        }
        else if (_entry.Count(char.IsDigit) < 15)
            _entry = _entry == "0" ? d : _entry + d;
        Afficher();
    }

    void OnDecimal(object? sender, EventArgs e)
    {
        if (_error) Reset();
        if (_fresh)
        {
            if (_op is null) _expr = "";
            _entry = "0.";
            _fresh = false;
        }
        else if (!_entry.Contains('.'))
            _entry += ".";
        Afficher();
    }

    void OnOperator(object? sender, EventArgs e)
    {
        if (_error) Reset();
        var op = ((Button)sender!).Text;
        if (_op is not null && !_fresh)
        {
            if (!Appliquer()) return;          // enchaînement : 12 + 3 + ...
        }
        else if (_op is null)
            _left = Courant;
        _op = op;
        _fresh = true;
        _expr = $"{Format(_left!.Value)} {op}";
        Afficher();
    }

    void OnEquals(object? sender, EventArgs e)
    {
        if (_error || _op is null) return;
        var (a, op, b) = (_left!.Value, _op, Courant);
        if (!Appliquer()) return;
        _expr = $"{Format(a)} {op} {Format(b)} =";
        _left = null;
        _op = null;
        _fresh = true;
        Afficher();
    }

    bool Appliquer()
    {
        try
        {
            var b = Courant;
            decimal? r = _op switch
            {
                "+" => _left + b,
                "−" => _left - b,
                "×" => _left * b,
                "÷" => b == 0 ? null : _left / b,
                _ => b
            };
            if (r is null) { Erreur("Division par zéro impossible"); return false; }
            _left = r;
            _entry = Format(r.Value);
            return true;
        }
        catch (OverflowException)
        {
            Erreur("Nombre trop grand");
            return false;
        }
    }

    void OnClear(object? sender, EventArgs e)
    {
        Reset();
        Afficher();
    }

    void OnBackspace(object? sender, EventArgs e)
    {
        if (_error) { Reset(); Afficher(); return; }
        if (_fresh) return;
        _entry = _entry[..^1];
        if (_entry is "" or "-") _entry = "0";
        Afficher();
    }

    void OnSign(object? sender, EventArgs e)
    {
        if (_error || _entry == "0") return;
        _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        Afficher();
    }

    void OnPercent(object? sender, EventArgs e)
    {
        if (_error) return;
        _entry = Format(Courant / 100);
        _fresh = true;
        Afficher();
    }
}
