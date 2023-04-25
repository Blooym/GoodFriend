use rocket::tokio::sync::broadcast::Sender;
use rocket::State;

use crate::{
    requests::update_player_loginstate::UpdatePlayerLoginStateRequest,
    responses::playerstate::{EventStreamPlayerStateUpdateResponse, PlayerStateUpdateType},
};

/// Sends a logout to the server-sent event stream.
#[put("/update/loginstate?<update..>")]
pub async fn put_update_loginstate(
    update: UpdatePlayerLoginStateRequest,
    queue: &State<Sender<EventStreamPlayerStateUpdateResponse>>,
) {
    let _ = queue.send(EventStreamPlayerStateUpdateResponse {
        content_id_hash: update.content_id_hash.to_owned(),
        content_id_salt: update.content_id_salt.to_owned(),
        state_update_type: PlayerStateUpdateType::LoginStateChange {
            logged_in: update.logged_in,
            datacenter_id: update.datacenter_id,
            world_id: update.world_id,
            territory_id: update.territory_id,
        },
    });
}
