# Bootstrap layout update

This update restores the Bootstrap-based web layout for Part 5.

Changes:

- Bootstrap CSS and JavaScript are included under `wwwroot/lib/bootstrap`.
- `App.razor` loads Bootstrap before `app.css`.
- `MainLayout.razor` no longer uses a left sidebar.
- `CampusLibrary` is displayed as the application title above the menu.
- `TopMenu.razor` renders a horizontal Bootstrap navbar.
- The menu starts with `Home`.
- Reader view shows `Home | Katalog | Ausleihen | Logout`.
- Employee view shows `Home | Katalog | Leser | Ausleihen | Logout`.
- Login and Logout are toggled. In Part 5, Logout navigates to `/logout`.
- The home page no longer contains the technical Blazor/API explanation.

API and validation error messages remain unchanged and are not translated.
