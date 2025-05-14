use rocket::http::ContentType;

/// Serve the favicon from the static files.
#[get("/favicon.ico")]
pub async fn get_favicon() -> Option<(ContentType, &'static [u8])> {
    let asset = include_bytes!("../../../../static/favicon.ico");
    Some((ContentType::Icon, asset))
}
