#[macro_use]
extern crate rocket;

mod api;
mod config;

use anyhow::{Context, Result};
use api::routes::{
    announcements::{routes as announcements_routes, *},
    auth::routes as auth_routes,
    core::routes as core_routes,
    player_events::{routes as player_events_routes, *},
    static_files::routes as static_files_routes,
};
use clap::Parser;
use config::Config;
use dotenvy::dotenv;
use notify::{Event, RecommendedWatcher, Watcher};
use rocket::{
    shield::{self, Shield},
    tokio::sync::{broadcast::channel, RwLock},
};
use std::{path::PathBuf, sync::Arc};

#[derive(Parser, Clone, Debug)]
#[command(version, about)]
struct GoodFriendArguments {
    #[arg(
        short = 'c',
        long = "config",
        env = "GOODFRIEND_CONFIG",
        default_value = "./data/config.toml"
    )]
    /// A path to the configuration file to use.
    pub config_file: PathBuf,

    #[arg(
        long = "api-player-sse-cap",
        env = "GOODFRIEND_API_PLAYERSSE_CAP",
        default_value_t = 5000
    )]
    /// The capacity of the 'player events' SSE stream. This should be kept close to `announce-sse-cap`.
    pub player_sse_cap: usize,

    #[arg(
        long = "api-announce-sse-cap",
        env = "GOODFRIEND_API_ANNOUNCESSE_CAP",
        default_value_t = 5000
    )]
    /// The capacity of the 'announcements' SSE stream. This should be kept close to `player_sse_cap-sse-cap`.
    pub announce_sse_cap: usize,

    #[arg(
        long = "api-base-route",
        env = "GOODFRIEND_API_BASEROUTE",
        default_value = "/api"
    )]
    /// The route to put all non-static file API routes after.
    pub api_base_route: String,
}

#[rocket::main]
async fn main() -> Result<()> {
    dotenv().ok();
    let args = Arc::from(GoodFriendArguments::parse());

    // Load and validate the configuration before starting.
    let config = Config::get_or_create_from_path(&args.config_file).with_context(|| {
        format!(
            "Failed whilst trying to load or create configuration from {:?}",
            args.config_file
        )
    })?;
    let config = Arc::from(RwLock::from(config));

    // Create a watcher that reloads the configuration is changed
    let watcher_config = Arc::clone(&config);
    let watcher_args = Arc::clone(&args);
    let mut watcher = RecommendedWatcher::new(
        move |result: Result<Event, notify::Error>| {
            let event = result.unwrap();
            if event.kind.is_modify() || event.kind.is_create() || event.kind.is_remove() {
                println!(
                    "Change to the configuration detected, attempting reload of configuration...",
                );

                match Config::get_or_create_from_path(&watcher_args.config_file) {
                    Ok(config) => {
                        *watcher_config.blocking_write() = config;
                        println!("Successfully hot reloaded configuration file");
                    }
                    Err(err) => {
                        eprintln!("Failed to hot reload configuration file, configuration has been left unchanged: {:?}", err)
                    }
                }
            }
        },
        notify::Config::default(),
    )?;
    watcher.watch(
        args.config_file
            .parent()
            .context("Cannot get parent directory of config file")?,
        notify::RecursiveMode::Recursive,
    )?;

    // Build the rocket instance.
    let base_route = &Arc::clone(&args).api_base_route;
    let rocket = rocket::build()
        .manage(channel::<PlayerEventStreamUpdate>(args.player_sse_cap).0)
        .manage(channel::<AnnouncementMessage>(args.announce_sse_cap).0)
        .manage(config)
        .mount("/", static_files_routes())
        .mount([base_route, "/"].concat(), core_routes())
        .mount(
            [base_route, "/playerevents"].concat(),
            player_events_routes(),
        )
        .mount(
            [base_route, "/announcements"].concat(),
            announcements_routes(),
        )
        .mount([base_route, "/auth"].concat(), auth_routes())
        .attach(Shield::default().enable(shield::Frame::Deny));
    rocket.manage(args).launch().await?;
    Ok(())
}
