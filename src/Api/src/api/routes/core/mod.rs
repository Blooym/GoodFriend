mod features;
mod health;
mod metadata;

use crate::api::routes::core::{
    features::get_features, health::get_health, metadata::get_metadata,
};

pub fn routes() -> Vec<rocket::Route> {
    routes![get_health, get_metadata, get_features]
}
