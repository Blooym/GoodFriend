use super::Asset;
use rocket::response::content::RawHtml;
use std::borrow::Cow;

/// Serve the favicon from the static files.
#[get("/favicon.ico")]
pub async fn get_favicon() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset: rust_embed::EmbeddedFile = Asset::get("favicon.ico")?;
    Some(RawHtml(asset.data))
}
