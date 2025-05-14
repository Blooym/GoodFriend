mod favicon;
mod index;

use crate::api::routes::static_files::{favicon::get_favicon, index::get_index};

pub fn routes() -> Vec<rocket::Route> {
    routes![get_index, get_favicon]
}
