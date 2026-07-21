$dir_path = 'C:\Users\KEVINE\Downloads\DigitalLoanPlatform2\DigitalLoanPlatform2\Web\Components\Pages'
$files = Get-ChildItem -Path $dir_path -Filter *.razor

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    if ($content -match '<MudDatePicker') {
        # Replace occurrences of <MudDatePicker where ShowToolbar is not already present
        # We can just inject ShowToolbar="false" right after <MudDatePicker
        $newContent = [regex]::Replace($content, '(<MudDatePicker(?![^>]*ShowToolbar="false"))', '$1 ShowToolbar="false"')
        
        if ($content -ne $newContent) {
            Write-Host "Modified $($file.Name)"
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
        }
    }
}
