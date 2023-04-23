use rocket::serde::{Deserialize, Serialize};

#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct LogoutRequest {
    #[field(validate = len(128..))]
    pub content_id_hash: String,
    #[field(validate = len(32..))]
    pub content_id_salt: String,
    pub datacenter_id: u32,
    pub world_id: u32,
    pub territory_id: u16,
}
