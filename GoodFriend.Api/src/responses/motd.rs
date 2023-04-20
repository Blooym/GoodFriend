/// Represents motd that a client can request.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MotdResponse {
    pub content: String,
    pub important: bool,
    pub ignore: bool,
}

use rocket::serde::{Deserialize, Serialize};
