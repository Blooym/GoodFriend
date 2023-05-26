#![forbid(unsafe_code)]

#[macro_use]
extern crate rocket;

mod api;
mod config;
mod types;

use api::responses::playerevent::EventStreamPlayerStateUpdateResponse;
use config::Config;
use dotenv::dotenv;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};
use std::process;

const BASE_PATH: &str = "/api";

#[launch]
fn rocket() -> Rocket<Build> {
    dotenv().ok();

    if !Config::exists() {
        Config::default().save();
        eprintln!(
            "Error: Missing configuration file - one has been created for you. Please refer to the documentation and fill it out before starting the API again."
        );
        process::exit(1);
    }
    config::get_config_cached_prime_cache();

    println!("Starting GoodFriend API - Configuration cache primed.");
    rocket::build()
        .manage(channel::<EventStreamPlayerStateUpdateResponse>(1024).0)
        .mount("/", api::web::routes())
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
        )
}
