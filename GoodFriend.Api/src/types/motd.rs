use rocket::serde::{Deserialize, Serialize};

/// The current message of the day for the API.
#[derive(Debug, Clone, Serialize, Deserialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
pub struct Motd {
    pub message: String,
    pub important: bool,
    pub ignore: bool,
}

impl Motd {
    pub fn default() -> Self {
        Self {
            message: String::default(),
            important: false,
            ignore: true,
        }
    }
}

impl Default for Motd {
    fn default() -> Self {
        Self::default()
    }
}
