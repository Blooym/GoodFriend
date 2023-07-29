use super::responses::metadata::MetadataResponse;
use super::responses::player_event::EventStreamPlayerStateUpdateResponse;
use crate::config::get_config_cached;
use rocket::http::Status;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::Route;
use rocket::State;

pub fn routes() -> Vec<Route> {
    routes![get_metadata, get_health]
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn get_metadata(
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) -> Json<MetadataResponse> {
    let config = get_config_cached();
    Json(MetadataResponse {
        connected_clients: queue.receiver_count(),
        about: config.about,
    })
}

/// Health check endpoint
#[get("/health")]
pub async fn get_health() -> Status {
    Status::Ok
}
