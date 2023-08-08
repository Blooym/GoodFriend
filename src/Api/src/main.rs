#![forbid(unsafe_code)]

#[macro_use]
extern crate rocket;

mod api;

use api::routes::AnnouncementStreamUpdate;
use api::routes::PlayerEventStreamUpdate;
use api::routes::{announcements_routes, core_routes, player_events_routes, static_files_routes};
use api::types::config::{get_config_cached_prime_cache, Config};
use dotenv::dotenv;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};
use std::process;

/// The base path for all API routes.
const BASE_PATH: &str = "/api";

#[launch]
async fn rocket() -> Rocket<Build> {
    dotenv().ok();

    // Validate configuration before starting & prime the cache.
    match Config::exists() {
        true => match Config::get() {
            Ok(config) => config,
            Err(e) => {
                eprintln!("Error: Invalid configuration file - please fix the errors and restart the API.");
                for (field, errors) in e.field_errors().iter() {
                    for error in errors.iter() {
                        eprintln!("{}: {}", field, error);
                    }
                }
                process::exit(1);
            }
        },
        false => Config::default(),
    };
    get_config_cached_prime_cache();

    rocket::build()
        .manage(channel::<PlayerEventStreamUpdate>(15000).0)
        .manage(channel::<AnnouncementStreamUpdate>(15000).0)
        .mount("/", static_files_routes())
        .mount([BASE_PATH, "/"].concat(), core_routes())
        .mount(
            [BASE_PATH, "/playerevents"].concat(),
            player_events_routes(),
        )
        .mount(
            [BASE_PATH, "/announcements"].concat(),
            announcements_routes(),
        )
        .attach(
            Shield::default()
                .enable(shield::XssFilter::Enable)
                .enable(shield::Frame::Deny)
                .enable(shield::Referrer::NoReferrer)
                .enable(shield::NoSniff::Enable)
                .enable(shield::Prefetch::Off),
        )
}
