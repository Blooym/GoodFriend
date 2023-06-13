use crate::types::game_version::GameVersion;
use rocket::serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq, Default)]
#[serde(crate = "rocket::serde")]
pub struct SecurityConfig {
    pub minimum_game_version: GameVersion,
}
