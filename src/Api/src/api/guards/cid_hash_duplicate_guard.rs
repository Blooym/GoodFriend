use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};
use std::sync::Mutex;

/// The amount of hashes to remember before removing the oldest hash from memory.
const SAVE_LAST_N_HASHES: usize = 500;

/// The list of seen content id hashes from events using this guard.
/// Will automatically drop older hashes to free memory.
static SEEN_CONTENT_ID_HASHES: Mutex<Vec<String>> = Mutex::new(Vec::new());

/// A guard that checks if a Content ID hash has already been seen by all endpoints using this guard.
/// This forces a new hash to be used for each event where this guard is used.
/// Used to prevent spamming of the same event and improves privacy by removing the ability track the same
/// hash across multiple events.
///
/// Note: This guard will not fail a request if the Content ID hash is missing.
pub struct CidHashDuplicateGuard;

#[rocket::async_trait]
impl<'r> FromRequest<'r> for CidHashDuplicateGuard {
    type Error = UpdateSpamGuardError;
    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let content_id = req
            .query_value::<String>("content_id_hash")
            .unwrap_or(Ok("".to_string()));

        let mut hashes = SEEN_CONTENT_ID_HASHES.lock().unwrap();
        if hashes.len() >= SAVE_LAST_N_HASHES {
            hashes.remove(0);
        }

        match content_id {
            Ok(content_id) => {
                {
                    if hashes.contains(&content_id) {
                        return Outcome::Failure((
                            Status::BadRequest,
                            UpdateSpamGuardError::ContentIdDuplicate,
                        ));
                    }
                    hashes.push(content_id);
                }
                Outcome::Success(CidHashDuplicateGuard)
            }
            Err(_) => {
                Outcome::Failure((Status::BadRequest, UpdateSpamGuardError::ContentIdMissing))
            }
        }
    }
}

/// The error that can occur when checking for duplicate content id hashes.
#[derive(Debug)]
pub enum UpdateSpamGuardError {
    ContentIdMissing,
    ContentIdDuplicate,
}
