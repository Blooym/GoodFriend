use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};
use std::sync::Mutex;

const CONTENT_ID_HASH_HEADER: &str = "X-Content-Id-Hash";
const CONTENT_ID_HASH_MIN_LENGTH: usize = 64;
const CONTENT_ID_SALT_HEADER: &str = "X-Content-Id-Salt";
const CONTENT_ID_SALT_MIN_LENGTH: usize = 32;

/// The amount of hashes to remember before removing the oldest hash from memory.
const SAVE_LAST_N_HASHES: usize = 350;

/// The list of seen content id hashes from events using this guard.
/// Will automatically drop older hashes to free memory.
static SEEN_CONTENT_ID_HASHES: Mutex<Vec<String>> = Mutex::new(Vec::new());

/// A guard that checks if a Content ID hash has already been seen by all endpoints using this guard.
/// This forces a new hash to be used for each event where this guard is used.
/// Used to prevent spamming of the same event multiple times without re-processing the hash and salt.
///
/// Note: This guard will fail if the `content_id_hash` query parameter is missing.
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
        // Get hash from header.
        let Some(content_id_hash) = req
            .headers()
            .get_one(CONTENT_ID_HASH_HEADER)
            .map(|s| s.to_string())
        else {
            return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::HashMissing));
        };

        // Get salt from header.
        let Some(content_id_salt) = req
            .headers()
            .get_one(CONTENT_ID_SALT_HEADER)
            .map(|s| s.to_string())
        else {
            return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::SaltMissing));
        };

        // Validate that all values are valid.
        if content_id_hash.len() < CONTENT_ID_HASH_MIN_LENGTH
            || content_id_salt.len() < CONTENT_ID_SALT_MIN_LENGTH
        {
            return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::HashOrSaltInvalid));
        }

        // Check if this hash has been used before.
        let mut seen_hashes = SEEN_CONTENT_ID_HASHES.lock().unwrap();
        if seen_hashes.contains(&content_id_hash) {
            return Outcome::Error((Status::BadRequest, UpdateSpamGuardError::ContentIdDuplicate));
        }

        // Mark this hash as seen and clear out old hashes until we're at SAVE_LAST_N_HASHES or below.
        seen_hashes.push(content_id_hash.clone());
        while seen_hashes.len() >= SAVE_LAST_N_HASHES {
            seen_hashes.remove(0);
        }
        drop(seen_hashes);

        return Outcome::Success(Self {
            hash: content_id_hash,
            salt: content_id_salt,
        });
    }
}
