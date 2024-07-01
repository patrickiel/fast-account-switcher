On Error Resume Next

Set service = CreateObject("Schedule.Service")
If Err.Number <> 0 Then
    ' WScript.Echo "Error creating Schedule.Service object: " & Err.Description
    WScript.Quit
End If

service.Connect
If Err.Number <> 0 Then
    ' WScript.Echo "Error connecting to Task Scheduler: " & Err.Description
    WScript.Quit
End If

' Define the folder path
folderPath = "\FastAccountSwitcher"

' Get the folder
Set folder = service.GetFolder(folderPath)
If Err.Number <> 0 Then
    ' WScript.Echo "Error getting folder: " & Err.Description
    WScript.Quit
End If

' Delete all tasks in the folder
For Each task In folder.GetTasks(0)
    folder.DeleteTask task.Name, 0
    If Err.Number <> 0 Then
        ' WScript.Echo "Error deleting task '" & task.Name & "': " & Err.Description
        Err.Clear
    End If
Next

' Delete the folder
WScript.Echo "Attempting to delete folder: " & folderPath
Set parentFolder = service.GetFolder("\")
If Err.Number <> 0 Then
    ' WScript.Echo "Error getting root folder: " & Err.Description
    WScript.Quit
End If

parentFolder.DeleteFolder folderPath, 0
If Err.Number <> 0 Then
    ' WScript.Echo "Error deleting folder: " & Err.Description
Else
    ' WScript.Echo "Folder deleted: " & folderPath
End If

Set folder = Nothing
Set parentFolder = Nothing
Set service = Nothing