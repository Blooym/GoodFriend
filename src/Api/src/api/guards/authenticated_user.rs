use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    tokio::sync::RwLock,
    Request,
};
use std::sync::Arc;

use crate::config::Config;

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
        let config = req.rocket().state::<Arc<RwLock<Config>>>().unwrap();

        let token = req.headers().get_one("X-Auth-Token");
        if let Some(token) = token {
            let token = token.to_string();

            if token.trim().is_empty() {
                return Outcome::Error((
                    Status::Unauthorized,
                    AuthenticatedUserError::MissingToken,
                ));
            }

            if config.read().await.authentication.tokens.contains(&token) {
                Outcome::Success(AuthenticatedUserGuard {
                    token_used: token.to_string(),
                })
            } else {
                Outcome::Error((Status::Unauthorized, AuthenticatedUserError::InvalidToken))
            }
        } else {
            Outcome::Error((Status::Unauthorized, AuthenticatedUserError::MissingToken))
        }
    }
}
