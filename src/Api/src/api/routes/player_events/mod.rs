mod stream;
mod update_loginstate;
mod update_world;

use rocket::serde::{Deserialize, Serialize};

/// Represents a player state update that is sent to clients.
///
/// The data included is not enough to identify a player unless the client
/// has the original content id locally.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct PlayerEventStreamUpdate {
    pub content_id_hash: String,
    pub content_id_salt: String,
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

pub fn routes() -> Vec<rocket::Route> {
    routes![
        update_loginstate::post_loginstate,
        update_world::post_world,
        stream::get_stream
    ]
}
pub use self::stream::CONNECTED_PLAYER_EVENTS_CLIENTS;
