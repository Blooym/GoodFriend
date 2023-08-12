use crate::api::types::config;
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};

/// A guard that checks if a user is authenticated.
pub struct AuthenticatedUserGuard {
    /// The token used to authenticate the user.
    pub token_used: String,
}

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
            let token = token.to_string();

            if token.trim().is_empty() {
                return Outcome::Failure((
                    Status::Unauthorized,
                    AuthenticatedUserError::MissingToken,
                ));
            }

            if config.authentication.tokens.contains(&token) {
                Outcome::Success(AuthenticatedUserGuard {
                    token_used: token.to_string(),
                })
            } else {
                Outcome::Failure((Status::Unauthorized, AuthenticatedUserError::InvalidToken))
            }
        } else {
            Outcome::Failure((Status::Unauthorized, AuthenticatedUserError::MissingToken))
        }
    }
}