use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::State;

use crate::constants::{
    ENV_APP_META_DONATION_PAGE, ENV_APP_META_DONATION_PAGE_DEFAULT, ENV_APP_META_STATUS_PAGE,
    ENV_APP_META_STATUS_PAGE_DEFAULT,
};
use crate::types::metadata::MetadataResponse;
use crate::types::player::PlayerStateUpdate;

/// Gets metadata about the current status of the API.
#[get("/metadata")]
pub async fn handle_metadata(queue: &State<Sender<PlayerStateUpdate>>) -> Json<MetadataResponse> {
    Json(MetadataResponse {
        connected_clients: queue.receiver_count(),
        status_url: std::env::var(ENV_APP_META_STATUS_PAGE)
            .unwrap_or(String::from(ENV_APP_META_STATUS_PAGE_DEFAULT)),
        donate_url: std::env::var(ENV_APP_META_DONATION_PAGE)
            .unwrap_or(String::from(ENV_APP_META_DONATION_PAGE_DEFAULT)),
    })
}
