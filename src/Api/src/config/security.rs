use crate::types::game_version::GameVersion;
use rocket::serde::{Deserialize, Serialize};
use std::collections::HashMap;

#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq, Default)]
#[serde(crate = "rocket::serde")]
pub struct SecurityConfig {
    pub blocked_user_agents: HashMap<String, UserAgentBlockMode>,
    pub minimum_game_version: GameVersion,
}

/// Defines the user agent block mode.
#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
pub enum UserAgentBlockMode {
    ExactMatch,
    PartialMatch,
}
