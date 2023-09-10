using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace mdt
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ChargerDonneesDepuisXML();
        }

        private void ChargerDonneesDepuisXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("config.xml");

                RemplirListeDepuisXML(xmlDoc, "Parameter1", cmbParametre1);
                RemplirListeDepuisXML(xmlDoc, "Parameter2", cmbParametre2);
                RemplirListeDepuisXML(xmlDoc, "Parameter3", cmbParametre3);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données depuis le fichier XML : {ex.Message}");
            }
        }

        private void RemplirListeDepuisXML(XmlDocument xmlDoc, string nodeName, System.Windows.Controls.ComboBox comboBox)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes($"//{nodeName}/Item");
            var parametres = new List<Parametre>();

            foreach (XmlNode node in nodes)
            {
                string libelle = node.SelectSingleNode("Libelle")?.InnerText;
                string valeur = node.SelectSingleNode("Valeur")?.InnerText;

                if (libelle != null && valeur != null)
                {
                    parametres.Add(new Parametre(libelle, valeur));
                }
            }

            comboBox.ItemsSource = parametres;
            comboBox.DisplayMemberPath = "Libelle";
            comboBox.SelectedIndex = 0;
        }

        private void ExecuterCommande_Click(object sender, RoutedEventArgs e)
        {
            Parametre parametre1 = cmbParametre1.SelectedValue as Parametre;
            Parametre parametre2 = cmbParametre2.SelectedValue as Parametre;
            Parametre parametre3 = cmbParametre3.SelectedValue as Parametre;

            if (parametre1 != null && parametre2 != null && parametre3 != null)
            {
                string commande = $"dir {parametre1.Valeur} {parametre2.Valeur} {parametre3.Valeur}";

                txtCommande.Text = commande;

                string sortie = ExecuterCommandeShell(commande);
                txtSortie.Text = sortie;
            }
        }

        private string ExecuterCommandeShell(string commande)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.Diagnostics.Process process = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                System.IO.StreamWriter sw = process.StandardInput;
                System.IO.StreamReader sr = process.StandardOutput;

                sw.WriteLine(commande);
                sw.Close();

                string sortie = sr.ReadToEnd();
                sr.Close();

                return sortie;
            }
            catch (Exception ex)
            {
                return $"Erreur lors de l'exécution de la commande : {ex.Message}";
            }
        }
    }

    public class Parametre
    {
        public string Libelle { get; set; }
        public string Valeur { get; set; }

        public Parametre(string libelle, string valeur)
        {
            Libelle = libelle;
            Valeur = valeur;
        }
    }
}
