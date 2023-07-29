use rocket::{http::ContentType, response::content::RawHtml};
use rust_embed::RustEmbed;
use std::{borrow::Cow, ffi::OsStr, path::PathBuf};

/// Returns a list of routes for the web API.
pub fn routes() -> Vec<rocket::Route> {
    routes![get_serve_static, get_index, get_favicon]
}

/// Compiles the contents of the static directory into the binary at build-time
/// for portability.
#[derive(RustEmbed)]
#[folder = "./static/"]
struct Asset;

/// Serve index.html as the root path
#[get("/")]
pub async fn get_index() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset = Asset::get("index.html")?;
    Some(RawHtml(asset.data))
}

/// Serve the favicon from the static files.
#[get("/favicon.ico")]
pub async fn get_favicon() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset: rust_embed::EmbeddedFile = Asset::get("favicon.ico")?;
    Some(RawHtml(asset.data))
}

/// Serves static assets from the static directory or the application bundle.
#[get("/static/<file..>")]
pub async fn get_serve_static(file: PathBuf) -> Option<(ContentType, Cow<'static, [u8]>)> {
    let filename = file.display().to_string();
    let asset = Asset::get(&filename)?;
    let content_type: ContentType = file
        .extension()
        .and_then(OsStr::to_str)
        .and_then(ContentType::from_extension)
        .unwrap_or(ContentType::Bytes);

    Some((content_type, asset.data))
}
