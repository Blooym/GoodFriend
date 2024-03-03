mod send;
mod stream;

use crate::api::routes::announcements::{send::post_announcement, stream::get_stream};
use rocket::serde::uuid::Uuid;
use rocket::serde::{Deserialize, Serialize};

pub fn routes() -> Vec<rocket::Route> {
    routes![get_stream, post_announcement]
}
pub use stream::CONNECTED_ANNOUNCEMENTS_CLIENTS;

/// The type of announcement that is being made.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
enum AnnouncementKind {
    Informational,
    Maintenance,
    Critical,
    Miscellaneous,
}

/// The cause/trigger for the announcement.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
enum AnnouncementCause {
    Manual,
    Automatic,
    Scheduled,
}

// Represents an announcement sent to other clients.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct AnnouncementMessage {
    /// The unique identifier of this announcement.
    #[serde(skip_serializing_if = "Option::is_none")]
    id: Option<Uuid>,

    /// The message content of the announcement.
    message: String,

    /// The kind of announcement that is being made.
    kind: AnnouncementKind,

    /// The reason for this announcement being made.
    cause: AnnouncementCause,

    /// An optional channel field that allows for clients to filter announcements.
    #[serde(skip_serializing_if = "Option::is_none")]
    channel: Option<String>,
}
