# pgDbUP
This is based on the project [dbUp](https://dbup.readthedocs.io).

### Purpose
This project assists with the deployment of database changes for each environment.

### Contributing
Create a file in the Scripts folder. The script should be named 'YYYYDDMMhhmmZ-NewScript.sql' where YYYYDDMMhhmmZ is a (in [UTC](https://time.is/Z)) date time stamp and 'Short Description' is a short description about what the script does.
Run the Powershell file in the script directory 'CreateNewScript.ps1' to create the file for you and replace 'NewScript' with your description - ie Ticket number or something else meaningful.

Files that have already are immutable and most not be changed. Add more scripts to get the database into the desired state.

### Deployment
Update the connection string in App.config to point to the database for the environment you wish to target and run the executable.

### Restarting
To reduce the amount of scripts, occasionally the scripts should be all deleted and a new baseline schema script can be used.
