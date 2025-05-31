use axum::{
    extract::FromRequestParts,
    http::{HeaderMap, StatusCode, request::Parts},
    response::{IntoResponse, Response},
};
use std::{
    collections::VecDeque,
    sync::{Mutex, OnceLock},
};

const CONTENT_ID_HASH_HEADER: &str = "x-content-id-hash";
const CONTENT_ID_SALT_HEADER: &str = "x-content-id-salt";
const CONTENT_ID_HASH_MIN_LENGTH: usize = 64;
const CONTENT_ID_SALT_MIN_LENGTH: usize = 32;
const SAVE_LAST_N_VALUES: usize = 500;

static SEEN_CONTENT_ID_HASHES: OnceLock<Mutex<VecDeque<String>>> = OnceLock::new();

fn get_seen_hashes() -> &'static Mutex<VecDeque<String>> {
    SEEN_CONTENT_ID_HASHES.get_or_init(|| Mutex::new(VecDeque::with_capacity(SAVE_LAST_N_VALUES)))
}

pub struct UniqueContentId {
    pub hash: String,
    pub salt: String,
}

#[derive(Debug)]
pub enum ContentIdExtractError {
    HashMissing,
    SaltMissing,
    HashOrSaltInvalid,
    ContentIdDuplicate,
}

impl<S> FromRequestParts<S> for UniqueContentId
where
    S: Send + Sync,
{
    type Rejection = ContentIdExtractError;

    async fn from_request_parts(parts: &mut Parts, _state: &S) -> Result<Self, Self::Rejection> {
        let headers: &HeaderMap = &parts.headers;

        let content_id_hash = headers
            .get(CONTENT_ID_HASH_HEADER)
            .and_then(|v| v.to_str().ok())
            .ok_or(ContentIdExtractError::HashMissing)?;

        let content_id_salt = headers
            .get(CONTENT_ID_SALT_HEADER)
            .and_then(|v| v.to_str().ok())
            .ok_or(ContentIdExtractError::SaltMissing)?;

        if content_id_hash.len() < CONTENT_ID_HASH_MIN_LENGTH
            || content_id_salt.len() < CONTENT_ID_SALT_MIN_LENGTH
        {
            return Err(ContentIdExtractError::HashOrSaltInvalid);
        }

        let mut seen = get_seen_hashes().lock().unwrap();
        if seen.iter().any(|h| h == content_id_hash) {
            return Err(ContentIdExtractError::ContentIdDuplicate);
        }

        seen.push_back(content_id_hash.to_string());
        if seen.len() > SAVE_LAST_N_VALUES {
            seen.pop_front();
        }

        Ok(Self {
            hash: content_id_hash.to_string(),
            salt: content_id_salt.to_string(),
        })
    }
}

impl IntoResponse for ContentIdExtractError {
    fn into_response(self) -> Response {
        match self {
            Self::HashMissing | Self::SaltMissing | Self::HashOrSaltInvalid => {
                (StatusCode::BAD_REQUEST, "Invalid ContentId data")
            }
            Self::ContentIdDuplicate => (StatusCode::CONFLICT, "Duplicated ContentID request data"),
        }
        .into_response()
    }
}
