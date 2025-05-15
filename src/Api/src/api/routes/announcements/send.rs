use super::AnnouncementMessage;
use crate::api::guards::authenticated_user::AuthenticatedUser;
use rocket::State;
use rocket::http::Status;
use rocket::serde::json::Json;
use rocket::serde::uuid::Uuid;
use rocket::tokio::sync::broadcast::Sender;

/// Send an announcement to the announcement stream.
#[post("/send", data = "<announcement>", format = "json")]
pub async fn post_announcement(
    _user: AuthenticatedUser,
    announcement: Json<AnnouncementMessage>,
    announcements_stream: &State<Sender<AnnouncementMessage>>,
) -> Status {
    let mut announcement = announcement.into_inner();

    // Add an ID to the announcement if one was not provided by the sender.
    if announcement.id.is_none() {
        announcement.id = Some(Uuid::new_v4());
    }

    // Ensure there is content for this message.
    if announcement.message.trim().is_empty() {
        return Status::BadRequest;
    }

    // Send the message to all clients connected to the announcements stream.
    if announcements_stream.send(announcement).is_err() {
        return Status::InternalServerError;
    };

    Status::Ok
}
