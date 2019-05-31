@echo off
:https://superuser.com/questions/1129712/kill-all-processes-with-taskkill-by-username-but-exclude-some-processes
if not "%1"=="am_admin" (powershell start -verb runas '%0' am_admin & exit /b)
:------------------------
taskkill /f ^
/fi "USERNAME eq %username%" ^
/fi "IMAGENAME ne explorer.exe" ^
/fi "USERNAME ne NT AUTHORITY\SYSTEM" ^
/fi "IMAGENAME ne svchost.exe" ^
/fi "IMAGENAME ne sihost.exe" ^
/fi "IMAGENAME ne nvcontainer.exe" ^
/fi "IMAGENAME ne RestartOnCrash.exe" ^
/fi "IMAGENAME ne cmd.exe" ^
/fi "IMAGENAME ne conhost.exe"

timeout 1
START C:\Users\AlexRickett\Desktop\Switcher_5_30_v2\Crocko-Dial.exe

