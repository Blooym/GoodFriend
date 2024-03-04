use crate::config::Config;
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    tokio::sync::RwLock,
    Request,
};
use std::sync::Arc;

/// A key provided from the client that represents the software that they are using to make the request.
///
/// If the `config.allowed_client_keys` configuration option has been enabled then this guard will block
/// all requests from clients that either do not have a key contained in `allowed_client_keys` or did not send a key.
pub struct ClientKey(Option<String>);

#[derive(Debug)]
pub enum ClientKeyGuardError {
    InvalidKey,
    MissingKey,
    ConfigurationStateFailure,
}

const CLIENT_KEY_HEADER: &str = "X-Client-Key";

#[rocket::async_trait]
impl<'r> FromRequest<'r> for ClientKey {
    type Error = ClientKeyGuardError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        // Read the configuration or return an error if unable to.
        let Some(config) = req.rocket().state::<Arc<RwLock<Config>>>() else {
            return Outcome::Error((
                Status::InternalServerError,
                ClientKeyGuardError::ConfigurationStateFailure,
            ));
        };
        let config = config.read().await;

        // If there are no configured keys, just return a success even if the keys isn't valid
        // as we have nothing to validate it against.
        if config
            .security
            .allowed_client_keys
            .as_ref()
            .map_or(true, |s| s.is_empty())
        {
            return Outcome::Success(Self(
                req.headers()
                    .get_one(CLIENT_KEY_HEADER)
                    .map(|s| s.to_owned()),
            ));
        };

        // Get the key or return an error if it isn't provided or is invalid.
        let Some(key) = req
            .headers()
            .get_one(CLIENT_KEY_HEADER)
            .map(|s| s.trim().to_owned())
        else {
            return Outcome::Error((Status::Unauthorized, ClientKeyGuardError::MissingKey));
        };
        if key.is_empty() {
            return Outcome::Error((Status::Unauthorized, ClientKeyGuardError::MissingKey));
        }

        // Check if this key is in the configuration.
        if !config
            .security
            .allowed_client_keys
            .as_ref()
            .map_or(false, |s| s.contains(&key))
        {
            return Outcome::Error((Status::Forbidden, ClientKeyGuardError::InvalidKey));
        }

        return Outcome::Success(Self(Some(key)));
    }
}
