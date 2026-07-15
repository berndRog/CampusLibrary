namespace CampusLibraryApi._2_BuildingBlocks._1_Ports;

// Port for accessing the technical identity of the current request.
// Application use cases depend on this interface instead of HttpContext.
public interface IIdentityGateway {

   // Stable technical subject from the IdentityAccessServer.
   // OIDC: "sub" 
   string Subject { get; }

   // Email/username from the IdentityAccessServer.
   // OIDC: "preferred_username"
   string Username { get; }

   // Identity creation time on IA-Server
   DateTime CreatedAt { get; } // Identity creation time (stabil)

   // Admin Rights, not used in CampusLibrary
   int AdminRights { get; } // bitmask claim "admin_rights" 
   
   // True if the current request contains an authenticated user.
   bool IsAuthenticated { get; }

   // True if the current user represents a library reader.
   bool IsReader { get; }

   // True if the current user represents an employee.
   bool IsEmployee { get; }

   
   
}

/*
Didaktik
--------

IIdentityGateway trennt die Anwendungsschicht von der HTTP- und Token-Technik.

Reader-Provisioning benötigt die technische Identität des angemeldeten
Benutzers. Diese Informationen kommen aus dem Access Token des
IdentityAccessServers und nicht aus einem Formular des Clients.

Der Use Case soll trotzdem nicht direkt von HttpContext, ClaimsPrincipal oder
JWT-Bibliotheken abhängen. Deshalb liest ein Adapter in der Web-Schicht die
Claims und stellt sie über dieses kleine Interface bereit.

Wichtig ist die Trennung:

- Authentifizierung beantwortet: Wer ist der technische Benutzer?
- Provisioning beantwortet: Welcher fachliche Reader gehört zu diesem Benutzer?

Lernziele
---------

- technische Identität über einen Port in die Anwendungsschicht holen
- HttpContext aus Core- und UseCase-Code heraushalten
- subject/email als vertrauenswürdige Token-Daten behandeln
- Benutzerrollen nicht über UI-Eingaben ableiten
*/
