using CampusLibraryApi._2_Shared._3_Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CampusLibraryApi._1_Web.Common;

// Factory for creating standardized ProblemDetails objects.
// Controllers still decide the HTTP response type explicitly.
// This factory only centralizes the shape of error responses.
public static class DomainProblemDetailsFactory {
   // Create ProblemDetails from a domain error and the current HTTP context.
   public static ProblemDetails FromDomainError(
      DomainError error,
      HttpContext httpContext
   ) {
      var statusCode = (int)error.Status;

      var problemDetails = new ProblemDetails {
         Title = error.Title,
         Detail = error.Message,
         Status = statusCode,
         Type = $"https://httpstatuses.com/{statusCode}",
         Instance = httpContext.Request.Path
      };

      problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

      return problemDetails;
   }
}

/*
Didaktik
--------

ProblemDetailsFactory erzeugt ein einheitliches Fehlerformat für die Web-API.

Die Entscheidung, welcher HTTP-Status zurückgegeben wird, bleibt im Controller
sichtbar. Die Factory übernimmt nur die wiederkehrende Erzeugung des
ProblemDetails-Objekts.

Dadurch entsteht ein Mittelweg:

- keine komplexe Controller-Extension
- keine doppelte ProblemDetails-Erzeugung in jedem Endpunkt
- explizite Fallunterscheidung bleibt für Studierende sichtbar

Lernziele
---------

- ProblemDetails als standardisiertes Fehlerformat verstehen
- DomainError in eine HTTP-nahe Darstellung übersetzen
- Wiederverwendung ohne komplexe Extension Methods erreichen
- Verantwortlichkeiten zwischen Controller und Hilfsklasse trennen
*/
