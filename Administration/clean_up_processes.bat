@echo off
REM https://superuser.com/questions/1129712/kill-all-processes-with-taskkill-by-username-but-exclude-some-processes
taskkill /f /t ^
/fi "IMAGENAME ne chrome.exe" ^
/fi "IMAGENAME ne explorer.exe" ^
/fi "IMAGENAME ne sublime_text.exe"
