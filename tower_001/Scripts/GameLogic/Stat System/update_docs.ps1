# PowerShell script to update CharacterStatSystem documentation
$sourcePath = "CharacterStatSystem.docs.txt"
$targetPath = "CharacterStatSystem.cs"

# Read the documentation file
$docs = Get-Content $sourcePath -Raw

# Read the target file
$content = Get-Content $targetPath -Raw

# Create backup
Copy-Item $targetPath "$targetPath.bak"

# Replace the class documentation
$content = $content -replace '(?s)\/\/\/ <summary>.*?public class CharacterStatSystem', $docs

# Save the changes
$content | Set-Content $targetPath -NoNewline
