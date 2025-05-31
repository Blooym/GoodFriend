use crate::extractors::AuthenticatedUser;
use axum::http::StatusCode;

pub async fn validate_auth_handler(_auth_user: AuthenticatedUser) -> StatusCode {
    StatusCode::OK
}
