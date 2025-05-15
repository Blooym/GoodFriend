#[macro_use]
extern crate rocket;

mod api;

use anyhow::Result;
use api::routes::{
    announcements::{routes as announcements_routes, *},
    auth::routes as auth_routes,
    core::routes as core_routes,
    player_events::{routes as player_events_routes, *},
    static_files::routes as static_files_routes,
};
use clap::Parser;
use dotenvy::dotenv;
use rocket::{
    shield::{self, Shield},
    tokio::sync::broadcast::channel,
};

const BASE_ROUTE: &str = "/api";

#[derive(Parser, Clone, Debug)]
#[command(version, about)]
struct Arguments {
    #[arg(
        long = "api-auth-tokens",
        env = "GOODFRIEND_API_AUTH_TOKENS",
        value_delimiter = ','
    )]
    pub authentication_tokens: Vec<String>,

    #[arg(
        long = "api-client-keys",
        env = "GOODFRIEND_API_CLIENT_KEYS",
        value_delimiter = ','
    )]
    pub allowed_client_keys: Option<Vec<String>>,

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
}

#[rocket::main]
async fn main() -> Result<()> {
    dotenv().ok();
    let args = Arguments::parse();
    rocket::build()
        .manage(channel::<PlayerEventStreamUpdate>(args.player_sse_cap).0)
        .manage(channel::<AnnouncementMessage>(args.announce_sse_cap).0)
        .mount("/", static_files_routes())
        .mount([BASE_ROUTE, "/"].concat(), core_routes())
        .mount(
            [BASE_ROUTE, "/playerevents"].concat(),
            player_events_routes(),
        )
        .mount(
            [BASE_ROUTE, "/announcements"].concat(),
            announcements_routes(),
        )
        .mount([BASE_ROUTE, "/auth"].concat(), auth_routes())
        .attach(Shield::default().enable(shield::Frame::Deny))
        .manage(args)
        .launch()
        .await?;
    Ok(())
}
