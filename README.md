# EfasScraper
Scrapes the daily lists of the Victorian Magistrates court and outputs a .json file with th information obtained.

## Why? ##
So it becomes easy to prove that the Magistrates' court is indeed scum and that they are purposefully causing delays 
in any hearing where a person pleads not guilty.

Run the PowerShell script court.ps1 and you will get a nice CSV file showing the number of hearings and the types of hearings per day.
***NOTE:*** This is currently hardcoded to the Frankston Court Criminal List, plans exist to extract the data of every single Court.

The goal is to build an analytics website showing the progression of matters through the Magistrates' Courts and to see if the Courts can live up to 
their performance guidelines and prove which Courts are creating unneccessary delays.

NO WE WILL NOT BE INCLUDING AS PART OF OUR ANALYTICS ANY NAMES OF THE PUBLIC.

We will however be including names of the Police informants, so that we can give a ranking on which officers take the most matters to court.
We will be collecting data such as how many of those cases progress to what stages, etc.
It will be a comprehensive analytics platform, based solely off the EFAS data.

If we could get access to a more reliable source, we would.

We also don't care about civil matters at this stage.

## Does the Court know about this? ##
Yes we wrote this to help an individual who had a matter before the Court who plead not guilty.
He got endless amounts of 4 month delays, purposefully designed to put improper pressure on him to plead guilty for something he wasn't at 
fault for.

He is fighting the system and is unable to get any help anywhere else.
This individual has tried every lawyer, even people like Julian Burnside QC who claim to be all about human rights.

The entire Court system in the States of Australia operate on the same corruption.
It's not any one person's fault it is just an entire system that has been unrestrained in the past doing what it wants.
This is because nobody calls any of it out on its lies and deception.
Rights are trampled by corrupt State Governments, sold under the guise of reasonability and manipulation of the wording to pretend they aren't infringing on rights.
This is in a country which tells the rest of the world how it supports human rights.

No, we were and may never activists for Human Rights.
It is bullying that we do not accept, especially from those that should know better.
Heaven help us, because the bullies have all the power and all we can do is slowly work to discrediting them.

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

