use rocket::serde::{Deserialize, Serialize};

use crate::types::content_id::{ContentIdHash, ContentIdSalt};

/// A request to send an update of the players current logged in state.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct UpdatePlayerLoginStateRequest {
    pub content_id_hash: ContentIdHash,
    pub content_id_salt: ContentIdSalt,
    pub logged_in: bool,
    pub datacenter_id: u32,
    pub world_id: u32,
    pub territory_id: u16,
}
