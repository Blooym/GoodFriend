use rocket::http::Status;

use crate::api::guards::authenticated_user::AuthenticatedUserGuard;

/// Validates an authentication key by returning a 200 OK if it's valid.
#[post("/validateauth")]
pub async fn get_validate_auth(_user: AuthenticatedUserGuard) -> Status {
    Status::Ok
}
