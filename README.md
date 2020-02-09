# DCSServerControl
Robs dcs server control


This app allows for control of 3 DCS servers server 1 has the most controls / information but all allow for 
Dedicated Server Tag.
No Render 
Web GUI
Save/Write Folder
Restarts based on minutes.
Auto Restart

There is also a Timeout for Non Responsive.

The basic premises is that it launches and stores a process for the DCS server based on the settings you set and then monitors it based on the restart times, it will restart the server when that time is hit, if a server is non responsive for the timeout period it will kill the process and restart. Server 1 also has a 'Sync hour' which will force a restart at that hour no matter what.
