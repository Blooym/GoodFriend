use crate::api::guards::authenticated_user::AuthenticatedUser;
use crate::api::routes::announcements::AnnouncementMessage;
use crate::api::routes::player_events::PlayerEventStreamUpdate;
use rocket::State;
use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};
use rocket::tokio::sync::broadcast::Sender;

/// The response to a metadata request.
#[derive(Debug, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MetadataResponse {
    pub player_events_connections: usize,
    pub announcements_connections: usize,
}

/// Gets metadata about the current status of the API.
#[get("/connections")]
pub async fn get_connections(
    _auth_user: AuthenticatedUser,
    player_events_queue: &State<Sender<PlayerEventStreamUpdate>>,
    announcements_queue: &State<Sender<AnnouncementMessage>>,
) -> Json<MetadataResponse> {
    Json(MetadataResponse {
        player_events_connections: player_events_queue.receiver_count(),
        announcements_connections: announcements_queue.receiver_count(),
    })
}
