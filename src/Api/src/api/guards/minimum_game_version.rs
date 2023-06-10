use crate::{config::get_config_cached, types::game_version::GameVersion};
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};

/// The header name for the game version.
const GAME_VERSION_HEADER: &str = "X-Game-Version";

/// A guard that checks the game version is up to date.
pub struct MinimumGameVersionGuard;

#[rocket::async_trait]
impl<'r> FromRequest<'r> for MinimumGameVersionGuard {
    type Error = GameVersionError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let sent_version_header = req
            .headers()
            .get_one(GAME_VERSION_HEADER)
            .unwrap_or_default();
        if sent_version_header.is_empty() {
            return Outcome::Failure((Status::BadRequest, GameVersionError::NotPresent));
        }

        let sent_version_result = GameVersion::from_str(sent_version_header);
        let sent_version: GameVersion = match sent_version_result {
            Ok(version) => version,
            Err(_) => return Outcome::Failure((Status::BadRequest, GameVersionError::Invalid)),
        };
        if get_config_cached().minimum_game_version > sent_version {
            return Outcome::Failure((Status::Forbidden, GameVersionError::Outdated));
        }

        Outcome::Success(MinimumGameVersionGuard)
    }
}

/// An error that can occur when checking the game version.
#[derive(Debug)]
pub enum GameVersionError {
    Outdated,
    Invalid,
    NotPresent,
}
