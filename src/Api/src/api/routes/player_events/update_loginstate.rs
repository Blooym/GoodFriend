use super::{PlayerEventStreamUpdate, PlayerStateUpdateType};
use crate::api::guards::{client_key::ClientKey, content_id::UniqueContentId};
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::{
    State,
    response::status,
    serde::{Deserialize, Serialize},
};

/// A request to send an update of the players current logged in state.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct UpdatePlayerLoginStateRequest {
    pub logged_in: bool,
    pub world_id: u32,
    pub territory_id: u16,
}

/// Sends a login state change to the server-sent player event stream.
#[post("/loginstate", data = "<update>", format = "json")]
pub async fn post_loginstate(
    _build_guard: ClientKey,
    content_id: UniqueContentId,
    update: Json<UpdatePlayerLoginStateRequest>,
    player_event_stream: &State<Sender<PlayerEventStreamUpdate>>,
) -> status::Accepted<()> {
    let _ = player_event_stream.send(PlayerEventStreamUpdate {
        content_id_hash: content_id.hash,
        content_id_salt: content_id.salt,
        state_update_type: PlayerStateUpdateType::LoginStateChange {
            logged_in: update.logged_in,
            world_id: update.world_id,
            territory_id: update.territory_id,
        },
    });
    println!(
        "   >> Sent loginstate event to {} subscribers",
        player_event_stream.receiver_count()
    );
    status::Accepted(())
}
