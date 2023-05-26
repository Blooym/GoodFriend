use super::guards::user_agent::UserAgentGuard;
use super::responses::metadata::MetadataResponse;
use super::responses::minimum_game_version::MinimumGameVersionResponse;
use super::responses::playerevent::EventStreamPlayerStateUpdateResponse;
use crate::config::get_config_cached;
use crate::types::motd::Motd;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::Route;
use rocket::State;

pub fn routes() -> Vec<Route> {
    routes![motd, metadata, minversion]
}

/// Relays a "message of the day" created from the motd.json file.
#[get("/motd")]
pub async fn motd(_agent_guard: UserAgentGuard) -> Json<Motd> {
    Json(get_config_cached().motd)
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn metadata(
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
    _agent_guard: UserAgentGuard,
) -> Json<MetadataResponse> {
    let config = get_config_cached();
    Json(MetadataResponse {
        connected_clients: queue.receiver_count(),
        status_url: config.metadata.status_url,
        donate_url: config.metadata.donation_url,
    })
}

#[get("/minimum_game_version")]
pub async fn minversion(_agent_guard: UserAgentGuard) -> Json<MinimumGameVersionResponse> {
    Json(MinimumGameVersionResponse {
        version_string: get_config_cached().minimum_game_version.to_string(),
        version: get_config_cached().minimum_game_version,
    })
}
