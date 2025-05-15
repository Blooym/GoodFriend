use super::PlayerEventStreamUpdate;
use crate::api::guards::client_key::ClientKey;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{Sender, error::RecvError};
use rocket::{Shutdown, State};

/// The server-sent player event stream that will relay player state updates to clients.
#[get("/stream")]
pub async fn get_stream(
    _build_guard: ClientKey,
    player_event_stream: &State<Sender<PlayerEventStreamUpdate>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = player_event_stream.subscribe();
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
            yield Event::json(&msg).event("player_event");
        }
    }
}
