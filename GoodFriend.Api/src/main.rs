#[macro_use]
extern crate rocket;

mod constants;
mod requests;
mod responses;
mod routes;
mod types;

use dotenv::dotenv;
use responses::playerstate::EventStreamPlayerStateUpdateResponse;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};
use routes::index::get_index;
use routes::metadata::get_metadata;
use routes::motd::get_motd;
use routes::player_events::get_player_events;
use routes::static_files::get_static_file;
use routes::update_loginstate::put_update_loginstate;
use rust_embed::RustEmbed;

/// Compiles the contents of the static directory into the binary at build-time
/// for portability.
#[derive(RustEmbed)]
#[folder = "./static"]
struct Asset;

#[launch]
fn rocket() -> Rocket<Build> {
    dotenv().ok();

    rocket::build()
        .manage(channel::<EventStreamPlayerStateUpdateResponse>(1024).0)
        .mount(
            "/",
            routes![
                get_index,
                get_static_file,
                put_update_loginstate,
                get_metadata,
                get_player_events,
                get_motd,
            ],
        )
        .register("/", catchers![not_found])
        .attach(
            Shield::default()
                .enable(shield::XssFilter::Enable)
                .enable(shield::Frame::Deny)
                .enable(shield::Referrer::NoReferrer)
                .enable(shield::NoSniff::Enable)
                .enable(shield::Prefetch::Off),
        )
}

#[catch(404)]
fn not_found() -> &'static str {
    "Not found"
}
