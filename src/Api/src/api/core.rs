use super::guards::user_agent::UserAgentGuard;
use super::responses::metadata::MetadataResponse;
use super::responses::minimum_game_version::MinimumGameVersionResponse;
use super::responses::player_event::EventStreamPlayerStateUpdateResponse;
use crate::config::base::get_config_cached;
use rocket::http::Status;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::Route;
use rocket::State;

pub fn routes() -> Vec<Route> {
    routes![metadata, minversion, health]
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn metadata(
    _agent_guard: UserAgentGuard,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> Json<MetadataResponse> {
    let config = get_config_cached();
    Json(MetadataResponse {
        connected_clients: queue.receiver_count(),
        about: config.about,
    })
}

/// Gets the minimum game version required to use the API.
#[get("/minversion")]
pub async fn minversion(_agent_guard: UserAgentGuard) -> Json<MinimumGameVersionResponse> {
    Json(MinimumGameVersionResponse {
        version_string: get_config_cached()
            .security
            .minimum_game_version
            .to_string(),
        version: get_config_cached().security.minimum_game_version,
    })
}

#[get("/health")]
pub async fn health() -> Status {
    Status::Ok
}
