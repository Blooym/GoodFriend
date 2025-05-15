mod connections;
mod health;

use crate::api::routes::core::health::get_health;
use connections::get_connections;

pub fn routes() -> Vec<rocket::Route> {
    routes![get_health, get_connections]
}
