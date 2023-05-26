use super::{
    guards::{minimum_game_version::MinimumGameVersionGuard, user_agent::UserAgentGuard},
    responses::playerevent::{EventStreamPlayerStateUpdateResponse, PlayerStateUpdateType},
};
use crate::types::content_id::ContentIdHash;
use rocket::{
    response::status,
    serde::{Deserialize, Serialize},
    State,
};
use rocket::{tokio::sync::broadcast::Sender, Route};

pub fn routes() -> Vec<Route> {
    routes![loginstate, world]
}

/// A request to send an update of the players current logged in state.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct UpdatePlayerLoginStateRequest {
    pub content_id_hash: ContentIdHash,
    pub logged_in: bool,
    pub datacenter_id: u32,
    pub world_id: u32,
    pub territory_id: u16,
}

/// Sends a loginstate change to the server-sent event stream.
#[put("/loginstate?<update..>")]
async fn loginstate(
    update: UpdatePlayerLoginStateRequest,
    _version_guard: MinimumGameVersionGuard,
    _agent_guard: UserAgentGuard,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> status::Accepted<()> {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash.to_owned(),
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
    pub world_id: u32,
}

/// Sends a world change to the server-sent event stream.
#[put("/world?<update..>")]
async fn world(
    update: UpdatePlayerWorldRequest,
    _version_guard: MinimumGameVersionGuard,
    _agent_guard: UserAgentGuard,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> status::Accepted<()> {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash.to_owned(),
        state_update_type: PlayerStateUpdateType::WorldChange {
            world_id: update.world_id,
        },
    });

    status::Accepted(Some(()))
}
