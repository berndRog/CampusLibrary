namespace CampusLibraryApi._2_BuildingBlocks._1_Ports;

// Port for accessing the technical identity used by the current operation.
// Application code depends on this interface instead of IConfiguration,
// HttpContext, ClaimsPrincipal or JWT libraries.
public interface IIdentityGateway {

   // Stable, opaque technical subject.
   // Part 5: read from the API DevIdentity configuration.
   // Part 6: read from the validated OIDC "sub" claim.
   string Subject { get; }

   // Technical username; initially identical to the email address.
   // Part 5: read from the API DevIdentity configuration.
   // Part 6: read from "preferred_username".
   string Username { get; }

   // Creation time of the technical identity.
   DateTime CreatedAt { get; }

   // Kept compatible with the IA-server token contract.
   // CampusLibrary does not evaluate this bitmask.
   int AdminRights { get; }

   bool IsAuthenticated { get; }
   bool IsReader { get; }
   bool IsEmployee { get; }
}

/*
Didaktik
--------

Der Port bleibt in Teil 5 und Teil 6 gleich. Nur der technische Adapter
wechselt:

- Teil 5: DevIdentityGateway liest eine API-eigene Konfiguration.
- Teil 6: IdentityGatewayHttpContext liest Claims aus einem validierten Token.

Die Anwendungsschicht kann dadurch dieselben /me-Abfragen und dieselbe
Subject-basierte Reader-Zuordnung verwenden, ohne die Herkunft der Identität
zu kennen.
*/
