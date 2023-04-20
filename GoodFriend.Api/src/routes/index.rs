use std::borrow::Cow;

use rocket::response::content::RawHtml;

use crate::Asset;

/// Displays the index.html page for the root path
#[get("/")]
pub async fn get_index() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset = Asset::get("index.html")?;
    Some(RawHtml(asset.data))
}
