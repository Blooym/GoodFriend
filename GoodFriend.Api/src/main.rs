#![forbid(unsafe_code)]

#[macro_use]
extern crate rocket;

mod api;
mod config;
mod types;

use api::responses::player_event::EventStreamPlayerStateUpdateResponse;
use config::Config;
use dotenv::dotenv;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};
use rocket_async_compression::Compression;
use std::process;

/// The base path for all API routes.
const BASE_PATH: &str = "/api";

#[launch]
fn rocket() -> Rocket<Build> {
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
        false => {
            eprintln!("Error: No configuration file was detected - please create in the data directory and restart the API.");
            process::exit(1);
        }
    };
    config::get_config_cached_prime_cache();

    let server = rocket::build()
        .manage(channel::<EventStreamPlayerStateUpdateResponse>(50000).0)
        .mount("/", api::static_content::routes())
        .mount([BASE_PATH, "/"].concat(), api::core::routes())
        .mount([BASE_PATH, "/update"].concat(), api::update::routes())
        .mount([BASE_PATH, "/events"].concat(), api::events::routes())
        .attach(
            Shield::default()
                .enable(shield::XssFilter::Enable)
                .enable(shield::Frame::Deny)
                .enable(shield::Referrer::NoReferrer)
                .enable(shield::NoSniff::Enable)
                .enable(shield::Prefetch::Off),
        );

    if cfg!(debug_assertions) {
        server
    } else {
        server.attach(Compression::fairing())
    }
}
