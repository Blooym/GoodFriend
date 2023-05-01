use rocket::serde::{Deserialize, Serialize};

#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
#[field(validate = len(64..))]
pub struct ContentIdHash(String);

#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
#[field(validate = len(32..))]
pub struct ContentIdSalt(String);
