use rocket::tokio::sync::broadcast::Sender;
use rocket::State;

use crate::requests::login::LoginRequest;
use crate::responses::playerstate::PlayerStateUpdateResponse;

/// Sends a login event to the server-sent event stream.
// #[put("/login?<datacenter_id>&<world_id>&<territory_id>&<content_id_hash>&<content_id_salt>")]
#[put("/login?<update..>")]
pub async fn put_login(update: LoginRequest, queue: &State<Sender<PlayerStateUpdateResponse>>) {
    let _ = queue.send(PlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash.to_owned(),
        content_id_salt: update.content_id_salt.to_owned(),
        datacenter_id: update.datacenter_id,
        world_id: update.world_id,
        territory_id: update.territory_id,
        is_logged_in: true,
    });
}
