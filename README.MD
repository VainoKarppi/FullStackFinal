# How to run the Backend project:
1. Backend is written using C# Net8.0
1. [Download .NET 8.0 (Linux, macOS, and Windows) (microsoft.com)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
1. Check the settings in: Backend/appsettings.json for MySql connection.
1. Run in cmd: 

    ``dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org``
   
    ``dotnet dev-certs https --trust``


Navigate to FullStackfinal/Backend folder in CMD. Run command: **dotnet run**

This will now start the server at http://localhost:5000 and https://localhost:5001.

You can check the API status by opening this web page in your browser.

Also make sure that the tables were created successfully using a HeidiSQSL, or another SQL viewer.

# How to run the Frontend project:
1. navigate to Frontend folder using ``cd ./Frontend``
1. Run in cmd: 
    
    ``npm install``

    ``npm run dev``

# How to use the API
# WIP
You can start by creating a POST request at: <http://localhost:5000/register>. This does not require authentication. Make sure Content-Type is set to: **application/json**

In Body add the username and password to form-data.


The passwords are sent to server by Plain text, since we are not using HTTPS as for now. the password then gets hashed on the server and salt will be added to it, before storing the user to Database.

After sending the request status 201 will be returned with the return data:

```
{
    "id": 22,
    "username": "mytestaccount",
    "lastLogin": "2024-04-06T14:43:38.0714405+03:00",
    "sessionToken": "f180a53e-ef96-4f58-af45-a22b0ed98a08"
}
```


We can now use the Session Token to access /protected view using Bearier token.

The session token only lasts for 5 minutes, unless a new request is made to any of the protected resources.

Remove the Body Form-Data for username and password. Then use GET to access the protected resource.




You can now check that you have the access to **/protected**
“Welcome to the protected resource!” was returned with status 200.


If the timeout has been reached, status 403 will be returned.


Using POST login will, return the same JSON data as register. Then you can also use the session token to access the protected resources.
