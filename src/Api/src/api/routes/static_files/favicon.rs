use super::Asset;
use rocket::http::ContentType;
use rust_embed::EmbeddedFile;
use std::borrow::Cow;

/// Serve the favicon from the static files.
#[get("/favicon.ico")]
pub async fn get_favicon() -> Option<(ContentType, Cow<'static, [u8]>)> {
    let asset: EmbeddedFile = Asset::get("favicon.ico")?;
    Some((ContentType::Icon, asset.data))
}
