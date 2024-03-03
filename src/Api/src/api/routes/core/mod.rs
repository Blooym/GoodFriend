mod health;
mod metadata;

use crate::api::routes::core::{health::get_health, metadata::get_metadata};

pub fn routes() -> Vec<rocket::Route> {
    routes![get_health, get_metadata]
}
