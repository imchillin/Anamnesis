// Launcher.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <cstdio>
#include <memory>
#include <stdexcept>
#include <string>
#include <array>
#include <windows.h>

#include "resource.h"

#pragma comment(lib, "urlmon.lib")

using namespace std;

string exec(const char* cmd)
{
    array<char, 128> buffer;
    string result;
    unique_ptr<FILE, decltype(&_pclose)> pipe(_popen(cmd, "r"), _pclose);

    if (!pipe)
    {
        throw runtime_error("popen() failed!");
    }

    while (fgets(buffer.data(), buffer.size(), pipe.get()) != nullptr)
    {
        result += buffer.data();
    }

    return result;
}

std::string GetCurrentDirectory()
{
    char buffer[MAX_PATH];
    GetModuleFileNameA(NULL, buffer, MAX_PATH);
    std::string::size_type pos = std::string(buffer).find_last_of("\\/");

    return std::string(buffer).substr(0, pos);
}

int main()
{
    HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
    DWORD mode = 0;
    GetConsoleMode(hStdin, &mode);
    SetConsoleMode(hStdin, mode & (~ENABLE_ECHO_INPUT));


    cout << "Check .Net desktop runtime\n";
    string result = exec("dotnet --info");

    if (result.find("Microsoft.WindowsDesktop.App 5.0.6") == std::string::npos)
    {
        cout << "Desktop runtime not found\n";

        string setupFile = GetCurrentDirectory().append("\\setupdotnet5.exe");

        wstring dwnld_URL = L"https://download.visualstudio.microsoft.com/download/pr/6279dc90-f437-4481-82a5-73dd9f97da06/6519ef44735fd31115b9b1a81d6ff1e8/windowsdesktop-runtime-5.0.6-win-x64.exe";
        wstring stemp = wstring(setupFile.begin(), setupFile.end());
        if (S_OK == URLDownloadToFile(NULL, dwnld_URL.c_str(), stemp.c_str(), 0, NULL))
        {
            cout << "Downloaded setup\n";
            system(setupFile.c_str());
            remove(setupFile.c_str());
        }
        else
        {
            cout << "Setup download failed\n";
            system("pause");
            return 2;
        }
    }
    else
    {
        cout << "Desktop runtime found\n";
    }

    string anaFile = "start ";
    anaFile.append(GetCurrentDirectory().append("\\bin\\Anamnesis.exe"));
    system(anaFile.c_str());
}