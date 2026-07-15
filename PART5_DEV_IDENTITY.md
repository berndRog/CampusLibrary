# Part 5 – DevIdentity ohne IdentityAccessServer

Teil 5 verwendet keinen Login, kein Access Token und keine Identitätsheader.
Client und API lesen jeweils ihre eigene `appsettings.json`.

## Client

`DevCurrentUserProvider` liest die konfigurierte Demo-Identität für Navigation
und UI-Perspektive. Der Client sendet diese Identitätsdaten nicht an die API.

## API

`DevIdentityGateway` liest dieselbe Profilstruktur aus der API-Konfiguration und
stellt sie über `IIdentityGateway` bereit. `/me`-Operationen ermitteln den
fachlichen Reader ausschließlich über das stabile `Subject`.

```text
API DevIdentity.Subject
→ IIdentityGateway.Subject
→ ReaderReadModelEf.FindMeAsync
→ Reader.Subject
→ Reader.Id
```

Die E-Mail darf später fachlich geändert werden und wird deshalb nicht zur
Reader-Zuordnung verwendet.

## Profile synchron halten

Die `DevIdentity`-Abschnitte in API und Client enthalten dieselben Profile.
`ActiveProfile` sollte in beiden Anwendungen denselben Wert besitzen:

```json
"DevIdentity": {
  "ActiveProfile": "ReaderRita"
}
```

- `ReaderRita` für Reader-UI und `/loans/me`
- `EmployeeAdmin` für die Mitarbeiterperspektive

Die API ignoriert die nur für den Client benötigten Felder `ReaderId` und
`DisplayName`. Der Client ignoriert die API-relevanten Felder `Subject`,
`CreatedAt` und `AdminRights`.

## Manuelle API-Tests

`CampusLibraryApi/_5_ApiTest/Loan_Me.http` ruft die `/me`-Endpunkte direkt auf.
Es werden weder ein Client noch der IdentityAccessServer benötigt.

Vor dem Test muss in der API gelten:

```text
DevIdentity:ActiveProfile = ReaderRita
```

`ReaderRita.Subject` ist mit dem Subject des Seed-Readers abgestimmt.
