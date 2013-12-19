## Exec Rainmeter Plugin ##
This is a plugin that will execute a command (as though through the Windows 
"Run" dialog) and return any output to standard output from that command as a Rainmeter string.
Written for the learning experience in imitation of Conky's "exec" variable.

### DO NOT USE THIS PLUGIN ###
I have no idea how to do multithreading.  Until I learn, this thing is an
barely-functional mess that might eat your computer.  You have been warned.

Planned usage example (eventually, it will work like this)
```
[Variables]
prog="cmd"
args="/c vol C:"

[measureExec]
Measure=Plugin
Plugin="Exec.dll"
ExecFile=#prog#
Arguments=#args#
WriteToFile="#CURRENTPATH#\output.txt"
UpdateDivider=-1

[meterString]
Meter=String
MeasureName=measureExec
...
Text=%1
```
- `ExecFile` is the program to run.
- `Arguments` are its arguments.
- If `WriteToFile` is set to a vaild file path, then the plugin will also write its output to that file.
- ExecFile will be run once on refresh, or on demand using a !CommandMeasure bang.
