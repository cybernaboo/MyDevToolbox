public class Parametre
{
    public string Libelle { get; set; }
    public string Valeur { get; set; }

    public Parametre(string libelle, string valeur)
    {
        Libelle = libelle;
        Valeur = valeur;
    }

    public override string ToString()
    {
        return $"{Libelle} ({Valeur})"; // Affiche le libellé suivi de la valeur dans la ComboBox
    }
}
