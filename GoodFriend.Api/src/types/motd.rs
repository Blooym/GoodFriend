/// Represents motd that a client can request.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct MotdResponse {
    pub content: String,
    pub urgency: MotdUrgency,
    pub brand: MotdBrand,
    pub ignore: bool,
}

use rocket::serde::{Deserialize, Serialize};

/// Represents the urgency of a motd.
#[derive(Debug, FromFormField, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub enum MotdUrgency {
    Information = 0,
    Warning = 2,
    Critical = 3,
}

/// Represents the type of motd.
#[derive(Debug, FromFormField, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub enum MotdBrand {
    Maintenance = 0,
    Issue = 1,
    General = 2,
}
