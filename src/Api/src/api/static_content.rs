use rocket::{http::ContentType, response::content::RawHtml};
use rust_embed::RustEmbed;
use std::{borrow::Cow, ffi::OsStr, path::PathBuf};

/// Returns a list of routes for the web API.
pub fn routes() -> Vec<rocket::Route> {
    routes![serve_static, index]
}

/// Compiles the contents of the static directory into the binary at build-time
/// for portability.
#[derive(RustEmbed)]
#[folder = "./static/"]
struct Asset;

/// Displays the index.html page for the root path
#[get("/")]
pub async fn index() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset = Asset::get("index.html")?;
    Some(RawHtml(asset.data))
}

/// Serves static assets that where bundled into the binary at build time.
#[get("/<file..>")]
pub async fn serve_static(file: PathBuf) -> Option<(ContentType, Cow<'static, [u8]>)> {
    let filename = file.display().to_string();
    let asset = Asset::get(&filename)?;
    let content_type: ContentType = file
        .extension()
        .and_then(OsStr::to_str)
        .and_then(ContentType::from_extension)
        .unwrap_or(ContentType::Bytes);

    Some((content_type, asset.data))
}
