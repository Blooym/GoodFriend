use rocket::serde::{Deserialize, Serialize};

/// Represents a content id hash.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
#[field(validate = len(64..))]
pub struct ContentIdHash(String);

/// Represents a content id salt.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
#[field(validate = len(32..))]
pub struct ContentIdSalt(String);
