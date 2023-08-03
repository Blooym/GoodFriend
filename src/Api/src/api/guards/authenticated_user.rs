use crate::api::types::config;
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};

/// A guard that checks if the user is authenticated with a valid token.
pub struct AuthenticatedUserGuard {}

/// An error that can occur when checking if a user is authenticated.
#[derive(Debug)]
pub enum AuthenticatedUserError {
    InvalidToken,
    MissingToken,
}

#[rocket::async_trait]
impl<'r> FromRequest<'r> for AuthenticatedUserGuard {
    type Error = AuthenticatedUserError;
    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let config = config::get_config_cached();
        let token = req.headers().get_one("X-Auth-Token");
        if let Some(token) = token {
            if config.authentication.tokens.contains(&token.to_string()) {
                Outcome::Success(AuthenticatedUserGuard {})
            } else {
                Outcome::Failure((Status::Unauthorized, AuthenticatedUserError::InvalidToken))
            }
        } else {
            Outcome::Failure((Status::Unauthorized, AuthenticatedUserError::MissingToken))
        }
    }
}
