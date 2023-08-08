use super::{PlayerEventStreamUpdate, PlayerStateUpdateType};
use crate::api::guards::cid_hash_duplicate_guard::CidHashDuplicateGuard;
use crate::api::types::content_id::{ContentIdHash, ContentIdSalt};
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::{
    response::status,
    serde::{Deserialize, Serialize},
    State,
};

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

/// Sends a login state change to the server-sent player event stream.
#[post("/loginstate", data = "<update>", format = "json")]
pub async fn post_loginstate(
    _spam_guard: CidHashDuplicateGuard,
    update: Json<UpdatePlayerLoginStateRequest>,
    queue: &State<Sender<PlayerEventStreamUpdate>>,
) -> status::Accepted<()> {
    let _ = queue.send(PlayerEventStreamUpdate {
        content_id_hash: update.content_id_hash.clone(),
        content_id_salt: update.content_id_salt.clone(),
        state_update_type: PlayerStateUpdateType::LoginStateChange {
            logged_in: update.logged_in,
            datacenter_id: update.datacenter_id,
            world_id: update.world_id,
            territory_id: update.territory_id,
        },
    });
    status::Accepted(Some(()))
}
