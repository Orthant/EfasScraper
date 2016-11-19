# EfasScraper
Scrapes the daily lists of the Victorian Magistrates court and outputs a .json file with th information obtained.

## Why? ##
So it becomes easy to prove that the Magistrates' court is indeed scum and that they are purposefully causing delays 
in any hearing where a person pleads not guilty.

Run the PowerShell script court.ps1 and you will get a nice CSV file showing the number of hearings and the types of hearings per day.
***NOTE:*** This is currently hardcoded to the Frankston Court Criminal List, plans exist to extract the data of every single Court.

The goal is to build an analytics website showing the progression of matters through the Courts and to see if the Courts can live up to 
their performance guidelines and prove which Courts are creating unneccessary delays.

## Does the Court know about this? ##
Yes, I have had a matter before the Court and 


## Files

*** Program.cs ***
The code for running in C# .NET using Parallel processing.
THe Parallel processing may not work on Windows 8/8.1, due to an issue with
multithreated Console.WriteLine() calls.  
  
For this you are out of luck, because I don't see a reason to accomodate you.

This will take about 2 minutes to run.

***NOTE:*** Running this will open thousands of connections that are only open for
under a second or so. If you get the Police come to your door accussing you of trying
to hack or whatever the court's daily lists, that's your fault not mine.


*** court.ps1 ***
PowerShell script, this will go through sequentially, not parallel and thus is a bit safer to run.
Although it will take a lot longer, around 20-30 mins? I haven't timed it to know.

This will probably be a lot safer to run but doesn't give quite as much detail and spits out a CSV
file.

