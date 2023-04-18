use std::{borrow::Cow, ffi::OsStr, path::PathBuf};

use rocket::http::ContentType;

use crate::Asset;

/// Serves static assets that where bundled into the binary at build time.
#[get("/<file..>")]
pub async fn handle_static_files(file: PathBuf) -> Option<(ContentType, Cow<'static, [u8]>)> {
    let filename = file.display().to_string();
    let asset = Asset::get(&filename)?;
    let content_type = file
        .extension()
        .and_then(OsStr::to_str)
        .and_then(ContentType::from_extension)
        .unwrap_or(ContentType::Bytes);

    Some((content_type, asset.data))
}
