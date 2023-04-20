use rocket::response::stream::{Event, EventStream};
use rocket::serde::{Deserialize, Serialize};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{error::RecvError, Sender};
use rocket::{Shutdown, State};

use crate::responses::playerstate::PlayerStateUpdateResponse;

#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct LoginRequest {
    #[field(validate = len(100..))]
    pub content_id_hash: String,
    #[field(validate = len(30..))]
    pub content_id_salt: String,
    pub datacenter_id: u8,
    pub world_id: u8,
    pub territory_id: u16,
}

/// The server-sent event stream that clients will subscribe to so that
/// they can see `PlayerStateUpdate` events.
#[get("/events")]
pub async fn get_events(
    queue: &State<Sender<PlayerStateUpdateResponse>>,
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
