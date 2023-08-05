use super::Announcement;
use crate::api::guards::authenticated_user::AuthenticatedUserGuard;
use rocket::http::Status;
use rocket::serde::json::Json;
use rocket::tokio::sync::broadcast::Sender;
use rocket::State;
use uuid::Uuid;

/// Send an announcement to the announcement queue.
#[post("/send", data = "<announcement>", format = "json")]
pub async fn post_announcement(
    _authenticated_user: AuthenticatedUserGuard,
    announcement: Json<Announcement>,
    queue: &State<Sender<Announcement>>,
) -> Status {
    let mut announcement = announcement.into_inner();
    if announcement.id.is_none() {
        announcement.id = Some(Uuid::new_v4());
    }
    if announcement.message.trim().is_empty() {
        return Status::BadRequest;
    }
    let _ = queue.send(announcement);
    Status::Ok
}
