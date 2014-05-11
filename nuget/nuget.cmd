@echo off
set nuget=nuget.app
set wd=%~dp0
echo Packing package
%nuget% Pack "%wd%OpenLibrary.nuspec" -Verbosity detailed -OutputDirectory %wd%
%nuget% Pack "%wd%OpenLibrary-Document.nuspec" -Verbosity detailed -OutputDirectory %wd%
%nuget% Pack "%wd%OpenLibrary-MVC.nuspec" -Verbosity detailed -OutputDirectory %wd%
%nuget% Pack "%wd%OpenLibrary-Contrib.nuspec" -Verbosity detailed -OutputDirectory %wd%
echo Finished
pause>nul
@echo on