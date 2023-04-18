use rocket::serde::json::Json;

use crate::{
    constants::{ENV_MOTD_FILE_PATH, ENV_MOTD_FILE_PATH_DEFAULT},
    types::motd::{MotdBrand, MotdResponse, MotdUrgency},
};

/// Relays a "message of the day" created from the motd.json file.
#[get("/motd")]
pub async fn handle_motd() -> Json<MotdResponse> {
    let motd_file_path =
        std::env::var(ENV_MOTD_FILE_PATH).unwrap_or(String::from(ENV_MOTD_FILE_PATH_DEFAULT));
    let motd_file = std::fs::read_to_string(motd_file_path).unwrap_or_default();

    let mut motd: MotdResponse =
        rocket::serde::json::from_str(&motd_file).unwrap_or(MotdResponse {
            content: String::from(""),
            urgency: MotdUrgency::Information,
            brand: MotdBrand::General,
            ignore: true,
        });

    if motd.content.is_empty() {
        motd.ignore = true;
    }

    Json(motd)
}
