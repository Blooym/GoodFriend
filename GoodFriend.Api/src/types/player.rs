use rocket::serde::{Deserialize, Serialize};

/// Represents a player state update that is sent to clients.
///
/// The data included is not enough to identify a player unless the client
/// has access to both the original `content_id` and the salt from this update event.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct PlayerStateUpdate {
    pub content_id_hash: String,
    pub is_logged_in: bool,
    pub datacenter_id: usize,
    pub world_id: usize,
    pub territory_id: usize,
    pub content_id_salt: String,
}
