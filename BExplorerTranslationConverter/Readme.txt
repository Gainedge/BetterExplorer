Greetings.

This is the source code (with a compiled copy) of the Translation Manager I began using in BetterExplorer 2.0.0.016 Beta.

It has two important features:

1. Conversion between its .txt "Base File" format (the format used on the website and in the program) and the XML/XAML format used in BetterExplorer.

2. Quick addition or removal of translation entries, or editing of existing ones.

Unfortunately, this copy does not contain BetterExplorer's default base file data that is usually used in this program. You can create a default file from the DefaultLocale.xaml file in BetterExplorer's source code. Otherwise, you can create a default file by copying the template on the website and saving that as a text document, and importing that.

----------------------------------------------------

This is how I suggest using the program:

When you're first starting the program and you have an XAML file (such as DefaultLocale.xaml) to use, you can import it using the "Default..." button. It will open in the program, but the "Location" values need to be entered manually. (DO NOT USE "Load XML...".)

To export the current default values as an XAML file (creating an XAML file where the translated values are the same as the default values), use the "Def XML..." button and save the file.

To import a base file, click the "Load..." button. To save the current data as a base file, click the "Save..." button.

Now, to import an existing XAML file and add its information in, use the "Load XML..." button and select the file.

To save the current data as an XAML file, click the "Save XML..." button and save the file.

----------------------------------------------------

While editing the entries in the program, you have the following options available for you:

1. "Search..." Search for an entry that has the same Name (ID). Note that it only searches their names, and nothing else.

2. "Add Value..." Create a new entry. It will be added at the bottom of the list.

3. "Edit Value..." If you select an existing entry, and either double-click it, press the "Enter" key, or press the "Edit Value..." button, you can edit all of the data for this entry.

4. "Delete..." If you select an existing entry, and either press the "Delete" key or press the "Delete..." button, you can delete this entry. An error message will appear, but just click "OK" and the item will be deleted.

5. "Refresh" The program contains two lists of translation entries at any one time: the "internal" list that contains all of the data, and the "read-only" list that is displayed to you, the user. Pressing "Refresh" forces the "read-only" list to load data from the "internal" list again. Usually, this button will not be used, but better safe than sorry. :)

----------------------------------------------------

Created June 16, 2012. Updated June 25, 2012.
Jayke R. Huempfner. (JaykeBird)