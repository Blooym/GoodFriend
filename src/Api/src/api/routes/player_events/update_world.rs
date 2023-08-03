use super::{EventStreamPlayerStateUpdateResponse, PlayerStateUpdateType};
use crate::api::guards::cid_hash_duplicate_guard::CidHashDuplicateGuard;
use crate::api::types::content_id::{ContentIdHash, ContentIdSalt};
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
    pub content_id_hash: ContentIdHash,
    pub content_id_salt: ContentIdSalt,
    pub world_id: u32,
}

/// Sends a world change to the server-sent player event stream.
#[post("/currentworld", data = "<update>", format = "json")]
pub async fn post_world(
    _spam_guard: CidHashDuplicateGuard,
    update: Json<UpdatePlayerWorldRequest>,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> status::Accepted<()> {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash.clone(),
        content_id_salt: update.content_id_salt.clone(),
        state_update_type: PlayerStateUpdateType::WorldChange {
            world_id: update.world_id,
        },
    });
    status::Accepted(Some(()))
}
