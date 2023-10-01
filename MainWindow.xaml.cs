using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace mdt
{
    public partial class MainWindow : Window
    {
        static public string? fileEditor;
        static public string? commandName;
        static string setupFile = "config.xml";

        public MainWindow()
        {
            InitializeComponent();
            LoadSetupFromSetupFile();
        }

        void LoadSetupFromSetupFile()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(setupFile);

                LoadParameterListFromSetupFile(xmlDoc, "Parameter1", cmbParameter1);
                LoadParameterListFromSetupFile(xmlDoc, "Parameter2", cmbParameter2);
                LoadParameterListFromSetupFile(xmlDoc, "Parameter3", cmbParameter3);
                LoadParameterListFromSetupFile(xmlDoc, "Parameter4", cmbParameter4);

                LoadFileLinksFromSetupFile(xmlDoc);
                LoadParametersFromSetupFile(xmlDoc);
                LoadBookmarksFromSetupFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during setp loading from setup file : {ex.Message}");
            }
        }

        void LoadParametersFromSetupFile(XmlDocument xmlDoc)
        {
            try
            {
                fileEditor = xmlDoc.SelectSingleNode("//FileEditor")?.InnerText;
                if (fileEditor == null)
                {
                    MessageBox.Show($"No file editor parameter defined in setup file");
                    return;
                }
                commandName = xmlDoc.SelectSingleNode("//Command")?.InnerText;
                if (commandName == null)
                {
                    MessageBox.Show($"No system command defined in setup file");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during global parameters loading from setup file : {ex.Message}");
            }
        }

        private void LoadBookmarksFromSetupFile()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(setupFile);

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
                MessageBox.Show($"Error during bookmarks loading from setup file : {ex.Message}");
            }
        }


        void LoadParameterListFromSetupFile(XmlDocument xmlDoc, string nodeName, System.Windows.Controls.ComboBox comboBox)
        {
            XmlNodeList? nodes = xmlDoc.SelectNodes($"//{nodeName}/Item");
            var parameters = new List<Parameter>();
            if (nodes == null)
            {
                return;
            }
            foreach (XmlNode node in nodes)
            {
                string? description = node.SelectSingleNode("Description")?.InnerText;
                string? value = node.SelectSingleNode("Value")?.InnerText;

                if (description != null && value != null)
                {
                    parameters.Add(new Parameter(description, value));
                }
            }

            comboBox.ItemsSource = parameters;
            comboBox.DisplayMemberPath = "description";
            comboBox.SelectedIndex = 0;
        }


        private void LoadFileLinksFromSetupFile(XmlDocument xmlDoc)
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
                MessageBox.Show($"Error during file links loading from setup file : {ex.Message}");
            }
        }

        private void RunBookmarkCommand_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            BookmarkItem bookmarkItem = (BookmarkItem)button.DataContext;

            // Construisez la commande à partir des paramètres du bookmark
            string commande = $"{commandName} {bookmarkItem.Parameter1} {bookmarkItem.Parameter2} {bookmarkItem.Parameter3} {bookmarkItem.Parameter4}";

            txtBookmarkCommand.Text = commande;

            string output = RunShellCommand(commande);
            txtBookmarkCommandOutput.Text = output;
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("File doesn't exist.");
            }
        }

        void RunCommand_Click(object sender, RoutedEventArgs e)
        {
            Parameter? parameter1 = cmbParameter1.SelectedValue as Parameter;
            Parameter? parameter2 = cmbParameter2.SelectedValue as Parameter;
            Parameter? parameter3 = cmbParameter3.SelectedValue as Parameter;
            Parameter? parameter4 = cmbParameter4.SelectedValue as Parameter;

            if (parameter1 != null && parameter2 != null && parameter3 != null && parameter4 != null)
            {
                string commande = $"{commandName} {parameter1.value} {parameter2.value} {parameter3.value} {parameter4.value}";

                txtCommande.Text = commande;

                string output = RunShellCommand(commande);
                txtCommandOutput.Text = output;
            }
        }

        static string RunShellCommand(string commande)
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

                string output = sr.ReadToEnd();
                sr.Close();

                //await Task.Run(() => process.WaitForExit());

                return output;
            }
            catch (Exception ex)
            {
                return $"Error during command execution : {ex.Message}";
            }
        }

        void RemoveBookmark_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            BookmarkItem bookmarkItem = (BookmarkItem)button.DataContext;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(setupFile);

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
                    xmlDoc.Save(setupFile);

                    // Rechargez la liste des bookmarks
                    LoadBookmarksFromSetupFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during bookmark removing : {ex.Message}");
            }
        }

        void RemoveFileLink_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            FileItem fileItem = (FileItem)button.DataContext;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(setupFile);

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
                    xmlDoc.Save(setupFile);

                    // Rechargez la liste des bookmarks
                    LoadFileLinksFromSetupFile(xmlDoc);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during bookmark removing : {ex.Message}");
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
                    xmlDocFiles.Load(setupFile);

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
                    xmlDocFiles.Save(setupFile);
                    LoadFileLinksFromSetupFile(xmlDocFiles);
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

        void RegistrerBookmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Parameter? selectedParameter1 = cmbParameter1.SelectedValue as Parameter;
                Parameter? selectedParameter2 = cmbParameter2.SelectedValue as Parameter;
                Parameter? selectedParameter3 = cmbParameter3.SelectedValue as Parameter;
                Parameter? selectedParameter4 = cmbParameter4.SelectedValue as Parameter;
                string description = txtDescription.Text; // Obtenir la description depuis le champ de saisie

                if (selectedParameter1 != null && selectedParameter2 != null && selectedParameter3 != null && selectedParameter4 != null)
                {
                    // Créez un nouvel élément XML pour le bookmark avec les paramètres et la description
                    XmlDocument xmlDocBookmarks = new XmlDocument();
                    xmlDocBookmarks.Load(setupFile);

                    XmlElement? bookmarks = xmlDocBookmarks.SelectSingleNode("//Bookmarks") as XmlElement;

                    XmlElement bookmark = xmlDocBookmarks.CreateElement("Bookmark");
                    bookmarks?.AppendChild(bookmark);

                    XmlElement bookmarkDescription = xmlDocBookmarks.CreateElement("Description");
                    bookmarkDescription.InnerText = description;
                    bookmark.AppendChild(bookmarkDescription);

                    XmlElement parameter1 = xmlDocBookmarks.CreateElement("Parameter1");
                    if (selectedParameter1.value != null)
                    {
                        parameter1.InnerText = selectedParameter1.value;
                        bookmark.AppendChild(parameter1);
                    }

                    if (selectedParameter2.value != null)
                    {
                        XmlElement parameter2 = xmlDocBookmarks.CreateElement("Parameter2");
                        parameter2.InnerText = selectedParameter2.value;
                        bookmark.AppendChild(parameter2);
                    }

                    if (selectedParameter3.value != null)
                    {
                        XmlElement parameter3 = xmlDocBookmarks.CreateElement("Parameter3");
                        parameter3.InnerText = selectedParameter3.value;
                        bookmark.AppendChild(parameter3);
                    }

                    if (selectedParameter4.value != null)
                    {
                        XmlElement parameter4 = xmlDocBookmarks.CreateElement("Parameter4");
                        parameter4.InnerText = selectedParameter4.value;
                        bookmark.AppendChild(parameter4);
                    }

                    xmlDocBookmarks.SelectSingleNode("//Bookmarks")?.AppendChild(bookmark);

                    xmlDocBookmarks.Save(setupFile);
                    LoadBookmarksFromSetupFile();
                    txtDescription.Text = "";

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during bookmark registering : {ex.Message}");
            }
        }
    }
    public class FileItem
    {
        public string? FileName { get; set; }
        public string? FileDescription { get; set; }
    }
    public class Parameter
    {
        public string? description { get; set; }
        public string? value { get; set; }
        public Parameter(string paramDescription, string paramValue)
        {
            description = paramDescription;
            value = paramValue;
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

