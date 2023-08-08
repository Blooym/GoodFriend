use super::AnnouncementStreamUpdate;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{error::RecvError, Sender};
use rocket::{Shutdown, State};

/// Get a stream of announcements.
#[get("/stream")]
pub async fn get_stream(
    announcement_queue: &State<Sender<AnnouncementStreamUpdate>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = announcement_queue.subscribe();
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
