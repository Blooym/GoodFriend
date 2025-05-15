use super::AnnouncementMessage;
use crate::api::guards::client_key::ClientKey;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{Sender, error::RecvError};
use rocket::{Shutdown, State};

/// Get a stream of announcements.
#[get("/stream")]
pub async fn get_stream(
    _build: ClientKey,
    announcement_stream: &State<Sender<AnnouncementMessage>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = announcement_stream.subscribe();
    EventStream! {
        loop {
            let msg = select! {
                msg = rx.recv() => match msg {
                    Ok(msg) => msg,
                    Err(RecvError::Closed) => break,
                    Err(RecvError::Lagged(_)) => continue,
                },
                _ = &mut end => break,
            };
            yield Event::json(&msg);
        }
    }
}
