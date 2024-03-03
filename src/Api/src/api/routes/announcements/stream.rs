use super::AnnouncementMessage;
use crate::api::guards::client_key::ClientKey;
use once_cell::sync::Lazy;
use rocket::futures::channel::oneshot::channel;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{error::RecvError, Sender};
use rocket::{Shutdown, State};
use rocket_prometheus::prometheus::IntGauge;

pub static CONNECTED_ANNOUNCEMENTS_CLIENTS: Lazy<IntGauge> = Lazy::new(|| {
    IntGauge::new(
        "connected_announcements_clients",
        "The number of clients currently connected to the announcements SSE stream",
    )
    .expect("Could not create CONNECTED_ANNOUNCEMENTS_CLIENTS")
});

/// Get a stream of announcements.
#[get("/stream")]
pub async fn get_stream(
    _build: ClientKey,
    announcement_stream: &State<Sender<AnnouncementMessage>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = announcement_stream.subscribe();
    let (guard_send, guard_rx) = channel::<()>();

    let stream = EventStream! {
        let _ = guard_send;
        CONNECTED_ANNOUNCEMENTS_CLIENTS.inc();
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
    };

    // Decrement the metric when the guard is free again.
    rocket::tokio::spawn(async move {
        let _ = guard_rx.await;
        CONNECTED_ANNOUNCEMENTS_CLIENTS.dec();
    });

    stream
}
