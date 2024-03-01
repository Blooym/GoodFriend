#[macro_use]
extern crate rocket;

mod api;
mod config;

use anyhow::bail;
use anyhow::Context;
use anyhow::Result;
use api::routes::AnnouncementStreamUpdate;
use api::routes::PlayerEventStreamUpdate;
use api::routes::{announcements_routes, core_routes, player_events_routes, static_files_routes};
use config::Config;
use dotenvy::dotenv;
use notify::Event;
use notify::RecommendedWatcher;
use notify::Watcher;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::tokio::sync::RwLock;
use std::sync::Arc;
use validator::Validate;

/// The total amount of people connected to the announcement stream at any time.
const ANNOUNCEMENT_STREAM_CAPACITY: usize = 20000;

/// The total amount of people connected to the player evemt stream stream at any time.
const PLAYER_EVENT_STREAM_CAPACITY: usize = 20000;

/// The base path for all API routes.
const API_BASE_ROUTE: &str = "/api";

#[rocket::main]
async fn main() -> Result<()> {
    dotenv().ok();

    // Load and validate the configuration before starting.
    let config_file_path = Config::get_config_file_path()?;
    let config: Config = Config::get_or_create_from_path(&config_file_path)?;
    if let Err(err) = config.validate() {
        for (field, errors) in &err.field_errors() {
            for error in errors.iter() {
                eprintln!("{field}: {error}");
            }
        }
        bail!("Error: Invalid configuration file - please fix the errors and restart the server.")
    }

    // Create a watcher that reloads the configuration is changed
    let config = Arc::from(RwLock::from(config));
    let watcher_config = Arc::clone(&config);
    let mut watcher = RecommendedWatcher::new(
        move |result: Result<Event, notify::Error>| {
            let event = result.unwrap();
            println!("event: {:?}", event.kind);
            if event.kind.is_modify() || event.kind.is_create() || event.kind.is_remove() {
                println!(
                    "Change to the configuration detected, attempting reload of configuration...",
                );

                if let Ok(directory) = Config::get_config_file_path() {
                    match Config::get_or_create_from_path(&directory) {
                        Ok(config) => {
                            *watcher_config.blocking_write() = config;
                            println!("Successfully hot reloaded configuration file");
                        }
                        Err(err) => {
                            eprintln!("Failed to hot reload configuration file, configuration has been left unchanged: {:?}", err)
                        }
                    }
                }
            }
        },
        notify::Config::default(),
    )?;

    watcher.watch(
        config_file_path
            .parent()
            .context("Cannot get parent directory of config file")?,
        notify::RecursiveMode::Recursive,
    )?;

    rocket::build()
        .manage(channel::<PlayerEventStreamUpdate>(PLAYER_EVENT_STREAM_CAPACITY).0)
        .manage(channel::<AnnouncementStreamUpdate>(ANNOUNCEMENT_STREAM_CAPACITY).0)
        .manage(config)
        .mount("/", static_files_routes())
        .mount([API_BASE_ROUTE, "/"].concat(), core_routes())
        .mount(
            [API_BASE_ROUTE, "/playerevents"].concat(),
            player_events_routes(),
        )
        .mount(
            [API_BASE_ROUTE, "/announcements"].concat(),
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
        .launch()
        .await?;

    Ok(())
}
