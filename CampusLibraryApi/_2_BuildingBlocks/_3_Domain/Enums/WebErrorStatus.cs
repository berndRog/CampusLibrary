namespace CampusLibraryApi._2_BuildingBlocks._3_Domain.Enums;

public enum WebErrorStatus : int {
   None = 0,
   BadRequest = 400,
   Unauthorized = 401,
   Forbidden = 403,
   NotFound = 404,
   Conflict = 409,
   UnsupportedMediaType = 415,
   UnprocessableEntity = 422
}
