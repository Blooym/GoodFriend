use crate::api::guards::client_key::ClientKey;
use crate::api::routes::announcements::AnnouncementMessage;
use crate::api::routes::player_events::PlayerEventStreamUpdate;
use crate::config::{ApiAboutConfig, Config};
use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};
use rocket::tokio::sync::broadcast::Sender;
use rocket::tokio::sync::RwLock;
use rocket::State;
use std::sync::Arc;

/// The response to a metadata request.
#[derive(Debug, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub about: ApiAboutConfig,
    pub connections: ConnectionData,
}

/// Connection counts for the all streams.
#[derive(Debug, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct ConnectionData {
    pub player_events: usize,
    pub announcements: usize,
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn get_metadata(
    _build_guard: ClientKey,
    player_events_queue: &State<Sender<PlayerEventStreamUpdate>>,
    announcements_queue: &State<Sender<AnnouncementMessage>>,
    config: &State<Arc<RwLock<Config>>>,
) -> Json<MetadataResponse> {
    Json(MetadataResponse {
        about: config.read().await.about.clone(),
        connections: ConnectionData {
            player_events: player_events_queue.receiver_count(),
            announcements: announcements_queue.receiver_count(),
        },
    })
}
