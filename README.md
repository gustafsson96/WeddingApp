# Ever After - Projektuppgift

Ever After är en webbaserad ASP.NET Core MVC-applikation som avser underlätta bröllopsplanering för framtida brudpar.
Applikationen möjliggör registrering och inloggning och en registrerad användare kan skapa och hantera en personlig bröllopssida
med tillhörande gästlista och önskelista. Man kan även skicka ut inbjudningar via epost till tillagda gäster, där de får möjlighet
att svara via ett RSVP-formulär. Gäster kan även reservera presenter i önskelistan. 
<br><br>
Projektet är byggt som en MVC-applikation och består därför av en Model-View-Controller-arkitektur. Det har även skapats med Entity Framework Core, 
ASP.NET Core Identity och SQLite. 

## Funktionalitet

Projektet består av en publik del och en administrativ del som kräver autentisering. 

### Publik del
Alla besökare kan:
* Visa en startsida med information om applikationen. 
* Söka efter ett bröllop baserat på brudparets namn. 
* Besöka en bröllopssida skapad för ett särskilt brudpar via en unik slug-baserad URL. 
* Visa önskelista kopplad till ett bröllop och reservera presenter.
* Skicka meddelande via kontaktformulär (skickas som epost).
* Gäster kan även svara på en inbjudan via en unik länk om de fått den skickad till sig av brudparet. 

### Administrativ del
Inloggade användare kan: 
* Skapa en egen bröllopssida via ett formulär. 
* Se en dashboard kopplad till sitt eget bröllop. 
* Redigera och ta bort sitt bröllop. 
* Lägga till en egen headerbild för bröllopssidan (buffrad uppladdning med IFormFile).
* Lägga till, redigera och ta bort gäster. 
* Skicka inbjudningar med RSVP-formulär till gäster via epost i gränssnittet. 
* Se svar från RSVP-formulär. 
* Lägga till, redigera och ta bort presenter i önskelistan. 
* Ladda upp bilder till presenter (buffrad uppladdning med IFormFile).

## Tekniker och ramverk
Projektet använder följande tekniker: 
* ASP.NET Core MVC: Strukturerar applikationen i ett Models-Views-Controller-mönster. 
* Entity Framework Core: Hanterar databasen. 
* ASP.NET Core Identity: För autentisering och användarhantering. 
* SQLite: Vald databas. 
* Mailkit/Mimekit: Bygger och skickar epost via SMTP. 
* DotNetEnv: Läser in variabler från en .env-fil. 
* SendGrid: Extern e-posttjänst. 

## NuGet-paket
Projektet använder följande NuGet-paket:

* DotNetEnv – används för att läsa miljövariabler från .env-fil.
* MailKit – används för att bygga och skicka e-post via SMTP.
* Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore – felhantering för databasen under utveckling.
* Microsoft.AspNetCore.Identity.EntityFrameworkCore – autentisering och användarhantering med databaskoppling.
* Microsoft.AspNetCore.Identity.UI – färdiga vyer för login/registrering.
* Microsoft.EntityFrameworkCore.Sqlite – SQLite-databas.
* Microsoft.EntityFrameworkCore.SqlServer – stöd för SQL Server (installerades för publicering, ej aktuell i nuläget).
* Microsoft.EntityFrameworkCore.Tools – används för migrationer via CLI.
* Microsoft.VisualStudio.Web.CodeGeneration.Design – används för scaffolding.


## Databasmodell

Applikationen använder tre egna huvudmodeller utöver Identity-tabellerna: 

### Wedding
Representerar ett bröllop och innehåller information om: 
* Namn på brudpar
* Datum och tid
* Plats och stad
* Extra information
* Headerbild
* Publik slug
* Koppling till ägaren via UserId

Ett bröllop kan ha relationer till flera gäster och flera presenter. 

### Guest
Representerar en gäst och innehåller: 
* Namn
* Epost-address
* RSVP-status
* Matpreferenser
* Eventuella allergier
* Meddelande
* Unik RSVP-token
* Tidpunkt då inbjudan skickades
* Tidpunkt då svar togs emot

Varje gäst är kopplad till ett specifikt bröllop via WeddingId. 

### Gift
Representerar en present i önskelistan och innehåller: 
* Namn
* Beskrivning
* Länk
* Pris
* Bild
* Reservationsstatus 

Varje present är kopplad till ett specifikt bröllop via WeddingId. 
<br><br>
Databasen skapas med hjälp av Entity Framework Core migrationer där modellerna översätts till databastabeller. Innehåll skapas av användaren via applikationen.

## Instruktioner för att sätta upp och köra projektet

1. Klona projektet via följande kommandon: 
* git clone https://github.com/gustafsson96/WeddingApp.git 
* cd WeddingApp

2. Kontrollera att .NET SDK är installerat via dotnet --version

3. Installera beroenden (NuGet-paket) via: dotnet restore

4. Skapa en .env-fil och lägg till miljövariabler för SMTP-server och verifierad avsändarmail. 
* SENDGRID_API_KEY=din_api_nyckel
* SENDGRID_SENDER=din_verifierade_epost

5. Konfigurera databaskoppling genom att kontrollera att korrekt connection string finns i projektet innan databasen skapas. 

6. Skapa databasen via 'dotnet ef database update' (skapa migrationer först om de saknas via 'dotnet ef migration add InitialCreate' och 'dotnet ef database update'). 

7. Starta projektet med dotnet run. 











