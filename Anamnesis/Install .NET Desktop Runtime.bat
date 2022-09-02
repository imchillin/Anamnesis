@echo off
title .NET Desktop Runtime Installer
echo The Microsoft .NET Desktop Runtime will be installed. Please do not close the console until it has completed. 
winget install Microsoft.DotNet.DesktopRuntime.6 -e -v 6.0.6 --accept-package-agreements
