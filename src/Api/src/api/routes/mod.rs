mod announcements;
mod core;
mod player_events;
mod static_files;

pub use self::announcements::routes as announcements_routes;
pub use self::announcements::AnnouncementStreamUpdate;
pub use self::core::routes as core_routes;
pub use self::player_events::routes as player_events_routes;
pub use self::player_events::PlayerEventStreamUpdate;
pub use self::static_files::routes as static_files_routes;
