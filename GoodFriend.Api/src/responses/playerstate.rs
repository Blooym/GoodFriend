use rocket::serde::{Deserialize, Serialize};

use crate::types::content_id::{ContentIdHash, ContentIdSalt};

/// Represents a player state update that is sent to clients.
///
/// The data included is not enough to identify a player unless the client
/// has access to both the original `content_id` and the salt from this update event.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct EventStreamPlayerStateUpdateResponse {
    pub content_id_hash: ContentIdHash,
    pub content_id_salt: ContentIdSalt,
    pub state_update_type: PlayerStateUpdateType,
}

/// Represents state update types for different integrations and such.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub enum PlayerStateUpdateType {
    LoginStateChange {
        datacenter_id: u32,
        world_id: u32,
        territory_id: u16,
        logged_in: bool,
    },
}
