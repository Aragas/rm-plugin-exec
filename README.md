## Exec Rainmeter Plugin ##
This is a plugin that will execute a command (as though through the Windows 
"Run" dialog) and return any output to standard output from that command as a Rainmeter string.
Written for the learning experience in imitation of Conky's "exec" variable.

**Usage and Settings**
Ideally, the plugin would be distributed as part of a .rmskin package, in which case it should
automatically be installed in the right place.  If not, for the time being you'll have to build
it yourself from the code here on GitHub or download a prebuilt .dll from the the forums or something.
In that case, it should go in your "Plugins" folder under the settings path for your Rainmeter 
installation, as described here: http://docs.rainmeter.net/manual/plugins#Custom

Example usage:
'''
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
'''
- 'ExecFile' is the program to run.
- 'Arguments' are its arguments.
- If 'WriteToFile' is set to a vaild file path, then the plugin will also write its output to that file.
- 'UpdateDivider=-1' (or using the measure with a Lua script to turn it on and off) is **highly recommended**.
  Otherwise the plugin will be running the command every time the the skin updates, which is obviously rather resource intsensive.
- At the moment, this thing is *darned slow* for any command that takes a long time to run.
  I'm looking into making it asynchronous, but in any case, use it wisely (if at all).