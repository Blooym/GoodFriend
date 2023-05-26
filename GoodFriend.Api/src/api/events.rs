use super::guards::minimum_game_version::MinimumGameVersionGuard;
use super::guards::user_agent::UserAgentGuard;
use super::responses::playerevent::EventStreamPlayerStateUpdateResponse;
use rocket::response::stream::{Event, EventStream};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{error::RecvError, Sender};
use rocket::{Shutdown, State};

pub fn routes() -> Vec<rocket::Route> {
    routes![player_events]
}

/// The server-sent event stream that clients will subscribe to so that
/// they can see `PlayerStateUpdate` events.
#[get("/players")]
pub async fn player_events(
    _version_guard: MinimumGameVersionGuard,
    _agent_guard: UserAgentGuard,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
    mut end: Shutdown,
) -> EventStream![] {
    let mut rx = queue.subscribe();
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
