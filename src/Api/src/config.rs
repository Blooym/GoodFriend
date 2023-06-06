use crate::types::{api_about::ApiAbout, game_version::GameVersion};
use cached::proc_macro::cached;
use rocket::serde::{Deserialize, Serialize};
use std::{collections::HashMap, env, fs, path::Path};
use validator::{Validate, ValidationErrors};

/// Gets the currently cached config.
#[cached(time = 300)]
pub fn get_config_cached() -> Config {
    Config::get().unwrap_or_default()
}

/// Defines the applications configuration.
#[derive(Debug, Clone, Deserialize, Serialize, Eq, PartialEq, Validate, Default)]
#[serde(crate = "rocket::serde")]
pub struct Config {
    version: u8,
    pub about: ApiAbout,
    pub blocked_user_agents: HashMap<String, UserAgentBlockMode>,
    pub minimum_game_version: GameVersion,
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
