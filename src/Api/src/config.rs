use anyhow::Result;
use rocket::serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;
use url::Url;

/// Defines the applications configuration.
#[derive(Debug, Clone, Deserialize, Serialize)]
#[serde(crate = "rocket::serde")]
pub struct Config {
    version: u8,
    pub about: ApiAboutConfig,
    pub security: ApiSecurityConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct ApiAboutConfig {
    pub identifier: String,
    pub description: String,
    pub banner_url: Url,
    pub custom_urls: HashMap<String, Url>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(crate = "rocket::serde")]
pub struct ApiSecurityConfig {
    pub authentication_tokens: Vec<String>,
    pub allowed_client_keys: Option<Vec<String>>,
}

impl Default for Config {
    fn default() -> Self {
        Self {
            version: 1,
            about: ApiAboutConfig::default(),
            security: ApiSecurityConfig::default(),
        }
    }
}

impl Default for ApiAboutConfig {
    fn default() -> Self {
        Self {
            identifier: String::from("GoodFriend"),
            banner_url: Url::parse("https://raw.githubusercontent.com/Blooym/GoodFriend/main/src/Api/static/banner.png").expect("Default banner URL parse error"),
            custom_urls: HashMap::default(),
            description: String::default(),
        }
    }
}

impl Config {
    /// Checks if the config file exists.
    pub fn exists(path: &PathBuf) -> bool {
        fs::metadata(path).is_ok()
    }

    /// Gets the config file from where the environment points to and parses it.
    pub fn get_or_create_from_path(path: &PathBuf) -> Result<Self> {
        if !Self::exists(path) {
            let config = Self::default();
            config.save_to_path(path)?;
            return Ok(config);
        }

        let config: Self = toml::from_str(&fs::read_to_string(path)?)?;
        Ok(config)
    }

    /// Creates a config file at the path where the environment points to.
    pub fn save_to_path(&self, path: &PathBuf) -> Result<()> {
        let config_toml = toml::to_string(&self)?;

        if let Some(parent) = path.parent() {
            if !parent.exists() {
                fs::create_dir_all(parent)?;
            }
        }
        fs::write(path, config_toml)?;

        Ok(())
    }
}
