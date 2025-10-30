Don't create appsettings.Production unless you wish to suffer for 4 hours for nothing. Use everything in appsettings.json and nothing else!!!

mysql pass: 77228Gl;

For testing (local):

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=EventaDb;User=root;Password=77228Gl"
  },
  "EmailOptions": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Email": "titarenkonik1@gmail.com",
    "Password": "ryrr nzzv wnyq zdta"
  },
  "Jwt": {
    "Audience": "https://localhost:7293",
    "Issuer": "https://localhost:7293",
    "Key": "7c07aae2-a2b6-41b1-bec0-209485079d82",
    "ExpiresDay": 30
  }
}


For production: 

{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/etc/letsencrypt/live/eventa-app.fun/fullchain.pem",
          "KeyPath": "/etc/letsencrypt/live/eventa-app.fun/privkey.pem"
        }
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=EventaDb;User=root;Password=77228Gl"
  },
  "EmailOptions": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Email": "titarenkonik1@gmail.com",
    "Password": "ryrr nzzv wnyq zdta"
  },
  "Jwt": {
    "Audience": "https://0.0.0.0:5001",
    "Issuer": "https://0.0.0.0:5001",
    "Key": "7c07aae2-a2b6-41b1-bec0-209485079d82",
    "ExpiresDay": 30
  }
}