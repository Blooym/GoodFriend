use rocket::serde::{Deserialize, Serialize};

/// Represents a content id sha512 hash.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
#[field(validate = len(64..))]
pub struct ContentIdHash(pub String);
