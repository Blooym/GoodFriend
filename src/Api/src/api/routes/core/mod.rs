mod health;

use crate::api::routes::core::health::get_health;

pub fn routes() -> Vec<rocket::Route> {
    routes![get_health]
}
