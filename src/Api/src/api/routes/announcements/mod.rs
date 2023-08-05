mod send;
mod stream;

use crate::api::routes::announcements::{send::post_announcement, stream::get_stream};
use rocket::serde::{Deserialize, Serialize};
use uuid::Uuid;

/// The kind of announcement that is being made.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
enum AnnouncementKind {
    Informational,
    Maintenance,
    Critical,
    Miscellaneous,
}

/// The reason for this announcement being made.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
enum AnnouncementCause {
    Manual,
    Automatic,
    Scheduled,
}

/// An announcement that is being made.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct Announcement {
    /// The unique identifier of this announcement.
    #[serde(skip_serializing_if = "Option::is_none")]
    id: Option<Uuid>,

    /// The message content of the announcement.
    message: String,

    /// The kind of announcement that is being made.
    kind: AnnouncementKind,

    /// The reason for this announcement being made.
    cause: AnnouncementCause,
}

pub fn routes() -> Vec<rocket::Route> {
    routes![get_stream, post_announcement]
}
