@echo off
title .NET Desktop Runtime Installer
echo The Microsoft .NET Desktop Runtime will be installed. Please do not close the console until it has completed.
winget install Microsoft.DotNet.DesktopRuntime.9 -e -v 9.0.1 --accept-package-agreements
