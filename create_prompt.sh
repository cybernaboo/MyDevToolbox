cat intro_prompt_chatgpt.txt > prompt_chatgpt.txt
echo -e "\n*** Fichier MainWindow.xaml ***\n" >> prompt_chatgpt.txt
cat MainWindow.xaml >> prompt_chatgpt.txt
echo -e "\n*** Fichier MainWindow.xaml.cs ***\n" >> prompt_chatgpt.txt
cat MainWindow.xaml.cs >> prompt_chatgpt.txt
echo -e "\n*** Fichier config.xml ***\n" >> prompt_chatgpt.txt
cat config.xml >> prompt_chatgpt.txt
