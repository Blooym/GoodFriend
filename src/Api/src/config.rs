use anyhow::{Context, Result};
use rocket::serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::path::PathBuf;
use std::{env, fs};
use url::Url;
use validator::{Validate, ValidationError};

/// Defines the applications configuration.
#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq, Validate)]
#[serde(crate = "rocket::serde")]
pub struct Config {
    version: u8,
    pub about: ApiAboutConfig,
    pub authentication: ApiAuthenticationConfig,
}

impl Default for Config {
    fn default() -> Self {
        Self {
            version: 1,
            about: ApiAboutConfig::default(),
            authentication: ApiAuthenticationConfig::default(),
        }
    }
}

/// The environment variable for the GoodFriend data directory.
const DATA_DIRECTORY_ENV: &str = "GOODFRIEND_DATA_DIRECTORY";

/// The default data directory name. At runtime this will be appended to the executable working directory.
const DATA_DIRECTORY_DEFAULT_NAME: &str = "data";

/// The name of the configuration file.
const CONFIG_FILE_NAME: &str = "config.toml";

impl Config {
    /// Get the path to the data directory..
    fn get_data_directory_path() -> Result<PathBuf> {
        Ok(PathBuf::from(
            env::var(DATA_DIRECTORY_ENV).unwrap_or(
                env::current_dir()?
                    .join(DATA_DIRECTORY_DEFAULT_NAME)
                    .to_str()
                    .context("Failed to convert path to str")?
                    .to_string(),
            ),
        ))
    }

    /// Get the path to the configuration file from the environment.
    pub fn get_config_file_path() -> Result<PathBuf> {
        Ok(Self::get_data_directory_path()?.join(CONFIG_FILE_NAME))
    }

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

        let config: Self = toml::from_str(&fs::read_to_string(&path)?)?;
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

#[derive(Debug, Clone, Serialize, Deserialize, Eq, PartialEq, Validate)]
#[serde(crate = "rocket::serde")]
pub struct ApiAboutConfig {
    pub identifier: String,
    pub banner_url: String,
    pub description: String,
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
            banner_url: String::from("https://raw.githubusercontent.com/Blooym/GoodFriend/main/src/Api/static/banner.png"),
            custom_urls: HashMap::default(),
            description: String::default(),
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, Eq, PartialEq, Validate, Default)]
#[serde(crate = "rocket::serde")]
pub struct ApiAuthenticationConfig {
    pub tokens: Vec<String>,
}
