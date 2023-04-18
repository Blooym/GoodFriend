use rocket::serde::{Deserialize, Serialize};

/// Represents metadata that a client can request to know more about the API.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub connected_clients: usize,
    pub status_url: String,
    pub donate_url: String,
}
