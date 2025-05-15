use super::AnnouncementMessage;
use crate::{AppState, extractors::AuthenticatedUser};
use axum::{Json, extract::State, http::StatusCode};
use tracing::info;
use uuid::Uuid;

pub async fn post_announcement(
    _user: AuthenticatedUser,
    State(state): State<AppState>,
    Json(mut payload): Json<AnnouncementMessage>,
) -> StatusCode {
    if payload.id.is_none() {
        payload.id = Some(Uuid::new_v4());
    }

    if payload.message.trim().is_empty() {
        return StatusCode::BAD_REQUEST;
    }

    if state.announcement_events_stream.send(payload).is_err() {
        return StatusCode::INTERNAL_SERVER_ERROR;
    };

    info!(
        "Sent announcement event to {} subscribers",
        state.announcement_events_stream.receiver_count()
    );

    StatusCode::OK
}
