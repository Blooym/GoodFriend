use crate::config::Config;
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    tokio::sync::RwLock,
    Request,
};
use std::sync::Arc;

/// An authenticated user that has been validated from checking a request's authentication token.
pub struct AuthenticatedUser {
    pub token: String,
}

#[derive(Debug)]
pub enum AuthenticationError {
    /// No authentication token header was sent.
    MissingAuthToken,
    /// The authentication token obtained from the header was not valid.
    InvalidAuthToken,
    /// Something went wrong when trying to read the configuration.
    ConfigurationStateFailure,
}

const AUTH_TOKEN_HEADER: &str = "X-Auth-Token";

#[rocket::async_trait]
impl<'r> FromRequest<'r> for AuthenticatedUser {
    type Error = AuthenticationError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        // Read the configuration or return an error if unable to.
        let Some(config) = req.rocket().state::<Arc<RwLock<Config>>>() else {
            return Outcome::Error((
                Status::InternalServerError,
                AuthenticationError::ConfigurationStateFailure,
            ));
        };
        let config = config.read().await;

        // Get the authentication token from the headers.
        let Some(authentication_token) = req
            .headers()
            .get_one(AUTH_TOKEN_HEADER)
            .map(|s| s.trim().to_owned())
        else {
            return Outcome::Error((Status::Unauthorized, AuthenticationError::MissingAuthToken));
        };

        // Token must not be an empty string.
        if authentication_token.is_empty() {
            return Outcome::Error((Status::Unauthorized, AuthenticationError::MissingAuthToken));
        }

        // Check if the token is in the configuration.
        if !config
            .security
            .authentication_tokens
            .contains(&authentication_token)
        {
            return Outcome::Error((Status::Unauthorized, AuthenticationError::InvalidAuthToken));
        }

        return Outcome::Success(AuthenticatedUser {
            token: authentication_token,
        });
    }
}
