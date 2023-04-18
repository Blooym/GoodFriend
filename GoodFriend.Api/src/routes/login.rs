use rocket::http::Status;
use rocket::tokio::sync::broadcast::Sender;
use rocket::State;

use crate::types::player::PlayerStateUpdate;

/// Sends a login event to the server-sent event stream.
#[put("/login?<datacenter_id>&<world_id>&<territory_id>&<content_id_hash>&<content_id_salt>")]
pub async fn handle_login(
    datacenter_id: usize,
    world_id: usize,
    territory_id: usize,
    content_id_hash: String,
    content_id_salt: String,
    queue: &State<Sender<PlayerStateUpdate>>,
) -> Status {
    if content_id_hash.len() < 128 || content_id_salt.len() < 32 {
        return Status::BadRequest;
    }

    let _ = queue.send(PlayerStateUpdate {
        content_id_hash,
        content_id_salt,
        datacenter_id,
        territory_id,
        world_id,
        is_logged_in: true,
    });
    Status::Ok
}
