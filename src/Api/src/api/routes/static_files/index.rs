use rocket::response::content::RawHtml;

/// Serve index.html as the root path
#[get("/")]
pub async fn get_index() -> Option<RawHtml<&'static [u8]>> {
    Some(RawHtml(include_bytes!("../../../../static/index.html")))
}
