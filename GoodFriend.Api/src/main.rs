#[macro_use]
extern crate rocket;

use std::borrow::Cow;
use std::ffi::OsStr;
use std::path::PathBuf;

use dotenv::dotenv;
use rocket::http::ContentType;
use rocket::response::content::RawHtml;
use rocket::response::stream::{Event, EventStream};
use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};
use rocket::shield::{self, Shield};
use rocket::tokio::select;
use rocket::tokio::sync::broadcast::{channel, error::RecvError, Sender};
use rocket::{Build, Rocket, Shutdown, State};
use rust_embed::RustEmbed;

/// Represents a player state update that is sent to clients.
///
/// The data included is not enough to identify a player unless the client
/// has access to both the original `content_id` and the salt from this update event.
#[derive(Debug, Clone, FromForm, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
struct PlayerStateUpdate {
    pub content_id_hash: String,
    pub is_logged_in: bool,
    pub datacenter_id: usize,
    pub world_id: usize,
    pub territory_id: usize,
    pub content_id_salt: String,
}

/// Represents metadata that a client can request to know more about the API.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
struct MetadataResponse {
    pub connected_clients: usize,
    pub status_url: String,
    pub donate_url: String,
}

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
            routes![index, static_files, login, logout, metadata, events],
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

/// The server-sent event stream that clients will subscribe to so that
/// they can see `PlayerStateUpdate` events.
#[get("/events")]
async fn events(queue: &State<Sender<PlayerStateUpdate>>, mut end: Shutdown) -> EventStream![] {
    let mut rx = queue.subscribe();
    EventStream! {
        loop {
            let msg = select! {
                msg = rx.recv() => match msg {
                    Ok(msg) => msg,
                    Err(RecvError::Closed) => break,
                    Err(RecvError::Lagged(_)) => continue,
                },
                _ = &mut end => break,
            };

            yield Event::json(&msg);
        }
    }
}

/// Sends a login event to the server-sent event stream.
#[put("/login?<datacenter_id>&<world_id>&<territory_id>&<content_id_hash>&<content_id_salt>")]
async fn login(
    datacenter_id: usize,
    world_id: usize,
    territory_id: usize,
    content_id_hash: String,
    content_id_salt: String,
    queue: &State<Sender<PlayerStateUpdate>>,
) {
    let _ = queue.send(PlayerStateUpdate {
        content_id_hash,
        content_id_salt,
        datacenter_id,
        territory_id,
        world_id,
        is_logged_in: true,
    });
}

/// Sends a logout to the server-sent event stream.
#[put("/logout?<datacenter_id>&<world_id>&<territory_id>&<content_id_hash>&<content_id_salt>")]
async fn logout(
    datacenter_id: usize,
    world_id: usize,
    territory_id: usize,
    content_id_hash: String,
    content_id_salt: String,
    queue: &State<Sender<PlayerStateUpdate>>,
) {
    let _ = queue.send(PlayerStateUpdate {
        content_id_hash,
        content_id_salt,
        datacenter_id,
        territory_id,
        world_id,
        is_logged_in: false,
    });
}

/// Gets metadata about the current status of the API.
#[get("/metadata")]
async fn metadata(queue: &State<Sender<PlayerStateUpdate>>) -> Json<MetadataResponse> {
    Json(MetadataResponse {
        connected_clients: queue.receiver_count(),
        status_url: std::env::var("APP_META_STATUS_PAGE").unwrap_or_default(),
        donate_url: std::env::var("APP_META_DONATION_PAGE").unwrap_or_default(),
    })
}

#[get("/")]
fn index() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset = Asset::get("index.html")?;
    Some(RawHtml(asset.data))
}

#[get("/<file..>")]
fn static_files(file: PathBuf) -> Option<(ContentType, Cow<'static, [u8]>)> {
    let filename = file.display().to_string();
    let asset = Asset::get(&filename)?;
    let content_type = file
        .extension()
        .and_then(OsStr::to_str)
        .and_then(ContentType::from_extension)
        .unwrap_or(ContentType::Bytes);

    Some((content_type, asset.data))
}

#[catch(404)]
fn not_found() -> &'static str {
    "Not found"
}
