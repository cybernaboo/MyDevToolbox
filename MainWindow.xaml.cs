using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace mdt
{
    public partial class MainWindow : Window
    {
        string cheminFichierConfig = "config.xml";

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
                xmlDoc.Load(cheminFichierConfig);

                RemplirListeDepuisXML(xmlDoc, "Parameter1", cmbParametre1);
                RemplirListeDepuisXML(xmlDoc, "Parameter2", cmbParametre2);
                RemplirListeDepuisXML(xmlDoc, "Parameter3", cmbParametre3);
                RemplirListeDepuisXML(xmlDoc, "Parameter4", cmbParametre4);

                ChargerFichiersDepuisXML(xmlDoc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données depuis le fichier XML : {ex.Message}");
            }
        }

        private static void RemplirListeDepuisXML(XmlDocument xmlDoc, string nodeName, System.Windows.Controls.ComboBox comboBox)
        {
            XmlNodeList? nodes = xmlDoc.SelectNodes($"//{nodeName}/Item");
            var parametres = new List<Parametre>();
            if (nodes == null)
            {
                return;
            }
            foreach (XmlNode node in nodes)
            {
                string? libelle = node.SelectSingleNode("Libelle")?.InnerText;
                string? valeur = node.SelectSingleNode("Valeur")?.InnerText;

                if (libelle != null && valeur != null)
                {
                    parametres.Add(new Parametre(libelle, valeur));
                }
            }

            comboBox.ItemsSource = parametres;
            comboBox.DisplayMemberPath = "Libelle";
            comboBox.SelectedIndex = 0;
        }

        private void ChargerFichiersDepuisXML(XmlDocument xmlDoc)
        {
            try
            {
                XmlNodeList?  fileNodes = xmlDoc.SelectNodes("//Files/File");
                if (fileNodes == null)
                {
                    return;
                }
                foreach (XmlNode fileNode in fileNodes)
                {
                    string? fileName = fileNode.SelectSingleNode("FileName")?.InnerText;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        Button fileButton = new Button
                        {
                            Content = fileName,
                            Style = (Style)Resources["FileButtonStyle"]
                        };

                        fileButton.Click += async (sender, e) =>
                        {
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                            string commande = $"notepad++.exe {filePath}";
                            Console.WriteLine($"Commande : {commande}");
                            //string commande = $"C:\\Program Files\\Notepad++\\notepad++.exe {filePath}";
                            string sortie = await Task.Run(() => ExecuterCommandeShell(commande));
                            Console.WriteLine($"Sortie : {sortie}");
                            //Process.Start("notepad++.exe " + filePath);
                        };

                        fileListStackPanel.Children.Add(fileButton);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des fichiers depuis le fichier XML : {ex.Message}");
            }
        }

        private void ExecuterCommande_Click(object sender, RoutedEventArgs e)
        {
            Parametre? parametre1 = cmbParametre1.SelectedValue as Parametre;
            Parametre? parametre2 = cmbParametre2.SelectedValue as Parametre;
            Parametre? parametre3 = cmbParametre3.SelectedValue as Parametre;
            Parametre? parametre4 = cmbParametre4.SelectedValue as Parametre;

            if (parametre1 != null && parametre2 != null && parametre3 != null && parametre4 != null)
            {
                string commande = $"dir {parametre1.Valeur} {parametre2.Valeur} {parametre3.Valeur} {parametre4.Valeur}";

                txtCommande.Text = commande;

                string sortie = ExecuterCommandeShell(commande);
                txtSortie.Text = sortie;
            }
        }

        private static string ExecuterCommandeShell(string commande)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();

                StreamWriter sw = process.StandardInput;
                StreamReader sr = process.StandardOutput;

                sw.WriteLine(commande);
                sw.Close();

                string sortie = sr.ReadToEnd();
                sr.Close();

                //await Task.Run(() => process.WaitForExit());

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

