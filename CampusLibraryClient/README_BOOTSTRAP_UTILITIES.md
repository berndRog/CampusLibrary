# Bootstrap utility cleanup

This update removes most custom layout CSS from the CampusLibraryClient UI.

The client now uses Bootstrap classes for:

- page width and spacing (`container-fluid`, `px-*`, `py-*`, `mb-*`)
- navigation (`navbar`, `navbar-nav`, `nav-link`)
- buttons (`btn`, `btn-primary`, `btn-outline-*`)
- tables (`table`, `table-striped`, `table-hover`, `table-responsive`)
- cards (`card`, `card-body`, `shadow-sm`)
- definition lists (`row`, `col-sm-*`)
- muted text and badges (`text-muted`, `badge`, `text-bg-*`)

`wwwroot/app.css` intentionally keeps only Blazor-specific styles:

- validation field outlines
- validation-message color
- Blazor error boundary styling

API and validation error messages are still displayed as returned by the API.
They are not translated here.
