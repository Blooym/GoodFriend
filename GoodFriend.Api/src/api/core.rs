use super::guards::user_agent::UserAgentGuard;
use super::responses::metadata::MetadataResponse;
use super::responses::minimum_game_version::MinimumGameVersionResponse;
use super::responses::player_event::EventStreamPlayerStateUpdateResponse;
use crate::config::get_config_cached;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::Route;
use rocket::State;

pub fn routes() -> Vec<Route> {
    routes![metadata, minversion]
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn metadata(
    _agent_guard: UserAgentGuard,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> Json<MetadataResponse> {
    let config = get_config_cached();
    Json(MetadataResponse {
        motd: config.motd.clone(),
        connected_clients: queue.receiver_count(),
        custom_urls: config.custom_urls.clone(),
    })
}

/// Gets the minimum game version required to use the API.
#[get("/minversion")]
pub async fn minversion(_agent_guard: UserAgentGuard) -> Json<MinimumGameVersionResponse> {
    Json(MinimumGameVersionResponse {
        version_string: get_config_cached().minimum_game_version.to_string(),
        version: get_config_cached().minimum_game_version,
    })
}
