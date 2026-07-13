# Bootstrap-Crashkurs mit Beispielen aus dem CampusLibraryClient

> Selbstlernunterlage für Studierende  
> Bezug: Bootstrap 5.3 und Blazor SSR  
> Projektbeispiele: `CampusLibraryClient`

---

## 1. Lernziele

Nach der Bearbeitung dieses Kapitels können Sie:

- die Grundidee von Bootstrap erklären,
- Bootstrap-Klassen systematisch lesen,
- responsive Layouts mit Containern, Zeilen und Spalten erstellen,
- Abstände, Größen und Ausrichtungen mit Utility-Klassen steuern,
- Tabellen, Formulare, Buttons, Alerts, Badges und Navigationen gestalten,
- Bootstrap in Razor-Komponenten eines Blazor-Clients einsetzen,
- erkennen, wann Bootstrap-CSS genügt und wann Bootstrap-JavaScript benötigt wird,
- die offizielle Bootstrap-Dokumentation gezielt als Nachschlagewerk verwenden.

Die Beispiele orientieren sich an typischen Komponenten des `CampusLibraryClient`, unter anderem:

```text
Ui/Components/TopMenu.razor
Ui/Components/ErrorAlert.razor
Ui/Pages/Catalog/BooksList.razor
Ui/Pages/Reader/ReadersList.razor
Ui/Pages/Reader/Profile.razor
Ui/Pages/Reader/Update.razor
Ui/Pages/Loan/LoansList.razor
Ui/Pages/Loan/LoansListMe.razor
Ui/Pages/Loan/BorrowBook.razor
```

> **Hinweis:** Die Beispiele sind für die Lehre gekürzt und teilweise zusammengeführt.  
> Sie zeigen die Bootstrap-Ideen des CampusLibraryClient, sind aber nicht zwingend
> zeilenweise identisch mit dem jeweiligen aktuellen Projektstand.

---

## 2. Was ist Bootstrap?

Bootstrap ist ein Frontend-Toolkit. Es stellt vorgefertigte CSS-Klassen und einige
JavaScript-Komponenten bereit.

Bootstrap übernimmt vor allem drei Aufgaben:

1. **Layout**
   - Container
   - Zeilen und Spalten
   - responsive Breakpoints
   - Flexbox-Unterstützung

2. **Utilities**
   - Abstände
   - Ausrichtung
   - Anzeigeverhalten
   - Farben
   - Größen
   - Rahmen

3. **Komponenten**
   - Buttons
   - Tabellen
   - Formulare
   - Navigation
   - Alerts
   - Cards
   - Badges
   - Modals

Bootstrap ersetzt HTML nicht. Es ergänzt normales HTML durch CSS-Klassen.

```html
<button class="btn btn-primary">
   Speichern
</button>
```

Ohne Bootstrap ist dies ein normaler HTML-Button. Durch die Klassen `btn` und
`btn-primary` erhält er das Bootstrap-Styling.

**Offizielle Dokumentation**

- [Bootstrap: Introduction](https://getbootstrap.com/docs/5.3/getting-started/introduction/)
- [Bootstrap: Contents](https://getbootstrap.com/docs/5.3/getting-started/contents/)
- [Bootstrap: Cheatsheet](https://getbootstrap.com/docs/5.3/examples/cheatsheet/)

---

## 3. Bootstrap in einer Blazor-Komponente

In einer Razor-Komponente werden Bootstrap-Klassen wie in normalem HTML verwendet.

```razor
@page "/catalog/books"

<PageTitle>Bücher</PageTitle>

<h1 class="mb-4">Bücherkatalog</h1>

<button class="btn btn-primary">
   Buch hinzufügen
</button>
```

Razor ergänzt HTML um Blazor-Funktionen:

```razor
<button
   class="btn btn-primary"
   @onclick="LoadBooksAsync">
   Neu laden
</button>
```

Bootstrap steuert das Aussehen:

```text
class="btn btn-primary"
```

Blazor steuert das Verhalten:

```text
@onclick="LoadBooksAsync"
```

Diese Trennung ist wichtig:

```text
Bootstrap  -> Darstellung
Blazor     -> Zustand, Ereignisse und Anwendungslogik
```

---

## 4. Klassen als Parameter

Bootstrap verwendet meist keine Parameter im Sinne eines Methodenaufrufs.
Die Einstellungen sind in Klassennamen codiert.

```html
<div class="mt-3 p-4 text-center bg-light">
   Inhalt
</div>
```

Bedeutung:

| Klasse | Bedeutung |
|---|---|
| `mt-3` | Margin oben, Stufe 3 |
| `p-4` | Padding auf allen Seiten, Stufe 4 |
| `text-center` | Text zentrieren |
| `bg-light` | heller Hintergrund |

Eine Bootstrap-Klasse besteht häufig aus mehreren Teilen:

```text
Eigenschaft - Breakpoint - Wert
```

Beispiel:

```text
d-md-flex
```

| Teil | Bedeutung |
|---|---|
| `d` | display |
| `md` | ab Breakpoint `md` |
| `flex` | `display: flex` |

Ein weiteres Beispiel:

```text
col-lg-4
```

| Teil | Bedeutung |
|---|---|
| `col` | Grid-Spalte |
| `lg` | ab großem Bildschirm |
| `4` | vier von zwölf Grid-Spalten |

### Merksatz

```text
Bootstrap-Klassen sind kurze Konfigurationsanweisungen für CSS.
```

---

## 5. Mobile First und Breakpoints

Bootstrap arbeitet **mobile first**.

Zunächst gilt eine Gestaltung für kleine Geräte. Danach werden für größere
Bildschirme Ergänzungen definiert.

Die Standard-Breakpoints sind:

| Kürzel | Gültig ab |
|---|---:|
| ohne Kürzel / `xs` | 0 px |
| `sm` | 576 px |
| `md` | 768 px |
| `lg` | 992 px |
| `xl` | 1200 px |
| `xxl` | 1400 px |

Beispiel:

```html
<div class="col-12 col-md-6 col-lg-4">
   Buch
</div>
```

Bedeutung:

```text
kleine Geräte: volle Breite
ab md:         halbe Breite
ab lg:         ein Drittel der Breite
```

Die Regeln gelten jeweils ab dem genannten Breakpoint weiter:

```text
col-md-6
```

gilt für:

```text
md, lg, xl und xxl
```

**Offizielle Dokumentation**

- [Bootstrap: Breakpoints](https://getbootstrap.com/docs/5.3/layout/breakpoints/)
- [Bootstrap: Grid](https://getbootstrap.com/docs/5.3/layout/grid/)

---

## 6. Container

Container begrenzen und positionieren den Seiteninhalt.

### Responsive Container

```html
<main class="container py-4">
   ...
</main>
```

`container` hat abhängig von der Bildschirmbreite eine maximale Breite.

### Container über die volle Breite

```html
<main class="container-fluid py-4">
   ...
</main>
```

`container-fluid` verwendet immer die gesamte verfügbare Breite.

### CampusLibrary-Beispiel: Seitenlayout

```razor
<main class="container py-4">
   <h1 class="mb-4">CampusLibrary</h1>

   @Body
</main>
```

Bedeutung:

| Klasse | Aufgabe |
|---|---|
| `container` | Inhalt zentrieren und Breite begrenzen |
| `py-4` | vertikales Padding hinzufügen |
| `mb-4` | Abstand unter der Überschrift |

**Offizielle Dokumentation**

- [Bootstrap: Containers](https://getbootstrap.com/docs/5.3/layout/containers/)

---

## 7. Das Grid: Row und Column

Das Bootstrap-Grid basiert auf zwölf gedachten Spalten.

Grundstruktur:

```html
<div class="container">
   <div class="row">
      <div class="col">Spalte 1</div>
      <div class="col">Spalte 2</div>
   </div>
</div>
```

Die Hierarchie lautet:

```text
container
└── row
    ├── col
    └── col
```

### CampusLibrary-Beispiel: Suchformular

```razor
<div class="row g-3 align-items-end mb-4">
   <div class="col-12 col-md-4">
      <label class="form-label" for="searchField">
         Suchfeld
      </label>

      <select
         id="searchField"
         class="form-select"
         @bind="SearchField">
         <option value="Title">Titel</option>
         <option value="AuthorLastName">Nachname Autor</option>
         <option value="Isbn">ISBN</option>
      </select>
   </div>

   <div class="col-12 col-md-6">
      <label class="form-label" for="searchText">
         Suchtext
      </label>

      <input
         id="searchText"
         class="form-control"
         @bind="SearchText" />
   </div>

   <div class="col-12 col-md-2">
      <button
         class="btn btn-primary w-100"
         @onclick="SearchAsync">
         Suchen
      </button>
   </div>
</div>
```

Auf kleinen Geräten stehen alle Elemente untereinander:

```text
12 + 12 + 12
```

Ab `md` stehen sie in einer Zeile:

```text
4 + 6 + 2 = 12
```

Weitere verwendete Klassen:

| Klasse | Bedeutung |
|---|---|
| `g-3` | Abstand zwischen Grid-Zellen |
| `align-items-end` | Elemente am unteren Rand ausrichten |
| `w-100` | Button über volle Spaltenbreite |
| `mb-4` | Abstand unter dem Formular |

**Offizielle Dokumentation**

- [Bootstrap: Grid](https://getbootstrap.com/docs/5.3/layout/grid/)
- [Bootstrap: Columns](https://getbootstrap.com/docs/5.3/layout/columns/)
- [Bootstrap: Gutters](https://getbootstrap.com/docs/5.3/layout/gutters/)

---

## 8. Abstände: Margin, Padding und Gap

Bootstrap verwendet ein systematisches Schema.

### Eigenschaften

| Kürzel | CSS-Eigenschaft |
|---|---|
| `m` | margin |
| `p` | padding |

### Richtungen

| Kürzel | Richtung |
|---|---|
| `t` | top |
| `b` | bottom |
| `s` | start |
| `e` | end |
| `x` | links und rechts |
| `y` | oben und unten |
| ohne | alle Seiten |

`start` und `end` werden statt `left` und `right` verwendet, damit auch
rechts-nach-links geschriebene Sprachen unterstützt werden.

### Größenstufen

```text
0, 1, 2, 3, 4, 5
```

Beispiele:

| Klasse | Bedeutung |
|---|---|
| `mt-3` | Margin oben |
| `mb-4` | Margin unten |
| `ms-auto` | automatischer Abstand am Anfang |
| `px-3` | horizontales Padding |
| `py-2` | vertikales Padding |
| `p-4` | Padding auf allen Seiten |
| `gap-3` | Abstand zwischen Flex- oder Grid-Elementen |

### CampusLibrary-Beispiel: Aktionsleiste

```razor
<div class="d-flex justify-content-between align-items-center mb-4">
   <h1 class="mb-0">Bücher</h1>

   <button class="btn btn-primary">
      Neues Buch
   </button>
</div>
```

`mb-0` entfernt den Standardabstand unter der Überschrift, da der Abstand von
der umgebenden Aktionsleiste gesteuert wird.

### Responsive Abstände

```html
<div class="p-2 p-md-4">
   ...
</div>
```

Bedeutung:

```text
kleine Geräte: Padding-Stufe 2
ab md:         Padding-Stufe 4
```

**Offizielle Dokumentation**

- [Bootstrap: Spacing](https://getbootstrap.com/docs/5.3/utilities/spacing/)

---

## 9. Display und Flexbox

Mit Display-Utilities wird festgelegt, wie ein Element am Layout teilnimmt.

| Klasse | Ergebnis |
|---|---|
| `d-none` | nicht anzeigen |
| `d-block` | Blockelement |
| `d-inline` | Inlineelement |
| `d-inline-block` | Inlineblock |
| `d-flex` | Flexbox |
| `d-grid` | CSS Grid |

### Responsive Sichtbarkeit

```html
<span class="d-none d-md-inline">
   Angemeldeter Benutzer
</span>
```

Bedeutung:

```text
unter md: verborgen
ab md:    inline sichtbar
```

### Häufige Flexbox-Klassen

| Klasse | Bedeutung |
|---|---|
| `d-flex` | Flexbox aktivieren |
| `flex-column` | Elemente untereinander |
| `flex-md-row` | ab `md` nebeneinander |
| `justify-content-start` | am Anfang ausrichten |
| `justify-content-center` | zentrieren |
| `justify-content-between` | freien Raum dazwischen verteilen |
| `align-items-center` | quer zur Hauptachse zentrieren |
| `flex-wrap` | Umbruch erlauben |
| `gap-2` | Abstand zwischen Elementen |
| `ms-auto` | Element nach außen schieben |

### CampusLibrary-Beispiel: Kopfzeile

```razor
<div class="d-flex flex-column flex-md-row
            justify-content-between align-items-md-center
            gap-3 mb-4">
   <div>
      <h1 class="mb-1">Meine Ausleihen</h1>
      <p class="text-body-secondary mb-0">
         Aktuell ausgeliehene Medien
      </p>
   </div>

   <a class="btn btn-primary" href="/loans/borrow">
      Buch ausleihen
   </a>
</div>
```

Auf kleinen Geräten stehen Überschrift und Button untereinander. Ab `md`
stehen sie nebeneinander.

**Offizielle Dokumentation**

- [Bootstrap: Display](https://getbootstrap.com/docs/5.3/utilities/display/)
- [Bootstrap: Flex](https://getbootstrap.com/docs/5.3/utilities/flex/)

---

## 10. Breiten und Größen

Häufig verwendete Größenklassen:

| Klasse | Bedeutung |
|---|---|
| `w-25` | 25 % Breite |
| `w-50` | 50 % Breite |
| `w-75` | 75 % Breite |
| `w-100` | 100 % Breite |
| `mw-100` | maximale Breite 100 % |
| `h-100` | 100 % Höhe |

CampusLibrary-Beispiel:

```razor
<button class="btn btn-primary w-100">
   Profil speichern
</button>
```

Auf großen Bildschirmen soll ein Button manchmal nur so breit wie sein Inhalt
sein:

```razor
<button class="btn btn-primary w-100 w-md-auto">
   Profil speichern
</button>
```

Bootstrap 5.3 stellt nicht für jede Größenutility automatisch jede responsive
Variante bereit. Deshalb sollte vor der Verwendung einer Klasse wie
`w-md-auto` geprüft werden, ob sie in der eingesetzten Bootstrap-Konfiguration
existiert. Eine robuste Alternative ist das Grid:

```razor
<div class="col-12 col-md-auto">
   <button class="btn btn-primary w-100">
      Profil speichern
   </button>
</div>
```

**Offizielle Dokumentation**

- [Bootstrap: Sizing](https://getbootstrap.com/docs/5.3/utilities/sizing/)

---

## 11. Text, Farben und Hintergründe

### Textausrichtung

```html
<p class="text-start">Links beziehungsweise am Anfang</p>
<p class="text-center">Zentriert</p>
<p class="text-end">Rechts beziehungsweise am Ende</p>
```

### Textfarben

```html
<p class="text-primary">Primärfarbe</p>
<p class="text-success">Erfolg</p>
<p class="text-danger">Fehler</p>
<p class="text-warning">Warnung</p>
<p class="text-body-secondary">Zurückhaltender Text</p>
```

### Hintergründe

```html
<div class="bg-light">Heller Hintergrund</div>
<div class="bg-dark text-white">Dunkler Hintergrund</div>
```

### Semantische Farben

Bootstrap-Farben sollten eine Bedeutung ausdrücken.

| Variante | Typische Bedeutung im CampusLibraryClient |
|---|---|
| `primary` | Hauptaktion |
| `secondary` | Nebenaktion |
| `success` | erfolgreich abgeschlossen |
| `danger` | Fehler oder problematische Aktion |
| `warning` | Aufmerksamkeit erforderlich |
| `info` | neutrale Zusatzinformation |
| `light` | zurückhaltender Hintergrund |
| `dark` | dunkle Navigation |

**Offizielle Dokumentation**

- [Bootstrap: Colors](https://getbootstrap.com/docs/5.3/utilities/colors/)
- [Bootstrap: Background](https://getbootstrap.com/docs/5.3/utilities/background/)
- [Bootstrap: Text](https://getbootstrap.com/docs/5.3/utilities/text/)

---

## 12. Buttons

Buttons verwenden eine Basisklasse und eine Variante.

```html
<button class="btn btn-primary">Speichern</button>
<button class="btn btn-secondary">Abbrechen</button>
<button class="btn btn-danger">Deaktivieren</button>
```

### Outline-Buttons

```html
<button class="btn btn-outline-primary">Details</button>
<button class="btn btn-outline-danger">Deaktivieren</button>
```

### Größen

```html
<button class="btn btn-primary btn-sm">Klein</button>
<button class="btn btn-primary">Standard</button>
<button class="btn btn-primary btn-lg">Groß</button>
```

### CampusLibrary-Beispiel: Formularaktionen

```razor
<div class="d-flex gap-2">
   <button
      type="submit"
      class="btn btn-primary">
      Speichern
   </button>

   <a
      href="/catalog/books"
      class="btn btn-outline-secondary">
      Abbrechen
   </a>
</div>
```

### Links als Buttons

Ein Link bleibt fachlich eine Navigation, kann aber wie ein Button aussehen:

```razor
<a class="btn btn-primary" href="/readers/update">
   Profil ändern
</a>
```

Ein `<button>` wird für Aktionen verwendet:

```razor
<button class="btn btn-primary" @onclick="SaveAsync">
   Speichern
</button>
```

Merksatz:

```text
Navigation -> <a>
Aktion     -> <button>
```

**Offizielle Dokumentation**

- [Bootstrap: Buttons](https://getbootstrap.com/docs/5.3/components/buttons/)
- [Bootstrap: Button group](https://getbootstrap.com/docs/5.3/components/button-group/)

---

## 13. Tabellen

Bootstrap gestaltet normale HTML-Tabellen mit der Klasse `table`.

```html
<table class="table">
   ...
</table>
```

Häufige Erweiterungen:

| Klasse | Wirkung |
|---|---|
| `table-striped` | abwechselnde Zeilenfarben |
| `table-hover` | Hervorhebung beim Überfahren |
| `table-bordered` | Rahmen um Zellen |
| `align-middle` | Inhalt vertikal mittig |
| `table-sm` | kompaktere Tabelle |
| `table-responsive` | horizontal scrollbar auf kleinen Geräten |

### CampusLibrary-Beispiel: Bücherliste

```razor
<div class="table-responsive">
   <table class="table table-striped table-hover align-middle">
      <thead>
         <tr>
            <th scope="col">Titel</th>
            <th scope="col">Autoren</th>
            <th scope="col">ISBN</th>
            <th scope="col" class="text-end">Exemplare</th>
            <th scope="col" class="text-end">Aktionen</th>
         </tr>
      </thead>

      <tbody>
         @foreach(var book in Books) {
            <tr>
               <td>@book.Title</td>
               <td>@book.AuthorsText</td>
               <td>@book.Isbn</td>
               <td class="text-end">
                  @book.AvailableItemCount / @book.ItemCount
               </td>
               <td class="text-end">
                  <a
                     class="btn btn-sm btn-outline-primary"
                     href="@($"/catalog/books/{book.Id}")">
                     Details
                  </a>
               </td>
            </tr>
         }
      </tbody>
   </table>
</div>
```

### Warum `table-responsive` außen steht

Die Klasse wird auf ein umgebendes Element gesetzt:

```html
<div class="table-responsive">
   <table class="table">...</table>
</div>
```

Bei schmalen Bildschirmen kann die Tabelle horizontal gescrollt werden, ohne
die gesamte Seite zu verbreitern.

### Semantik der Tabellenköpfe

```html
<th scope="col">Titel</th>
```

`scope="col"` verbessert die Zugänglichkeit, da die Zelle als Spaltenüberschrift
gekennzeichnet wird.

**Offizielle Dokumentation**

- [Bootstrap: Tables](https://getbootstrap.com/docs/5.3/content/tables/)

---

## 14. Formulare

Bootstrap gestaltet Formularelemente über spezielle Klassen.

| Element | Bootstrap-Klasse |
|---|---|
| Label | `form-label` |
| Textfeld | `form-control` |
| Select | `form-select` |
| Checkbox | `form-check-input` |
| Checkbox-Container | `form-check` |
| Hilfetext | `form-text` |
| Fehlertext | `invalid-feedback` |

### Einfaches HTML-Beispiel

```html
<div class="mb-3">
   <label class="form-label" for="lastname">
      Nachname
   </label>

   <input
      id="lastname"
      class="form-control"
      type="text">
</div>
```

### CampusLibrary-Beispiel: Blazor-EditForm

```razor
<EditForm
   Model="Model"
   OnValidSubmit="SaveAsync">

   <DataAnnotationsValidator />

   <div class="mb-3">
      <label class="form-label" for="firstname">
         Vorname
      </label>

      <InputText
         id="firstname"
         class="form-control"
         @bind-Value="Model.Firstname" />

      <ValidationMessage For="@(() => Model.Firstname)" />
   </div>

   <div class="mb-3">
      <label class="form-label" for="lastname">
         Nachname
      </label>

      <InputText
         id="lastname"
         class="form-control"
         @bind-Value="Model.Lastname" />

      <ValidationMessage For="@(() => Model.Lastname)" />
   </div>

   <button type="submit" class="btn btn-primary">
      Profil speichern
   </button>
</EditForm>
```

Bootstrap kennt `InputText` nicht. Die Komponente erzeugt jedoch ein
HTML-`input` und übernimmt die Klasse `form-control`.

```text
Blazor-Komponente -> erzeugt HTML
Bootstrap-Klasse  -> gestaltet dieses HTML
```

### CampusLibrary-Beispiel: Adresse als Grid

```razor
<div class="row g-3">
   <div class="col-12">
      <label class="form-label" for="street">
         Straße
      </label>

      <InputText
         id="street"
         class="form-control"
         @bind-Value="Model.Street" />
   </div>

   <div class="col-12 col-md-4">
      <label class="form-label" for="postalCode">
         Postleitzahl
      </label>

      <InputText
         id="postalCode"
         class="form-control"
         @bind-Value="Model.PostalCode" />
   </div>

   <div class="col-12 col-md-8">
      <label class="form-label" for="city">
         Ort
      </label>

      <InputText
         id="city"
         class="form-control"
         @bind-Value="Model.City" />
   </div>

   <div class="col-12">
      <label class="form-label" for="country">
         Land
      </label>

      <InputText
         id="country"
         class="form-control"
         @bind-Value="Model.Country" />
   </div>
</div>
```

**Offizielle Dokumentation**

- [Bootstrap: Forms overview](https://getbootstrap.com/docs/5.3/forms/overview/)
- [Bootstrap: Form controls](https://getbootstrap.com/docs/5.3/forms/form-control/)
- [Bootstrap: Select](https://getbootstrap.com/docs/5.3/forms/select/)
- [Bootstrap: Form layout](https://getbootstrap.com/docs/5.3/forms/layout/)
- [Bootstrap: Validation](https://getbootstrap.com/docs/5.3/forms/validation/)

---

## 15. Bootstrap-Validierung und Blazor-Validierung

Bootstrap und Blazor haben unterschiedliche Aufgaben.

### Blazor

Blazor prüft das Modell und erzeugt Validierungsmeldungen.

```razor
<DataAnnotationsValidator />
<ValidationMessage For="@(() => Model.Lastname)" />
```

### Bootstrap

Bootstrap gestaltet gültige und ungültige Felder.

```html
<input class="form-control is-invalid">
<div class="invalid-feedback">
   Der Nachname ist erforderlich.
</div>
```

Blazor fügt abhängig vom Validierungszustand häufig CSS-Klassen wie `valid`
oder `invalid` hinzu. In einem Projekt kann eigenes CSS diese Klassen auf
Bootstrap-Klassen beziehungsweise Bootstrap-Farben abbilden.

Beispiel für eine mögliche Projektanpassung:

```css
.invalid {
   border-color: var(--bs-danger);
}

.validation-message {
   color: var(--bs-danger);
   font-size: 0.875rem;
   margin-top: 0.25rem;
}
```

Wichtig ist die Trennung:

```text
DataAnnotations und Blazor -> fachliche/technische Prüfung
Bootstrap und eigenes CSS  -> visuelle Rückmeldung
```

---

## 16. Alerts und Fehlerdarstellung

Alerts stellen wichtige Rückmeldungen sichtbar dar.

```html
<div class="alert alert-danger" role="alert">
   Das Buch konnte nicht geladen werden.
</div>
```

Varianten:

| Klasse | Bedeutung |
|---|---|
| `alert-primary` | allgemeine Hauptinformation |
| `alert-info` | Information |
| `alert-success` | Erfolg |
| `alert-warning` | Warnung |
| `alert-danger` | Fehler |

### CampusLibrary-Beispiel: `ErrorAlert`

```razor
@if(Error is not null) {
   <div class="alert alert-danger" role="alert">
      <h2 class="h5 alert-heading">
         Anfrage fehlgeschlagen
      </h2>

      <p class="mb-1">
         @Error.Title
      </p>

      @if(!string.IsNullOrWhiteSpace(Error.Detail)) {
         <p class="mb-0">
            @Error.Detail
         </p>
      }
   </div>
}
```

Interessant ist:

```html
<h2 class="h5">
```

Das Element bleibt semantisch eine Überschrift zweiter Ebene, wird aber wie
eine kleinere Überschrift dargestellt.

**Offizielle Dokumentation**

- [Bootstrap: Alerts](https://getbootstrap.com/docs/5.3/components/alerts/)

---

## 17. Badges für Zustände

Badges eignen sich für kurze Statusanzeigen.

```html
<span class="badge text-bg-success">Verfügbar</span>
<span class="badge text-bg-warning">Ausgeliehen</span>
<span class="badge text-bg-danger">Beschädigt</span>
```

### CampusLibrary-Beispiel: Zustand einer aktuellen Ausleihe

```razor
@if(loan.IsOverdue) {
   <span class="badge text-bg-danger">
      Überfällig
   </span>
}
else {
   <span class="badge text-bg-primary">
      Ausgeliehen
   </span>
}
```

Im vereinfachten Loan-Modell bedeutet bereits die Existenz eines `Loan`, dass
das Exemplar aktuell ausgeliehen ist. Bei der Rückgabe am Desk wird der Loan
gelöscht. Ein zusätzlicher `LoanStatus` ist deshalb nicht erforderlich.

Farben sollten nicht die einzige Informationsquelle sein. Der Zustand wird
deshalb zusätzlich als Text ausgegeben.

**Offizielle Dokumentation**

- [Bootstrap: Badges](https://getbootstrap.com/docs/5.3/components/badge/)
- [Bootstrap: Color and background](https://getbootstrap.com/docs/5.3/helpers/color-background/)

---

## 18. Cards

Cards gruppieren zusammengehörende Informationen.

```html
<div class="card">
   <div class="card-body">
      <h2 class="card-title">Clean Code</h2>
      <p class="card-text">Robert C. Martin</p>
   </div>
</div>
```

### CampusLibrary-Beispiel: Buchübersicht als Cards

```razor
<div class="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
   @foreach(var book in Books) {
      <div class="col">
         <article class="card h-100">
            <div class="card-body">
               <h2 class="h5 card-title">
                  @book.Title
               </h2>

               <p class="card-text text-body-secondary">
                  @book.AuthorsText
               </p>

               <p class="card-text">
                  Verfügbar:
                  <strong>@book.AvailableItemCount</strong>
               </p>
            </div>

            <div class="card-footer bg-transparent">
               <a
                  class="btn btn-outline-primary"
                  href="@($"/catalog/books/{book.Id}")">
                  Details
               </a>
            </div>
         </article>
      </div>
   }
</div>
```

`h-100` sorgt dafür, dass Cards innerhalb einer Zeile gleich hoch erscheinen.

**Offizielle Dokumentation**

- [Bootstrap: Cards](https://getbootstrap.com/docs/5.3/components/card/)

---

## 19. Navigation

Bootstrap unterscheidet zwischen allgemeiner Navigation und einer vollständigen
Navbar.

### Einfache Navigation

```razor
<nav class="nav">
   <NavLink class="nav-link" href="/">
      Start
   </NavLink>

   <NavLink class="nav-link" href="/catalog/books">
      Bücher
   </NavLink>

   <NavLink class="nav-link" href="/loans">
      Ausleihen
   </NavLink>
</nav>
```

`NavLink` ist eine Blazor-Komponente. Sie kann für den aktiven Link automatisch
eine CSS-Klasse setzen.

```razor
<NavLink
   class="nav-link"
   ActiveClass="active"
   href="/catalog/books">
   Bücher
</NavLink>
```

### CampusLibrary-Beispiel: obere Menüzeile

```razor
<nav class="navbar navbar-expand-lg bg-body-tertiary border-bottom">
   <div class="container">
      <a class="navbar-brand" href="/">
         CampusLibrary
      </a>

      <div class="navbar-nav me-auto">
         <NavLink class="nav-link" href="/catalog/books">
            Katalog
         </NavLink>

         <NavLink class="nav-link" href="/loans/me">
            Meine Ausleihen
         </NavLink>
      </div>

      <TopMenu />
   </div>
</nav>
```

### CampusLibrary-Beispiel: angemeldeter Benutzer

```razor
<AuthorizeView>
   <Authorized>
      <div class="d-flex align-items-center gap-2">
         <span class="text-body-secondary">
            @context.User.Identity?.Name
         </span>

         <a class="btn btn-outline-secondary btn-sm"
            href="/readers/update">
            Ändern
         </a>

         <a class="btn btn-outline-danger btn-sm"
            href="/identity/logout">
            Abmelden
         </a>
      </div>
   </Authorized>

   <NotAuthorized>
      <a class="btn btn-primary btn-sm"
         href="/identity/login">
         Anmelden
      </a>
   </NotAuthorized>
</AuthorizeView>
```

Bootstrap entscheidet hier nicht, ob der Benutzer angemeldet ist.

```text
AuthorizeView -> entscheidet, welcher Inhalt erscheint
Bootstrap     -> gestaltet den angezeigten Inhalt
```

**Offizielle Dokumentation**

- [Bootstrap: Navbar](https://getbootstrap.com/docs/5.3/components/navbar/)
- [Bootstrap: Navs and tabs](https://getbootstrap.com/docs/5.3/components/navs-tabs/)

---

## 20. Rahmen, Rundungen und Schatten

Häufige Klassen:

| Klasse | Wirkung |
|---|---|
| `border` | Rahmen |
| `border-top` | Rahmen oben |
| `border-bottom` | Rahmen unten |
| `border-danger` | Rahmen in Fehlerfarbe |
| `rounded` | abgerundete Ecken |
| `rounded-3` | stärkere Rundung |
| `shadow-sm` | kleiner Schatten |
| `shadow` | Standardschatten |
| `shadow-lg` | großer Schatten |

### CampusLibrary-Beispiel: Profilbereich

```razor
<section class="border rounded-3 p-4 shadow-sm">
   <h1 class="h3 mb-4">Profil vervollständigen</h1>

   ...
</section>
```

Diese Utilities eignen sich für kleine visuelle Anpassungen, ohne eine eigene
CSS-Klasse anzulegen.

**Offizielle Dokumentation**

- [Bootstrap: Borders](https://getbootstrap.com/docs/5.3/utilities/borders/)
- [Bootstrap: Shadows](https://getbootstrap.com/docs/5.3/utilities/shadows/)

---

## 21. Ladezustände

Ein Spinner zeigt an, dass Daten verarbeitet oder geladen werden.

```razor
@if(IsLoading) {
   <div class="d-flex align-items-center gap-2" role="status">
      <div class="spinner-border spinner-border-sm" aria-hidden="true"></div>
      <span>Daten werden geladen ...</span>
   </div>
}
```

Ein Button kann während einer Aktion deaktiviert werden:

```razor
<button
   class="btn btn-primary"
   disabled="@IsSaving"
   type="submit">

   @if(IsSaving) {
      <span
         class="spinner-border spinner-border-sm me-2"
         aria-hidden="true">
      </span>
   }

   Speichern
</button>
```

**Offizielle Dokumentation**

- [Bootstrap: Spinners](https://getbootstrap.com/docs/5.3/components/spinners/)

---

## 22. Welche Komponenten benötigen JavaScript?

Viele Bootstrap-Funktionen bestehen nur aus CSS.

Kein Bootstrap-JavaScript ist beispielsweise notwendig für:

```text
Grid
Spacing
Flexbox
Buttons
Tabellen
Formularstyling
Cards
Badges
einfache Alerts
```

Bootstrap-JavaScript wird unter anderem benötigt für:

```text
Collapse
Dropdown
Modal
Offcanvas
Toast
Tooltip
Popover
Carousel
responsive Navbar mit aufklappbarem Menü
```

Beispiel:

```html
<button
   class="navbar-toggler"
   type="button"
   data-bs-toggle="collapse"
   data-bs-target="#mainNavigation">
   <span class="navbar-toggler-icon"></span>
</button>
```

`data-bs-toggle="collapse"` wird vom Bootstrap-JavaScript ausgewertet.

In Blazor muss außerdem geprüft werden, ob eine interaktive Bootstrap-Komponente
besser durch Blazor-Zustand gesteuert werden sollte. Zwei unabhängige
Zustandsmodelle können sich sonst gegenseitig beeinflussen:

```text
Bootstrap-JavaScript-Zustand
Blazor-Komponenten-Zustand
```

Für einfache Layout- und Styling-Aufgaben ist dieses Problem nicht relevant.

**Offizielle Dokumentation**

- [Bootstrap: JavaScript](https://getbootstrap.com/docs/5.3/getting-started/javascript/)
- [Bootstrap: Collapse](https://getbootstrap.com/docs/5.3/components/collapse/)
- [Bootstrap: Modal](https://getbootstrap.com/docs/5.3/components/modal/)

---

## 23. Bootstrap und eigenes CSS

Bootstrap soll eigenes CSS nicht vollständig ersetzen.

### Bootstrap ist geeignet für

```text
Standardabstände
responsive Layouts
Formulare
Buttons
Tabellen
typische UI-Komponenten
```

### Eigenes CSS ist geeignet für

```text
fachliche Gestaltung
projektspezifische Farben
besondere Komponenten
komplexe Zustände
Layoutdetails, die Bootstrap nicht abbildet
```

Beispiel:

```razor
<div class="loan-due-date text-danger fw-semibold">
   Fällig am @loan.DueDate
</div>
```

```css
.loan-due-date {
   letter-spacing: 0.01rem;
}
```

Bootstrap übernimmt:

```text
text-danger
fw-semibold
```

Eigenes CSS übernimmt:

```text
loan-due-date
```

### Bootstrap nicht mit vielen Inline-Styles umgehen

Ungünstig:

```html
<div style="margin-top: 16px; display: flex; gap: 8px;">
```

Besser:

```html
<div class="mt-3 d-flex gap-2">
```

Bei einer fachlich bedeutenden Gestaltung ist dagegen eine eigene Klasse
sinnvoll:

```html
<div class="overdue-loan">
```

---

## 24. CSS-Variablen

Bootstrap 5 stellt viele CSS-Variablen bereit.

Beispiele:

```css
.custom-panel {
   color: var(--bs-body-color);
   background-color: var(--bs-body-bg);
   border: 1px solid var(--bs-border-color);
   border-radius: var(--bs-border-radius);
}
```

Dadurch passt sich eigenes CSS besser an Bootstrap und gegebenenfalls an
Farbmodi an.

**Offizielle Dokumentation**

- [Bootstrap: CSS variables](https://getbootstrap.com/docs/5.3/customize/css-variables/)
- [Bootstrap: Color modes](https://getbootstrap.com/docs/5.3/customize/color-modes/)

---

## 25. Barrierefreiheit

Bootstrap verbessert die Darstellung, ersetzt aber keine semantische und
barrierearme HTML-Struktur.

Wichtige Regeln:

### Labels mit Eingabefeldern verbinden

```html
<label for="email" class="form-label">E-Mail</label>
<input id="email" class="form-control">
```

### Tabellenüberschriften kennzeichnen

```html
<th scope="col">Titel</th>
```

### Buttons und Links korrekt verwenden

```text
Link   -> Navigation
Button -> Aktion
```

### Status nicht nur durch Farbe darstellen

Ungünstig:

```html
<span class="badge text-bg-danger"></span>
```

Besser:

```html
<span class="badge text-bg-danger">
   Überfällig
</span>
```

### Alerts mit Rolle kennzeichnen

```html
<div class="alert alert-danger" role="alert">
   ...
</div>
```

### Verborgener Text für Screenreader

```html
<span class="visually-hidden">
   Aktueller Status:
</span>
<span class="badge text-bg-success">
   Verfügbar
</span>
```

**Offizielle Dokumentation**

- [Bootstrap: Accessibility](https://getbootstrap.com/docs/5.3/getting-started/accessibility/)
- [Bootstrap: Visually hidden](https://getbootstrap.com/docs/5.3/helpers/visually-hidden/)

---

## 26. Häufige Muster im CampusLibraryClient

### Seitenüberschrift mit Aktion

```razor
<div class="d-flex justify-content-between align-items-center mb-4">
   <h1 class="mb-0">Readers</h1>

   <a class="btn btn-primary" href="/readers/create">
      Reader anlegen
   </a>
</div>
```

### Suchbereich

```razor
<div class="row g-3 mb-4">
   <div class="col-12 col-md">
      <input class="form-control" placeholder="Suchtext" />
   </div>

   <div class="col-12 col-md-auto">
      <button class="btn btn-primary w-100">
         Suchen
      </button>
   </div>
</div>
```

### Fehlermeldung

```razor
<div class="alert alert-danger" role="alert">
   Die Daten konnten nicht geladen werden.
</div>
```

### Leere Liste

```razor
<div class="alert alert-info" role="status">
   Es wurden keine Bücher gefunden.
</div>
```

### Statusanzeige

```razor
<span class="badge text-bg-success">
   Aktiv
</span>
```

### Aktionsgruppe

```razor
<div class="d-flex justify-content-end gap-2">
   <a class="btn btn-sm btn-outline-primary" href="...">
      Details
   </a>

   <button class="btn btn-sm btn-outline-danger">
      Deaktivieren
   </button>
</div>
```

### Formularbereich

```razor
<section class="border rounded p-4">
   <h2 class="h4 mb-3">Adresse</h2>

   <div class="row g-3">
      ...
   </div>
</section>
```

---

## 27. Klassennamen systematisch lesen

### Beispiel 1

```text
class="btn btn-sm btn-outline-primary"
```

```text
btn                  Basisklasse für Button
btn-sm               kleine Variante
btn-outline-primary  Umriss in Primärfarbe
```

### Beispiel 2

```text
class="d-flex justify-content-between align-items-center gap-2"
```

```text
d-flex                   Flexbox
justify-content-between  Platz zwischen den Elementen
align-items-center       vertikal zentrieren
gap-2                    Abstand zwischen Elementen
```

### Beispiel 3

```text
class="col-12 col-md-6 col-xl-4"
```

```text
col-12    volle Breite auf kleinen Geräten
col-md-6  halbe Breite ab md
col-xl-4  ein Drittel ab xl
```

### Beispiel 4

```text
class="table table-striped table-hover align-middle"
```

```text
table          Bootstrap-Tabelle
table-striped  abwechselnde Zeilenfarben
table-hover    Hover-Hervorhebung
align-middle   vertikale Zentrierung
```

### Beispiel 5

```text
class="mb-3"
```

```text
m  margin
b  bottom
3  Größenstufe
```

---

## 28. Wann wird eine eigene Komponente sinnvoll?

Wiederholt sich ein Bootstrap-Muster, sollte daraus gegebenenfalls eine
Razor-Komponente werden.

Beispiel:

```razor
<ErrorAlert Error="Error" />
```

statt auf jeder Seite:

```razor
@if(Error is not null) {
   <div class="alert alert-danger" role="alert">
      ...
   </div>
}
```

Weitere mögliche Komponenten:

```text
PageHeader
StatusBadge
LoadingIndicator
EmptyState
FormActions
ConfirmationDialog
```

Bootstrap bleibt dabei die Darstellungsbasis. Die Razor-Komponente kapselt
Wiederverwendung und Logik.

---

## 29. Häufige Fehler

### Fehler 1: `row` ohne `container`

Nicht immer technisch falsch, aber häufig fehlt dann die erwartete horizontale
Ausrichtung.

```html
<div class="container">
   <div class="row">
      ...
   </div>
</div>
```

### Fehler 2: Spalten nicht direkt in einer Row

Empfohlen:

```html
<div class="row">
   <div class="col">...</div>
</div>
```

### Fehler 3: zu viele individuelle Abstände

Ungünstig:

```html
<div class="mt-2 mb-3 ms-1 me-4">
```

Prüfen, ob ein einfacheres Layout mit `gap-*` möglich ist:

```html
<div class="d-flex gap-3">
```

### Fehler 4: Farbe ohne Bedeutung

`danger` sollte nicht nur gewählt werden, weil Rot optisch gefällt.

### Fehler 5: Link und Button verwechseln

```text
Seite wechseln -> Link
Daten ändern   -> Button
```

### Fehler 6: Tabelle nicht responsiv einbetten

```html
<div class="table-responsive">
   <table class="table">...</table>
</div>
```

### Fehler 7: eigene CSS-Klassen zu früh anlegen

Bevor eine eigene Klasse geschrieben wird, prüfen:

```text
Gibt es bereits eine passende Bootstrap-Utility?
```

### Fehler 8: nicht existente responsive Utility annehmen

Nicht jede Utility besitzt automatisch Varianten für alle Breakpoints.
Die offizielle Dokumentation zeigt, welche Klassen tatsächlich erzeugt werden.

---

## 30. Kompakte Bootstrap-Referenz

### Layout

```text
container
container-fluid
row
col
col-12
col-md-6
col-lg-4
g-3
gx-3
gy-3
```

### Abstände

```text
m-0 ... m-5
p-0 ... p-5
mt-*
mb-*
ms-*
me-*
mx-*
my-*
px-*
py-*
gap-*
```

### Flexbox

```text
d-flex
flex-column
flex-row
flex-wrap
justify-content-start
justify-content-center
justify-content-between
justify-content-end
align-items-start
align-items-center
align-items-end
```

### Anzeige

```text
d-none
d-block
d-inline
d-inline-block
d-flex
d-grid
d-md-block
d-lg-none
```

### Text

```text
text-start
text-center
text-end
text-primary
text-success
text-danger
text-warning
text-body-secondary
fw-normal
fw-semibold
fw-bold
```

### Buttons

```text
btn
btn-primary
btn-secondary
btn-success
btn-danger
btn-warning
btn-outline-primary
btn-outline-secondary
btn-sm
btn-lg
```

### Formulare

```text
form-label
form-control
form-select
form-check
form-check-input
form-text
is-valid
is-invalid
valid-feedback
invalid-feedback
```

### Tabellen

```text
table
table-striped
table-hover
table-bordered
table-sm
table-responsive
align-middle
```

### Komponenten

```text
alert
alert-danger
badge
text-bg-success
card
card-body
navbar
nav
nav-link
spinner-border
```

---

## 31. Übungen zum Selbststudium

### Übung 1: Klassen lesen

Erklären Sie jede Klasse:

```html
<div class="d-flex flex-column flex-md-row gap-3 mb-4">
```

### Übung 2: Bücherliste responsiv machen

Ergänzen Sie eine vorhandene Tabelle so, dass sie auf kleinen Geräten
horizontal gescrollt werden kann.

Zielstruktur:

```html
<div class="...">
   <table class="...">
      ...
   </table>
</div>
```

### Übung 3: Profilformular

Erstellen Sie ein Formular für:

```text
Vorname
Nachname
Straße
Postleitzahl
Ort
Land
```

Anforderungen:

```text
auf kleinen Geräten einspaltig
Postleitzahl und Ort ab md nebeneinander
Abstand zwischen allen Feldern
Speichern- und Abbrechen-Aktion
```

### Übung 4: Zustand einer aktuellen Ausleihe

Stellen Sie aktuelle Ausleihen abhängig von `IsOverdue` als Badge dar:

```text
Ausgeliehen
Überfällig
```

Bei der Rückgabe wird der Loan gelöscht und erscheint deshalb nicht mehr in
der Liste. Die Bedeutung muss auch ohne Farbwahrnehmung verständlich sein.

### Übung 5: Kopfzeile

Erstellen Sie eine Kopfzeile für die Bücherseite:

```text
links:  Überschrift und Beschreibung
rechts: Button "Neues Buch"
```

Auf kleinen Geräten sollen beide Bereiche untereinander stehen.

### Übung 6: Refactoring

Ersetzen Sie Inline-Styles durch Bootstrap-Utilities:

```html
<div style="display:flex; gap:16px; margin-bottom:24px;">
```

### Übung 7: Eigene Komponente

Kapseln Sie eine wiederkehrende Fehlermeldung in:

```razor
<ErrorAlert Error="..." />
```

---

## 32. Lösungshinweise

### Übung 1

```text
d-flex       Flexbox aktivieren
flex-column  zunächst untereinander
flex-md-row  ab md nebeneinander
gap-3        Abstand zwischen Elementen
mb-4         Abstand unter dem Container
```

### Übung 2

```html
<div class="table-responsive">
   <table class="table table-striped table-hover">
      ...
   </table>
</div>
```

### Übung 3: mögliche Grid-Aufteilung

```text
Straße:       col-12
Postleitzahl: col-12 col-md-4
Ort:          col-12 col-md-8
Land:         col-12
```

### Übung 6

```html
<div class="d-flex gap-3 mb-4">
```

---

## 33. Empfohlener Lernweg

### Phase 1: Überblick

Lesen:

1. Introduction
2. Containers
3. Grid
4. Spacing
5. Flex

Ziel:

```text
Bootstrap-Klassen lesen und einfache Layouts erstellen
```

### Phase 2: CampusLibrary-Oberflächen

Lesen und ausprobieren:

1. Buttons
2. Tables
3. Forms
4. Alerts
5. Badges
6. Navbar

Ziel:

```text
BooksList, ReadersList, LoansList und Profile gestalten
```

### Phase 3: Vertiefung

Lesen:

1. Accessibility
2. CSS variables
3. Color modes
4. JavaScript components

Ziel:

```text
Bootstrap kontrolliert erweitern statt nur Klassen zu kopieren
```

---

## 34. Offizielle Bootstrap-Dokumentation

### Einstieg

- [Bootstrap 5.3: Introduction](https://getbootstrap.com/docs/5.3/getting-started/introduction/)
- [Bootstrap 5.3: Download](https://getbootstrap.com/docs/5.3/getting-started/download/)
- [Bootstrap 5.3: Contents](https://getbootstrap.com/docs/5.3/getting-started/contents/)
- [Bootstrap 5.3: JavaScript](https://getbootstrap.com/docs/5.3/getting-started/javascript/)
- [Bootstrap 5.3: Accessibility](https://getbootstrap.com/docs/5.3/getting-started/accessibility/)

### Layout

- [Breakpoints](https://getbootstrap.com/docs/5.3/layout/breakpoints/)
- [Containers](https://getbootstrap.com/docs/5.3/layout/containers/)
- [Grid](https://getbootstrap.com/docs/5.3/layout/grid/)
- [Columns](https://getbootstrap.com/docs/5.3/layout/columns/)
- [Gutters](https://getbootstrap.com/docs/5.3/layout/gutters/)

### Inhalte und Formulare

- [Typography](https://getbootstrap.com/docs/5.3/content/typography/)
- [Tables](https://getbootstrap.com/docs/5.3/content/tables/)
- [Forms overview](https://getbootstrap.com/docs/5.3/forms/overview/)
- [Form controls](https://getbootstrap.com/docs/5.3/forms/form-control/)
- [Select](https://getbootstrap.com/docs/5.3/forms/select/)
- [Form layout](https://getbootstrap.com/docs/5.3/forms/layout/)
- [Validation](https://getbootstrap.com/docs/5.3/forms/validation/)

### Komponenten

- [Alerts](https://getbootstrap.com/docs/5.3/components/alerts/)
- [Badges](https://getbootstrap.com/docs/5.3/components/badge/)
- [Buttons](https://getbootstrap.com/docs/5.3/components/buttons/)
- [Cards](https://getbootstrap.com/docs/5.3/components/card/)
- [Collapse](https://getbootstrap.com/docs/5.3/components/collapse/)
- [Modal](https://getbootstrap.com/docs/5.3/components/modal/)
- [Navbar](https://getbootstrap.com/docs/5.3/components/navbar/)
- [Navs and tabs](https://getbootstrap.com/docs/5.3/components/navs-tabs/)
- [Spinners](https://getbootstrap.com/docs/5.3/components/spinners/)

### Utilities

- [Utilities API](https://getbootstrap.com/docs/5.3/utilities/api/)
- [Background](https://getbootstrap.com/docs/5.3/utilities/background/)
- [Borders](https://getbootstrap.com/docs/5.3/utilities/borders/)
- [Colors](https://getbootstrap.com/docs/5.3/utilities/colors/)
- [Display](https://getbootstrap.com/docs/5.3/utilities/display/)
- [Flex](https://getbootstrap.com/docs/5.3/utilities/flex/)
- [Shadows](https://getbootstrap.com/docs/5.3/utilities/shadows/)
- [Sizing](https://getbootstrap.com/docs/5.3/utilities/sizing/)
- [Spacing](https://getbootstrap.com/docs/5.3/utilities/spacing/)
- [Text](https://getbootstrap.com/docs/5.3/utilities/text/)

### Beispiele

- [Bootstrap Examples](https://getbootstrap.com/docs/5.3/examples/)
- [Bootstrap Cheatsheet](https://getbootstrap.com/docs/5.3/examples/cheatsheet/)

---

## 35. Zusammenfassung

Die wichtigsten Ideen von Bootstrap sind:

```text
1. Mobile first denken.
2. Layout mit Container, Row und Column aufbauen.
3. Kleine Anpassungen mit Utilities ausdrücken.
4. Komponenten aus Basis- und Variantenklassen zusammensetzen.
5. Bootstrap für Darstellung und Blazor für Verhalten verwenden.
6. Semantisches HTML und Barrierefreiheit beibehalten.
7. Die offizielle Dokumentation als Nachschlagewerk verwenden.
```

Für den CampusLibraryClient sind zunächst besonders wichtig:

```text
Container
Grid
Spacing
Flexbox
Buttons
Tables
Forms
Alerts
Badges
Navbar
```

Wer diese Bereiche sicher beherrscht, kann den größten Teil der
CampusLibrary-Oberfläche verstehen und selbstständig erweitern.
