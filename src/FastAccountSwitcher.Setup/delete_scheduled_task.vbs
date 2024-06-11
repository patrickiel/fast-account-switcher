Dim taskName
taskName = "FastAccountSwitcherStartup"

Dim shell
Set shell = CreateObject("WScript.Shell")

' Command to delete the scheduled task
Dim command
command = "schtasks /Delete /TN """ & taskName & """ /F"

' Execute the command
shell.Run command, 0, True

' Clean up
Set shell = Nothing