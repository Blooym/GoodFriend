use super::Asset;
use rocket::http::ContentType;
use std::{borrow::Cow, ffi::OsStr, path::PathBuf};

/// Serves static assets from the static directory or the application bundle.
#[get("/static/<file..>")]
pub async fn get_static_file(file: PathBuf) -> Option<(ContentType, Cow<'static, [u8]>)> {
    let filename = file.display().to_string();
    let asset = Asset::get(&filename)?;
    let content_type: ContentType = file
        .extension()
        .and_then(OsStr::to_str)
        .and_then(ContentType::from_extension)
        .unwrap_or(ContentType::Bytes);

    Some((content_type, asset.data))
}
