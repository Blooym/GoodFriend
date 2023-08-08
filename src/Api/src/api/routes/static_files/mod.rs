mod favicon;
mod files;
mod index;

use crate::api::routes::static_files::{
    favicon::get_favicon, files::get_static_file, index::get_index,
};
use rust_embed::RustEmbed;

/// Compiles the contents of the static directory into the binary at build-time
/// for portability.
#[derive(RustEmbed)]
#[folder = "./static/"]
struct Asset;

pub fn routes() -> Vec<rocket::Route> {
    routes![get_static_file, get_index, get_favicon]
}
