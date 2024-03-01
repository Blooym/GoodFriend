use std::sync::Arc;

use crate::api::routes::announcements::AnnouncementStreamUpdate;
use crate::api::routes::player_events::PlayerEventStreamUpdate;
use crate::config::{ApiAboutConfig, Config};
use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};
use rocket::tokio::sync::broadcast::Sender;
use rocket::tokio::sync::RwLock;
use rocket::State;

/// The response to a metadata request.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct Metadata {
    pub about: ApiAboutConfig,
    pub connections: ConnectionData,
}

/// Connection counts for the different streams provided.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct ConnectionData {
    pub player_events: usize,
    pub announcements: usize,
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn get_metadata(
    player_events_queue: &State<Sender<PlayerEventStreamUpdate>>,
    announcements_queue: &State<Sender<AnnouncementStreamUpdate>>,
    config: &State<Arc<RwLock<Config>>>,
) -> Json<Metadata> {
    Json(Metadata {
        about: config.read().await.about.clone(),
        connections: ConnectionData {
            player_events: player_events_queue.receiver_count(),
            announcements: announcements_queue.receiver_count(),
        },
    })
}
