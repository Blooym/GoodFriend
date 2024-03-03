use super::{PlayerEventStreamUpdate, PlayerStateUpdateType};
use crate::api::guards::{client_key::ClientKey, content_id::UniqueContentId};
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::{
    response::status,
    serde::{Deserialize, Serialize},
    State,
};

/// A request to send an update of the players current world.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct UpdatePlayerWorldRequest {
    pub world_id: u32,
}

/// Sends a world change to the server-sent player event stream.
#[post("/currentworld", data = "<update>", format = "json")]
pub fn post_world(
    _build_guard: ClientKey,
    content_id: UniqueContentId,
    update: Json<UpdatePlayerWorldRequest>,
    queue: &State<Sender<PlayerEventStreamUpdate>>,
) -> status::Accepted<()> {
    let _ = queue.send(PlayerEventStreamUpdate {
        content_id_hash: content_id.hash,
        content_id_salt: content_id.salt,
        state_update_type: PlayerStateUpdateType::WorldChange {
            world_id: update.world_id,
        },
    });
    status::Accepted(())
}
