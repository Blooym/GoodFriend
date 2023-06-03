use rocket::serde::{Deserialize, Serialize};

use crate::types::api_about::ApiAbout;

/// Metadata about the current state of the API.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub start_time: u64,
    pub connected_clients: usize,
    pub about: ApiAbout,
}
