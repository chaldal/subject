# purpose of this script is to create LibLifeCycleCoreBuild nuget package

param (
  # params are not used here but exist for convention
  $Version = '1.0.0-dev',
  $Registry = 'localhost:7777'
)

task Restore {
  exec { dotnet restore --interactive -r linux-x64 }
}

task CleanGenFiles {
  # delete sticky generated .cs files
  Get-ChildItem -Path './obj' -Recurse -Filter '*.cs' | ForEach-Object {
      Write-Output "Deleting: $($_.FullName)"
      Remove-Item -Force $_
  }
}


task Publish {
  exec { dotnet publish -o ./bin/publish --no-restore -c Release /p:ContinuousIntegrationBuild=true -r linux-x64 --no-self-contained }
}

task . Restore, CleanGenFiles, Publish
