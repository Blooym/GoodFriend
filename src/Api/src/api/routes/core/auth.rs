use rocket::http::Status;

use crate::api::guards::authenticated_user::AuthenticatedUserGuard;

#[get("/validateauth")]
pub async fn get_validate_auth(_user: AuthenticatedUserGuard) -> Status {
    Status::Ok
}
