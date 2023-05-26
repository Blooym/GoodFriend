use crate::types::content_id::ContentIdHash;
use rocket::serde::{Deserialize, Serialize};

/// Represents a player state update that is sent to clients.
///
/// The data included is not enough to identify a player unless the client
/// has the original content id locally.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct EventStreamPlayerStateUpdateResponse {
    pub content_id_hash: ContentIdHash,
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
    WorldChange {
        world_id: u32,
    },
}
