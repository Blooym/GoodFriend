use rocket::serde::{Deserialize, Serialize};

/// Metadata about the current state of the API.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub connected_clients: usize,
    pub status_url: String,
    pub donate_url: String,
}
