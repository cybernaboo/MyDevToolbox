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
        static public string? fileEditor;
        static public string? commandName;
        static string cheminFichierConfig = "config.xml";

        public MainWindow()
        {
            InitializeComponent();
            ChargerDonneesDepuisXML();
            ChargerBookmarksDepuisXML();
        }

        void ChargerDonneesDepuisXML()
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
                ChargerParametresDepuisXML(xmlDoc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données depuis le fichier XML : {ex.Message}");
            }
        }

        private void ChargerBookmarksDepuisXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(cheminFichierConfig);

                XmlNodeList? bookmarkNodes = xmlDoc.SelectNodes("//Bookmarks/Bookmark");
                if (bookmarkNodes == null)
                {
                    return;
                }

                List<BookmarkItem> bookmarks = new List<BookmarkItem>();

                foreach (XmlNode bookmarkNode in bookmarkNodes)
                {
                    if (bookmarkNode.SelectSingleNode("Description") != null) {
                        string? description = bookmarkNode.SelectSingleNode("Description")?.InnerText;
                        string? parameter1 = bookmarkNode.SelectSingleNode("Parameter1")?.InnerText;
                        string? parameter2 = bookmarkNode.SelectSingleNode("Parameter2")?.InnerText;
                        string? parameter3 = bookmarkNode.SelectSingleNode("Parameter3")?.InnerText;
                        string? parameter4 = bookmarkNode.SelectSingleNode("Parameter4")?.InnerText;
                        BookmarkItem? newBookmark = new BookmarkItem { Description = description, Parameter1 = parameter1, Parameter2 = parameter2, Parameter3 = parameter3, Parameter4 = parameter4 };
                        bookmarks.Add(newBookmark);
                    }
                }
                if (bookmarks != null)
                {
                    listViewBookmarks.ItemsSource = bookmarks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des bookmarks depuis le fichier XML : {ex.Message}");
            }
        }

        void ChargerParametresDepuisXML(XmlDocument xmlDoc)
        {
            try
            {
                fileEditor = xmlDoc.SelectSingleNode("//FileEditor")?.InnerText;
                if (fileEditor == null)
                {
                    MessageBox.Show($"Pas de paramètre Editeur de fichier défini dans le fichier de paramétrage");
                    return;
                }
                commandName = xmlDoc.SelectSingleNode("//Command")?.InnerText;
                if (commandName == null)
                {
                    MessageBox.Show($"Pas de Commande système défini dans le fichier de paramétrage");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des paramètres depuis le fichier XML : {ex.Message}");
            }
        }

        void RemplirListeDepuisXML(XmlDocument xmlDoc, string nodeName, System.Windows.Controls.ComboBox comboBox)
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
            comboBox.DisplayMemberPath = "libelle";
            comboBox.SelectedIndex = 0;
        }


        private void ChargerFichiersDepuisXML(XmlDocument xmlDoc)
        {
            try
            {
                XmlNodeList fileNodes = xmlDoc.SelectNodes("//Files/File");
                if (fileNodes == null)
                {
                    return;
                }

                List<FileItem> files = new List<FileItem>();

                foreach (XmlNode fileNode in fileNodes)
                {
                    string? fileName = fileNode.SelectSingleNode("FileName")?.InnerText;
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        files.Add(new FileItem { FileName = fileName });
                    }
                }

                listViewFiles.ItemsSource = files;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des fichiers depuis le fichier XML : {ex.Message}");
            }
        }

        private void ExecuterCommandeBookmark_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            BookmarkItem bookmarkItem = (BookmarkItem)button.DataContext;

            // Construisez la commande à partir des paramètres du bookmark
            string commande = $"{commandName} {bookmarkItem.Parameter1} {bookmarkItem.Parameter2} {bookmarkItem.Parameter3} {bookmarkItem.Parameter4}";

            txtCommandeBookmark.Text = commande;

            string sortie = ExecuterCommandeShell(commande);
            txtSortieCmdBookmark.Text = sortie;
        }

        private void OuvrirFichier_Click(object sender, RoutedEventArgs e)
        {
            // Obtenir l'élément de la liste associé au bouton cliqué
            Button button = (Button)sender;
            FileItem fileItem = (FileItem)button.DataContext;

            // Vérifier si le fichier existe avant de l'ouvrir
            if (File.Exists(fileItem.FileName))
            {
                string fileName = Path.Combine(Directory.GetCurrentDirectory(), fileItem.FileName);

                // Ici, vous pouvez utiliser Process.Start pour ouvrir le fichier avec l'éditeur de votre choix
                // par exemple, pour ouvrir avec Notepad++ :
                Process.Start("notepad++.exe", fileName);
            }
            else
            {
                MessageBox.Show("Le fichier n'existe pas.");
            }
        }

        void ExecuterCommande_Click(object sender, RoutedEventArgs e)
        {
            Parametre? parametre1 = cmbParametre1.SelectedValue as Parametre;
            Parametre? parametre2 = cmbParametre2.SelectedValue as Parametre;
            Parametre? parametre3 = cmbParametre3.SelectedValue as Parametre;
            Parametre? parametre4 = cmbParametre4.SelectedValue as Parametre;

            if (parametre1 != null && parametre2 != null && parametre3 != null && parametre4 != null)
            {
                string commande = $"{commandName} {parametre1.valeur} {parametre2.valeur} {parametre3.valeur} {parametre4.valeur}";

                txtCommande.Text = commande;

                string sortie = ExecuterCommandeShell(commande);
                txtSortieCmd.Text = sortie;
            }
        }

        static string ExecuterCommandeShell(string commande)
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

        void SupprimerBookmark_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            BookmarkItem bookmarkItem = (BookmarkItem)button.DataContext;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(cheminFichierConfig);

                // Trouvez le nœud de bookmark correspondant dans le XML
                XmlNode? bookmarksNode = xmlDoc.SelectSingleNode("//Bookmarks");
                XmlNodeList? bookmarkNodes = bookmarksNode?.SelectNodes("Bookmark");

                if (bookmarkNodes != null)
                {
                    foreach (XmlNode node in bookmarkNodes)
                    {
                        XmlNode? descriptionNode = node.SelectSingleNode("Description");
                        if (descriptionNode != null && descriptionNode.InnerText == bookmarkItem.Description)
                        {
                            // Supprimez le nœud du bookmark trouvé
                            bookmarksNode?.RemoveChild(node);
                            break;
                        }
                    }

                    // Enregistrez les modifications dans le fichier XML
                    xmlDoc.Save(cheminFichierConfig);

                    // Rechargez la liste des bookmarks
                    ChargerBookmarksDepuisXML();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression du bookmark : {ex.Message}");
            }
        }

        void RemoveFileLink_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FileItem fileItem = (FileItem)button.DataContext;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(cheminFichierConfig);

                // Trouvez le nœud de bookmark correspondant dans le XML
                XmlNode? filesNode = xmlDoc.SelectSingleNode("//Files");
                XmlNodeList? filesNodes = filesNode?.SelectNodes("File");

                if (filesNodes != null)
                {
                    foreach (XmlNode node in filesNodes)
                    {
                        XmlNode? fileNameNode = node.SelectSingleNode("FileName");
                        if (fileNameNode != null && fileNameNode.InnerText == fileItem.FileName)
                        {
                            // Supprimez le nœud du bookmark trouvé
                            filesNode?.RemoveChild(node);
                            break;
                        }
                    }

                    // Enregistrez les modifications dans le fichier XML
                    xmlDoc.Save(cheminFichierConfig);

                    // Rechargez la liste des bookmarks
                    ChargerFichiersDepuisXML(xmlDoc);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression du bookmark : {ex.Message}");
            }
        }

        void AddFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String? fileName = txtNewFileName.Text;
                String? fileDescription = txtNewFileDescription.Text;
                if (fileName != null && fileDescription != null)
                {
                    // Créez un nouvel élément XML pour le fichier avec le nom et la description
                    XmlDocument xmlDocFiles = new XmlDocument();
                    xmlDocFiles.Load(cheminFichierConfig);

                    XmlElement? files = xmlDocFiles.SelectSingleNode("//Files") as XmlElement;

                    XmlElement file = xmlDocFiles.CreateElement("File");
                    files?.AppendChild(file);

                    XmlElement fileNameElement = xmlDocFiles.CreateElement("FileName");
                    fileNameElement.InnerText = fileName;
                    file.AppendChild(fileNameElement);

                    XmlElement fileDescriptionElement = xmlDocFiles.CreateElement("FileDescription");
                    fileDescriptionElement.InnerText = fileDescription;
                    file.AppendChild(fileDescriptionElement);

                    xmlDocFiles.SelectSingleNode("//Files")?.AppendChild(file);

                    // Sauvegardez le fichier XML mis à jour
                    xmlDocFiles.Save(cheminFichierConfig);
                    ChargerFichiersDepuisXML(xmlDocFiles);
                    // Réinitialiser les champs de saisie wpf
                    txtNewFileName.Text = "";
                    txtNewFileDescription.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during file link registering : {ex.Message}");
            }
        }

        void EnregistrerDansBookmarks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Parametre? parametre1 = cmbParametre1.SelectedValue as Parametre;
                Parametre? parametre2 = cmbParametre2.SelectedValue as Parametre;
                Parametre? parametre3 = cmbParametre3.SelectedValue as Parametre;
                Parametre? parametre4 = cmbParametre4.SelectedValue as Parametre;
                string description = txtDescription.Text; // Obtenir la description depuis le champ de saisie

                if (parametre1 != null && parametre2 != null && parametre3 != null && parametre4 != null)
                {
                    // Créez un nouvel élément XML pour le bookmark avec les paramètres et la description
                    XmlDocument xmlDocBookmarks = new XmlDocument();
                    xmlDocBookmarks.Load(cheminFichierConfig);

                    XmlElement? bookmarks = xmlDocBookmarks.SelectSingleNode("//Bookmarks") as XmlElement;

                    XmlElement bookmark = xmlDocBookmarks.CreateElement("Bookmark");
                    bookmarks?.AppendChild(bookmark);

                    XmlElement bookmarkDescription = xmlDocBookmarks.CreateElement("Description");
                    bookmarkDescription.InnerText = description;
                    bookmark.AppendChild(bookmarkDescription);

                    XmlElement parameter1 = xmlDocBookmarks.CreateElement("Parameter1");
                    if (parametre1.valeur != null)
                    {
                        parameter1.InnerText = parametre1.valeur;
                        bookmark.AppendChild(parameter1);
                    }

                    if (parametre2.valeur != null)
                    {
                        XmlElement parameter2 = xmlDocBookmarks.CreateElement("Parameter2");
                        parameter2.InnerText = parametre2.valeur;
                        bookmark.AppendChild(parameter2);
                    }

                    if (parametre3.valeur != null)
                    {
                        XmlElement parameter3 = xmlDocBookmarks.CreateElement("Parameter3");
                        parameter3.InnerText = parametre3.valeur;
                        bookmark.AppendChild(parameter3);
                    }

                    if (parametre4.valeur != null)
                    {
                        XmlElement parameter4 = xmlDocBookmarks.CreateElement("Parameter4");
                        parameter4.InnerText = parametre4.valeur;
                        bookmark.AppendChild(parameter4);
                    }

                    xmlDocBookmarks.SelectSingleNode("//Bookmarks")?.AppendChild(bookmark);

                    // Sauvegardez le fichier XML mis à jour
                    xmlDocBookmarks.Save(cheminFichierConfig);
                    ChargerBookmarksDepuisXML();
                    // Réinitialiser les champs de saisie wpf bookmarkDescription
                    txtDescription.Text = "";



                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement du bookmark : {ex.Message}");
            }
        }
    }
    public class FileItem
    {
        public string? FileName { get; set; }
        public string? FileDescription { get; set; }
    }
    public class Parametre
    {
        public string? libelle { get; set; }
        public string? valeur { get; set; }
        public Parametre(string parmLibelle, string parmValeur)
        {
            libelle = parmLibelle;
            valeur = parmValeur;
        }
    }
    public class BookmarkItem
    {
        public string? Description { get; set; }
        public string? Parameter1 { get; set; }
        public string? Parameter2 { get; set; }
        public string? Parameter3 { get; set; }
        public string? Parameter4 { get; set; }
    }

}

