#[macro_use]
extern crate rocket;

mod constants;
mod requests;
mod responses;
mod routes;
use dotenv::dotenv;
use responses::playerstate::PlayerStateUpdateResponse;
use rocket::shield::{self, Shield};
use rocket::tokio::sync::broadcast::channel;
use rocket::{Build, Rocket};

use routes::events::get_events;
use routes::index::get_index;
use routes::login::put_login;
use routes::logout::put_logout;
use routes::metadata::get_metadata;
use routes::motd::get_motd;
use routes::static_files::get_static_file;
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
        .manage(channel::<PlayerStateUpdateResponse>(1024).0)
        .mount(
            "/",
            routes![
                get_index,
                get_static_file,
                put_login,
                put_logout,
                get_metadata,
                get_events,
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
