BetterExplorer
==============

A Better Explorer filemanager repository


How to build and test Better Explorer
======================================
Important: This process includes the installation of Visual C# 2010 Express, which in itself may take up to an hour to do, and will take up space on your hard drive (not a lot, but it's not a small program). If you don't want to deal with this, I understand. :) However, an alternative named SharpDevelop appears to work as well, but I've never tried it.

Anyway, here's how it's done (in just 3 steps!):

1. Download and install Visual C# Express 2010 or SharpDevelop.
I use Visual C# Express, so my instructions will assume you're using it too, but SharpDevelop doesn't seem that drastically different, so just read ahead.

2. Get the Source Code
On the Source Code page of this website (the link's right here and at the top of every page on this website), click on the Download link on the right side to download the latest build. (If you want a specific build, click on its number and then click on the Download link on the top of that page.) Unzip the archive into a new, empty folder.

3. Start Up the Program
Now that you downloaded and unzipped the source code, open the folder you created, and you should see four folders and one file inside it. Double-click on the file (called "BExplorer.sln") and it will open up in Visual C# Express or SharpDevelop (or at least, it should...). Wait for it to finish loading, and click on "OK" on anything that appears. (For BetterExplorer, they can be ignored.) Then, before doing anything else, open the Build menu and select "Build Solution". This will make the program, and if any errors occur, check back on the Source Code page within 10 minutes to see if we posted any fixes.

To access the program, go back to the folder you should still have open and open the "BExplorer" folder, followed by the "BetterExplorer" folder, and then open the newly-made "bin" folder and then the "Debug" folder inside of that one. In the Debug folder, you'll find the program you just built!