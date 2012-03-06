REQUIREMENTS 
=============
IIS 5.0-6.0 (IIS 7.0 may choke it's not tested)
.NET Framework 3.5


INSTALLATION
============
Copy the files into the folder for the website in which you wish to run the API.  Change the setting in the
web.config for your sites URL.


IIS CONFIGURATION
=================
In order for the API to function properly you must add the .xml MIME type to IIS.  This is a limitation IIS
so adding the below allows IIS to process XML/JSON pages as .NET pages.

1. Open IIS manager.
2. Open properties of the website for shack-api-net
3. Click "Home Directory" tab.
4. Click "Configuration"
5. Click "Add" under Application Extensions.
6. Enter the following information:

             Executable: c:\windows\microsoft.net\framework\v2.0.50727\aspnet_isapi.dll (this may vary based on your box)
              Extension: .xml 
         Verbs Limit to: GET, HEAD, POST, DEBUG ( Get will probably be sufficent though)
          Script Engine: [ X ] - checked
Verify that file exists: [ ] - not checked 

7. Repeat step 6 replacing .xml with .json in the extension field.

IIS SECURITY CONFIGURATION
==========================
1. Open IIS manager.
2. Expand Websites
3. Right Click on the root website for your API, and select Properties.
4. Click on the "Directory Security" tab
5. In "Authentcation and access control" click the "Edit" button
6. Make sure the following items are set if shown:

              Enable anonymous access: [ X ] - checked
              Everything under "Authenicated acess": [ ] - not checked
              
7. Click "OK"             
