use rocket::serde::{Deserialize, Serialize};
use std::collections::HashMap;

use crate::types::motd::Motd;

/// Metadata about the current state of the API.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub connected_clients: usize,
    pub motd: Motd,
    pub custom_urls: HashMap<String, String>,
}
