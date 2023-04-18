#[macro_use]
extern crate rocket;

mod constants;
mod routes;
mod types;
use dotenv::dotenv;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};
use routes::events::handle_events;
use routes::index::handle_index;
use routes::login::handle_login;
use routes::logout::handle_logout;
use routes::metadata::handle_metadata;
use routes::motd::handle_motd;
use routes::static_files::handle_static_files;
use rust_embed::RustEmbed;
use types::player::PlayerStateUpdate;

/// Compiles the contents of the static directory into the binary at build-time
/// for portability.
#[derive(RustEmbed)]
#[folder = "./static"]
struct Asset;

#[launch]
fn rocket() -> Rocket<Build> {
    dotenv().ok();

    rocket::build()
        .manage(channel::<PlayerStateUpdate>(1024).0)
        .mount(
            "/",
            routes![
                handle_index,
                handle_static_files,
                handle_login,
                handle_logout,
                handle_metadata,
                handle_events,
                handle_motd
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
