use crate::Arguments;
use rocket::{
    Request,
    http::Status,
    request::{FromRequest, Outcome},
};
use sha2::{Digest, Sha512};

const AUTH_TOKEN_HEADER: &str = "X-Auth-Token";

/// An authenticated user that has been validated from checking a request's authentication token.
pub struct AuthenticatedUser {
    #[allow(dead_code)]
    pub token_hash: String,
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

#[rocket::async_trait]
impl<'r> FromRequest<'r> for AuthenticatedUser {
    type Error = AuthenticationError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let Some(config) = req.rocket().state::<Arguments>() else {
            return Outcome::Error((
                Status::InternalServerError,
                AuthenticationError::ConfigurationStateFailure,
            ));
        };

        let Some(authentication_token) = req
            .headers()
            .get_one(AUTH_TOKEN_HEADER)
            .map(|s| s.trim().to_owned())
        else {
            return Outcome::Error((Status::Unauthorized, AuthenticationError::MissingAuthToken));
        };

        if authentication_token.is_empty() {
            return Outcome::Error((Status::Unauthorized, AuthenticationError::MissingAuthToken));
        }

        let mut hasher = Sha512::new();
        hasher.update(authentication_token);
        let authentication_token = hex::encode(hasher.finalize());

        if !config.authentication_tokens.contains(&authentication_token) {
            return Outcome::Error((Status::Forbidden, AuthenticationError::InvalidAuthToken));
        }

        return Outcome::Success(AuthenticatedUser {
            token_hash: authentication_token,
        });
    }
}
