#!/bin/bash

# Nom du fichier XML
xml_file="commands.xml"

# Nom du fichier de sortie
output_file="output.txt"

# Utilisation de grep pour extraire les lignes contenant "fromCommand" et awk pour mettre en forme la sortie
grep 'fromCommand' "$xml_file" | awk -F'"' '/fromCommand/{split($2, arr, "\\\\"); printf("  <Item>\n    <Libelle>%s</Libelle>\n    <Valeur>%s</Valeur>\n  </Item>\n", arr[length(arr)], arr[length(arr)])}' > "formatted_output.txt"

echo "Le fichier formaté a été enregistré dans formatted_output.txt"
