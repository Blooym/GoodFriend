use super::Announcement;
use crate::api::guards::authenticated_user::AuthenticatedUserGuard;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::State;
use uuid::Uuid;

/// Send an announcement to the announcement queue.
#[post("/send", data = "<message>", format = "json")]
pub async fn post_announcement(
    _authenticated_user: AuthenticatedUserGuard,
    message: Json<Announcement>,
    queue: &State<Sender<Announcement>>,
) {
    let mut message = message.into_inner();
    if message.id.is_none() {
        message.id = Some(Uuid::new_v4());
    }
    let _ = queue.send(message);
}
