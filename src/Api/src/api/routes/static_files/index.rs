use super::Asset;
use rocket::response::content::RawHtml;
use std::borrow::Cow;

/// Serve index.html as the root path
#[get("/")]
pub async fn get_index() -> Option<RawHtml<Cow<'static, [u8]>>> {
    let asset = Asset::get("index.html")?;
    Some(RawHtml(asset.data))
}
