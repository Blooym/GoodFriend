use super::{
    guards::{minimum_game_version::MinimumGameVersionGuard, user_agent::UserAgentGuard},
    responses::player_event::{EventStreamPlayerStateUpdateResponse, PlayerStateUpdateType},
};
use crate::types::content_id::{ContentIdHash, ContentIdSalt};
use rocket::{
    response::status,
    serde::{Deserialize, Serialize},
    State,
};
use rocket::{tokio::sync::broadcast::Sender, Route};

/// Returns all routes for the update api.
pub fn routes() -> Vec<Route> {
    routes![loginstate, world]
}

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
#[put("/loginstate?<update..>")]
async fn loginstate(
    _version_guard: MinimumGameVersionGuard,
    _agent_guard: UserAgentGuard,
    update: UpdatePlayerLoginStateRequest,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> status::Accepted<()> {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash,
        content_id_salt: update.content_id_salt,
        state_update_type: PlayerStateUpdateType::LoginStateChange {
            logged_in: update.logged_in,
            datacenter_id: update.datacenter_id,
            world_id: update.world_id,
            territory_id: update.territory_id,
        },
    });

    status::Accepted(Some(()))
}

/// A request to send an update of the players current world.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct UpdatePlayerWorldRequest {
    pub content_id_hash: ContentIdHash,
    pub content_id_salt: ContentIdSalt,
    pub world_id: u32,
}

/// Sends a world change to the server-sent player event stream.
#[put("/world?<update..>")]
async fn world(
    _version_guard: MinimumGameVersionGuard,
    _agent_guard: UserAgentGuard,
    update: UpdatePlayerWorldRequest,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> status::Accepted<()> {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash,
        content_id_salt: update.content_id_salt,
        state_update_type: PlayerStateUpdateType::WorldChange {
            world_id: update.world_id,
        },
    });

    status::Accepted(Some(()))
}