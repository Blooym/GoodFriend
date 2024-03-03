use crate::api::guards::authenticated_user::AuthenticatedUser;
use rocket::http::Status;

/// Validates an authentication key via the authentication guard and returns OK if it's valid.
#[post("/validate")]
pub async fn get_validate_auth(_user: AuthenticatedUser) -> Status {
    Status::Ok
}
