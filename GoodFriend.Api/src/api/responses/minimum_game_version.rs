use rocket::serde::{Deserialize, Serialize};

use crate::types::game_version::GameVersion;

/// Metadata about the current state of the API.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MinimumGameVersionResponse {
    pub version_string: String,
    pub version: GameVersion,
}
