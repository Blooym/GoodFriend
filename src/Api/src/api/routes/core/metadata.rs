use crate::api::routes::announcements::Announcement;
use crate::api::routes::player_events::EventStreamPlayerStateUpdateResponse;
use crate::api::types::config::{get_config_cached, ApiAboutConfig};
use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};
use rocket::tokio::sync::broadcast::Sender;
use rocket::State;

/// The response to a metadata request.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct Metadata {
    pub about: ApiAboutConfig,
    pub connections: ConnectionData,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct ConnectionData {
    pub player_events: usize,
    pub announcements: usize,
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn get_metadata(
    player_events_queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
    announcements_queue: &State<Sender<Announcement>>,
) -> Json<Metadata> {
    let config = get_config_cached();
    Json(Metadata {
        connections: ConnectionData {
            player_events: player_events_queue.receiver_count(),
            announcements: announcements_queue.receiver_count(),
        },
        about: config.about,
    })
}
