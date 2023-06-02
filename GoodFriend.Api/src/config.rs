use crate::types::{game_version::GameVersion, motd::Motd};
use cached::proc_macro::cached;
use rocket::serde::{Deserialize, Serialize};
use std::{collections::HashMap, env, fs, path::Path};
use url::Url;
use validator::{Validate, ValidationError, ValidationErrors};

/// Gets the currently cached config.
#[cached(time = 60)]
pub fn get_config_cached() -> Config {
    Config::get().unwrap_or_default()
}

/// Defines the applications configuration.
#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq, Validate)]
#[serde(crate = "rocket::serde")]
pub struct Config {
    version: u8,
    pub motd: Motd,
    pub blocked_user_agents: HashMap<String, UserAgentBlockMode>,
    #[validate(custom = "validate_custom_urls")]
    pub custom_urls: HashMap<String, String>,
    pub minimum_game_version: GameVersion,
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

/// Defines the user agent block mode.
#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
pub enum UserAgentBlockMode {
    ExactMatch,
    PartialMatch,
}

impl Config {
    /// Gets the config file path from the environment.
    fn get_config_file_path() -> String {
        const ENV_CONFIG_FILE_LOCATION: &str = "CONFIG_FILE_LOCATION";
        const DEFAULT_CONFIG_FILE_LOCATION: &str = "./data/config.toml";
        env::var(ENV_CONFIG_FILE_LOCATION).unwrap_or(String::from(DEFAULT_CONFIG_FILE_LOCATION))
    }

    /// Checks if the config file exists.
    pub fn exists() -> bool {
        fs::metadata(Self::get_config_file_path()).is_ok()
    }

    /// Creates a config file at the path where the environment points to.
    pub fn save(&self) {
        let config_toml = toml::to_string(&self).unwrap_or(String::default());
        let config_file_path = Self::get_config_file_path();

        let path = Path::new(&config_file_path);
        if let Some(parent) = path.parent() {
            if !parent.exists() {
                fs::create_dir_all(parent).unwrap_or_default();
            }
        }

        fs::write(config_file_path, config_toml).unwrap_or_default();
    }

    /// Gets the config file from where the environment points to and parses it.
    pub fn get() -> Result<Self, ValidationErrors> {
        let config_file_path = Self::get_config_file_path();

        if !Path::new(&config_file_path).exists() {
            let config = Self::default();
            config.save();
            return Ok(config);
        }

        let config_toml = fs::read_to_string(config_file_path).unwrap_or_default();
        let config: Self = toml::from_str(&config_toml).unwrap_or_default();

        match config.validate() {
            Ok(_) => Ok(config),
            Err(e) => Err(e),
        }
    }
}

impl Default for Config {
    fn default() -> Self {
        Self {
            version: 0,
            motd: Motd::default(),
            blocked_user_agents: HashMap::default(),
            minimum_game_version: GameVersion::default(),
            custom_urls: HashMap::default(),
        }
    }
}
