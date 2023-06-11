use std::collections::HashMap;

use rocket::serde::{Deserialize, Serialize};
use url::Url;
use validator::{Validate, ValidationError};

/// The current message of the day for the API.
#[derive(Debug, Clone, Serialize, Deserialize, Eq, PartialEq, Validate)]
#[serde(crate = "rocket::serde")]
pub struct ApiAboutConfig {
    pub identifier: String,
    pub banner_url: String,
    pub motd: Motd,
    #[validate(custom = "validate_custom_urls")]
    pub custom_urls: HashMap<String, String>,
}

/// Validates the custom urls, key is url name, value is url.
fn validate_custom_urls(urls: &HashMap<String, String>) -> Result<(), ValidationError> {
    if urls.is_empty() {
        return Ok(());
    }

    for url in urls.values() {
        match Url::parse(url) {
            Ok(_) => (),
            Err(_) => {
                return Err(ValidationError::new(
                    "Came across a URL that could not be parsed when validating custom urls.",
                ));
            }
        }
    }

    Ok(())
}

impl Default for ApiAboutConfig {
    fn default() -> Self {
        Self {
            identifier: String::from("GoodFriend"),
            banner_url: String::from(
                "https://raw.githubusercontent.com/BitsOfAByte/GoodFriend/main/.assets/api-banner.png",
            ),
            motd: Motd::default(),
            custom_urls: HashMap::default(),
        }
    }
}

/// The current message of the day for the API.
#[derive(Debug, Clone, Serialize, Deserialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
pub struct Motd {
    pub message: String,
    pub ignore: bool,
}

impl Default for Motd {
    fn default() -> Self {
        Self {
            message: String::default(),
            ignore: true,
        }
    }
}
