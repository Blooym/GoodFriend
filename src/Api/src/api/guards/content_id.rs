use rocket::{
    Request,
    http::Status,
    request::{FromRequest, Outcome},
};
use std::{
    collections::VecDeque,
    sync::{LazyLock, Mutex},
};

const CONTENT_ID_HASH_HEADER: &str = "X-Content-Id-Hash";
const CONTENT_ID_SALT_HEADER: &str = "X-Content-Id-Salt";
const CONTENT_ID_HASH_MIN_LENGTH: usize = 64;
const CONTENT_ID_SALT_MIN_LENGTH: usize = 32;

/// The amount of values to remember before removing the oldest from memory.
const SAVE_LAST_N_VALUES: usize = 500;

/// The list of seen content id hashes from events using this guard.
static SEEN_CONTENT_ID_HASHES: LazyLock<Mutex<VecDeque<String>>> =
    LazyLock::new(|| Mutex::new(VecDeque::with_capacity(SAVE_LAST_N_VALUES)));

/// A guard that checks if a Content ID hash has already been seen by all endpoints using this guard.
/// This forces a new hash to be used for each event where this guard is used.
/// Used to prevent spamming of the same event multiple times without re-processing the hash and salt.
pub struct UniqueContentId {
    pub hash: String,
    pub salt: String,
}

/// The error that can occur when checking for duplicate content id hashes.
#[derive(Debug)]
pub enum UpdateSpamGuardError {
    HashMissing,
    SaltMissing,
    HashOrSaltInvalid,
    ContentIdDuplicate,
}

#[rocket::async_trait]
impl<'r> FromRequest<'r> for UniqueContentId {
    type Error = UpdateSpamGuardError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let headers = req.headers();

        let content_id_hash = match headers.get_one(CONTENT_ID_HASH_HEADER) {
            Some(s) => s,
            None => return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::HashMissing)),
        };
        let content_id_salt = match headers.get_one(CONTENT_ID_SALT_HEADER) {
            Some(s) => s,
            None => return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::SaltMissing)),
        };
        if content_id_hash.len() < CONTENT_ID_HASH_MIN_LENGTH
            || content_id_salt.len() < CONTENT_ID_SALT_MIN_LENGTH
        {
            return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::HashOrSaltInvalid));
        }

        {
            let mut seen_hashes = SEEN_CONTENT_ID_HASHES.lock().unwrap();
            if seen_hashes.iter().any(|s| s == content_id_hash) {
                return Outcome::Error((
                    Status::BadRequest,
                    UpdateSpamGuardError::ContentIdDuplicate,
                ));
            }
            seen_hashes.push_back(content_id_hash.to_string());
            if seen_hashes.len() > SAVE_LAST_N_VALUES {
                seen_hashes.pop_front();
            }
        }

        Outcome::Success(Self {
            hash: content_id_hash.to_string(),
            salt: content_id_salt.to_string(),
        })
    }
}
