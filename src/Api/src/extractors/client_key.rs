use crate::AppState;
use axum::{
    extract::{FromRef, FromRequestParts},
    http::{StatusCode, request::Parts},
    response::{IntoResponse, Response},
};

const CLIENT_KEY_HEADER: &str = "X-Client-Key";

// Struct to represent the `ClientKey`
#[allow(dead_code)]
pub struct ClientKey(Option<String>);

#[derive(Debug)]
pub enum ClientKeyExtractError {
    InvalidKey,
    MissingKey,
}

impl<S> FromRequestParts<S> for ClientKey
where
    AppState: FromRef<S>,
    S: Send + Sync,
{
    type Rejection = ClientKeyExtractError;

    async fn from_request_parts(parts: &mut Parts, state: &S) -> Result<Self, Self::Rejection> {
        let state = AppState::from_ref(state);

        if state.client_keys.is_none() || state.client_keys.as_ref().unwrap().is_empty() {
            return Ok(ClientKey(
                parts
                    .headers
                    .get(CLIENT_KEY_HEADER)
                    .map(|s| s.to_str().unwrap_or_default().trim().to_owned()),
            ));
        }

        let Some(key) = parts
            .headers
            .get(CLIENT_KEY_HEADER)
            .and_then(|value| value.to_str().ok())
            .map(|s| s.trim().to_owned())
        else {
            return Err(ClientKeyExtractError::MissingKey);
        };

        if key.is_empty() {
            return Err(ClientKeyExtractError::MissingKey);
        }

        if !state.client_keys.as_ref().unwrap().contains(&key) {
            return Err(ClientKeyExtractError::InvalidKey);
        }

        Ok(ClientKey(Some(key)))
    }
}

impl IntoResponse for ClientKeyExtractError {
    fn into_response(self) -> Response {
        match self {
            ClientKeyExtractError::InvalidKey => (StatusCode::FORBIDDEN, "Invalid client key"),
            ClientKeyExtractError::MissingKey => (StatusCode::UNAUTHORIZED, "Missing client key"),
        }
        .into_response()
    }
}
