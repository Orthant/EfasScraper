# Put the existing location on the stack to return to it after
Push-Location -Path $pwd.Path -StackName EfasScraper

# Move to the EfasExtracts folder
Set-Location -Path ~\EfasExtracts

# Import the assembly to give us access to the native .NET JavaScriptSerializer
Import-Module -Assembly ([System.Reflection.Assembly]::LoadWithPartialName('System.Web.Extensions'))

# Set the user agent to mask what we are doing, but also provide a clue if they check the logs
$userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36 EfasScraper/0.95.3.2126";

# Create the JavaScriptSerializer
$json = New-Object System.Web.Script.Serialization.JavaScriptSerializer;

# Create an arraylist to hold the items
$itmList = New-Object System.Collections.ArrayList;

# Set the current date/time we started
$started = [System.DateTime]::Now;

# The day without hours & minutes for purposes of running through the days
$startDate = [System.DateTime]::Today;

# This stores all the various hearing types we come across
$hearTypeKeys = New-Object System.Collections.ArrayList;

# The number of days to process, hard capped at a year
$numberOfDays = 365;

# Loop through the days, starting at today
for ( $dayModifier = 0; $dayModifier -lt $numberOfDays -and $emptyCount -le 10; $dayModifier++) {

  # Work out the date
  $lookupDate = $startDate.AddDays($dayModifier);

  # Work out our percentage of days processed
  $dayModifierPercent = [float](($dayModifier / ($numberOfdays * 4))); # Four steps, verify date, load criminal, load civil, reformat data

  # Write a progress bar to the screen
  Write-Progress -Id 1 -Activity ("Loading daily data") -Status ("{0:d}, {1:p}% Complete" -f $lookupDate, $dayModifierPercent) -PercentComplete ($dayModifierPercent * 100) -CurrentOperation "Checking date is valid...";

  # Skip saturdays and sundays
  # if ($lookupDate.DayOfWeek -iin ('Saturday', 'Sunday')) { continue; }

  # Work out the percentage of days processed
  $dayModifierPercent = [float](($dayModifier + 1) / ($numberOfDays * 4));

  # Update the progress bar
  Write-Progress -Id 1 -Activity ("Loading daily data") -Status ("{0:d}, {1:p}% Complete" -f $lookupDate, $dayModifierPercent) -PercentComplete ($dayModifierPercent * 100) -CurrentOperation "Obtaining criminal cases...";

  # Generate the URL
  $url = "https://dailylists.magistratesvic.com.au/EFAS/CaseBrowse_Cases_GridData?CaseType=CRI&CourtID=4&HearingDate={0:MM\%\2\Fdd\%\2\Fyyyy\%\2\0\0\0\%\3\A\0\0\%\3\A\0\0}" -f $lookupDate;

  #  Make the web request
  $dayModifierRequest = Invoke-WebRequest -Uri $url -Method Post -Body "sort=CourtLinkCaseNo-desc&page=1&pageSize=500&group=&filter=" -ContentType "application/x-www-form-urlencoded; charset=UTF-8" -UserAgent $userAgent;

  # Deserialize the object
  $criminalMatters = $json.DeserializeObject($dayModifierRequest.Content);

  # Move the Data part into the actual object for ease
  $criminalMatters = $criminalMatters.Data;

  # Create the daily data hash table
  $dailyData = @{ 'Date' = $lookupDate; 'CriminalTotal' = ($criminalMatters.Length) };

  # Calculate the day progress percentage
  $dayModifierPercent = [float](($dayModifier + 2) / ($numberOfDays * 4));

  # Update the main progress bar
  Write-Progress -Id 1 -Activity ("Loading daily data") -Status ("{0:d}, {1:p}% Complete" -f $lookupDate, $dayModifierPercent) -PercentComplete ($dayModifierPercent * 100) -CurrentOperation "Loading criminal hearings...";

  # Create the secondary progress bar
  Write-Progress -Id 2 -Activity ("Loading criminal matters") -PercentComplete 0 -Status ("{0:p}% Complete" -f 0.00) -CurrentOperation "Initializing..." -ParentId 1;

  # Start looping through the cases
  for ($itemsParsed = 0; $itemsParsed -lt $criminalMatters.Length; $itemsParsed++ ) {
        # Grab the particularrs of the case
        $cmItem = $criminalMatters[$itemsParsed];

        # Update the secondary progress bar
        Write-Progress -Id 2 -Activity("Loading criminal matters") -PercentComplete (($itemsParsed / $criminalMatters.Length) * 100) -Status ("{0:p}% Complete" -f ($itemsParsed / $criminalMatters.Length)) -CurrentOperation ("Loading hearing " + $cmItem.CaseID) -ParentId 1;

        # Grab the data for the hearing
        $hearingRequest = (Invoke-WebRequest  -Uri ("https://dailylists.magistratesvic.com.au/EFAS/CaseCRI?CaseID={0}" -f $cmItem.CaseID) -UserAgent "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");

        # Extract all the TD elements of the HTML
        $elements = $hearingRequest.AllElements | ? { $_.TagName -eq 'TD' };

        # Loop through each of the elements
        for ($e = 0; $e  -lt $elements.Length; $e++) {
            # Fine the element that contains the Hearing Type phrase
            if ($elements[$e].innerText -ilike '*Hearing*Type*') {
                # If found, grab the inner text of the next TD element which will be the hearing type
                $hearingType = $elements[$e + 1].innerText.Trim(' ');
                # If the hearing type isn't in the list of keys we have, then we should add it to that list
                if ($hearingType -inotin $hearTypeKeys) { [void]$hearTypeKeys.Add($hearingType); }
                # If the daily data already has the hearing type in it, increase it's count by 1
                if ($dailyData.ContainsKey($hearingType)) { $dailyData[$hearingType] += 1; }
                # Otherwise just add it to the hearing types with an initial count of 1
                else { [void]$dailyData.Add($hearingType, 1); }
                # jump out of the elements if we have found the hearing type already
                break;
            }
        }
    }

    # Remove the secondary progress bar
    Write-Progress -Id 2 -Activity ("Loading criminal matters") -Completed;

    # Create the URL for civil matters
    $url = "https://dailylists.magistratesvic.com.au/EFAS/CaseBrowse_Cases_GridData?CaseType=CIV&CourtID=4&HearingDate={0:MM\%\2\Fdd\%\2\Fyyyy\%\2\0\0\0\%\3\A\0\0\%\3\A\0\0}" -f $lookupDate;

    # Invoke the request for civil matters
    $dayModifierRequest = Invoke-WebRequest -Uri $url -Method Post -Body "sort=CourtLinkCaseNo-desc&page=1&pageSize=500&group=&filter=" -ContentType "application/x-www-form-urlencoded; charset=UTF-8" -UserAgent $userAgent;

    # Deserialize the object
    $result = $json.DeserializeObject($webRequest.Content);

    # We just want the civil total
    $dailyData.Add('CivilTotal', $result.Total);

    # Add up the totals of the total
    $dailyData.Add('Total', $dailyData.CivilTotal + $dailyData.CriminalTotal);

    # Go through each of the hearing type keys, and if the daily data does not contain it,
    # add the hearing type key to the daily data (we repeat this at the end because we don't recurse
    # backwards on this, so hearing type keys added later will only go on later days
    $hearTypeKeys | ? { -not $dailyData.ContainsKey($_) } | % { [void] $dailyData.Add($_, 0); }

    # Add the daily data to the item list
    [void]$itmList.Add($dailyData);
    # Clear the screen
    Clear-Host
    # Move the cursor below the progress bar so it's visible when written to the screen
    Write-Host ([String]::Join([Environment]::NewLine, [System.Linq.Enumerable]::Repeat("", 12)));
    # Write the daily data to the screen
    $dailyData | Ft * -Force
}
# Update the progress bar
Write-Progress -Id 1 -Activity ("Loading daily data") -Completed;

# Out-Default $itmList;

# Loop through the items in the list adding any missing hearing type keys
$items = ($itmList | % {
  # Create a variable to hold the current item from itmList
  $itm = $_;
  # Loop through the hearing type keys
  $hearTypeKeys | % {
    # if the item from the itemList doesn't contain the hearing type key
    if ( -not $itm.ContainsKey($_) ) {
      # Add it to the item
      $itm.Add($_, 0);
    }
  };
  return ( New-Object -TypeName PSObject -Property $itm );
});


$items | Export-Csv -Path ([string]::Format('{0}-dailylists.csv', $started.ToString('yyyyMMdd-hhmm'))) -NoTypeInformation -Force -Encoding utf8;

# Return to where we were
Pop-Location -StackName EfasScraper
