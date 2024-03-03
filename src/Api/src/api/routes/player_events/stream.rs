use super::PlayerEventStreamUpdate;
use crate::api::guards::client_key::ClientKey;
use once_cell::sync::Lazy;
use rocket::futures::channel::oneshot::channel;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{error::RecvError, Sender};
use rocket::{Shutdown, State};
use rocket_prometheus::prometheus::IntGauge;

pub static CONNECTED_PLAYER_EVENTS_CLIENTS: Lazy<IntGauge> = Lazy::new(|| {
    IntGauge::new(
        "connected_player_events_clients",
        "The number of clients currently connected to the player events SSE stream",
    )
    .expect("Could not create CONNECTED_PLAYER_EVENTS_CLIENTS")
});

/// The server-sent player event stream that will relay player state updates to clients.
#[get("/stream")]
pub fn get_stream(
    _build_guard: ClientKey,
    player_event_queue: &State<Sender<PlayerEventStreamUpdate>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = player_event_queue.subscribe();
    let (guard_send, guard_rx) = channel::<()>();

    let stream = EventStream! {
        let _ = guard_send;
        CONNECTED_PLAYER_EVENTS_CLIENTS.inc();
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
    };

    // Decrement the metric when the guard is free again.
    rocket::tokio::spawn(async move {
        let _ = guard_rx.await;
        CONNECTED_PLAYER_EVENTS_CLIENTS.dec();
    });

    stream
}
