@echo off
echo.
echo Git Commit Helper
echo ================
echo.

REM Add all changes
git add .

REM Show status
echo Current changes to be committed:
echo -------------------------------
git status
echo.

REM Get commit message
set /p commit_message=Enter your commit message: 

REM Commit with the provided message
git commit -m "%commit_message%"

echo.
echo Commit completed!
echo.

pause
