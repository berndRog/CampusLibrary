using CampusLibraryClient.Api.Errors;
using Microsoft.AspNetCore.Components;

namespace CampusLibraryClient.Ui.Pages.Common;

public abstract class BasePage : ComponentBase {

   protected bool IsLoading { get; set; }

   protected ApiError? Error { get; set; }

   protected void StartLoading() {
      IsLoading = true;
      Error = null;
   }

   protected void StopLoading() {
      IsLoading = false;
   }
}
