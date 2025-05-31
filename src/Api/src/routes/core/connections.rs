use crate::{AppState, extractors::AuthenticatedUser};
use axum::{Json, extract::State};
use serde::Serialize;

#[derive(Debug, Serialize)]
pub struct MetadataResponse {
    pub player_events_connections: usize,
    pub announcements_connections: usize,
}

pub async fn connection_count_handler(
    _auth_user: AuthenticatedUser,
    State(state): State<AppState>,
) -> Json<MetadataResponse> {
    Json(MetadataResponse {
        player_events_connections: state.player_events_stream.receiver_count(),
        announcements_connections: state.announcement_events_stream.receiver_count(),
    })
}
