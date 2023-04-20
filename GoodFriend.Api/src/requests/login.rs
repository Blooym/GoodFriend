use rocket::serde::{Deserialize, Serialize};

#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct LoginRequest {
    #[field(validate = len(100..))]
    pub content_id_hash: String,
    #[field(validate = len(30..))]
    pub content_id_salt: String,
    pub datacenter_id: u8,
    pub world_id: u8,
    pub territory_id: u16,
}
