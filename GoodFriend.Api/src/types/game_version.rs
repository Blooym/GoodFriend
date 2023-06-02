use rocket::serde::{Deserialize, Serialize};
use std::{cmp::Ordering, fmt::Display};

/// Represents a game version.
#[derive(Debug, FromForm, Clone, Serialize, Deserialize, Eq, PartialEq)]
#[serde(crate = "rocket::serde")]
pub struct GameVersion {
    pub year: String,
    pub month: String,
    pub day: String,
    pub major: String,
    pub minor: String,
}

/// Represents an error when parsing a game version.
pub enum GameVersionError {
    /// The version string is invalid.
    InvalidVersion,

    /// The version string is too short.
    InvalidStringFormat,
}

impl GameVersion {
    /// Creates a new game version from a string formatted as `YYYY.MM.DD.MAJOR.MINOR`.
    pub fn from_str(version: &str) -> Result<Self, GameVersionError> {
        let mut split = version.split('.');
        if split.clone().count() != 5 {
            return Err(GameVersionError::InvalidStringFormat);
        }

        let year = split.next().unwrap();
        let month = split.next().unwrap();
        let day = split.next().unwrap();
        let major = split.next().unwrap();
        let minor = split.next().unwrap();

        if year.len() != 4
            || month.len() != 2
            || day.len() != 2
            || major.len() != 4
            || minor.len() != 4
        {
            return Err(GameVersionError::InvalidVersion);
        }

        Ok(Self {
            year: year.to_string(),
            month: month.to_string(),
            day: day.to_string(),
            major: major.to_string(),
            minor: minor.to_string(),
        })
    }
}

impl Default for GameVersion {
    fn default() -> Self {
        Self {
            year: String::from("2023"),
            month: String::from("01"),
            day: String::from("01"),
            major: String::from("0000"),
            minor: String::from("0000"),
        }
    }
}

impl Display for GameVersion {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "{}.{}.{}.{}.{}",
            self.year, self.month, self.day, self.major, self.minor
        )
    }
}

impl PartialOrd for GameVersion {
    fn partial_cmp(&self, other: &Self) -> Option<Ordering> {
        let self_version = format!(
            "{}{}{}{}{}",
            self.year, self.month, self.day, self.major, self.minor
        )
        .parse::<usize>()
        .unwrap_or(0);

        let other_version = format!(
            "{}{}{}{}{}",
            other.year, other.month, other.day, other.major, other.minor
        )
        .parse::<usize>()
        .unwrap_or(0);

        if self_version < other_version {
            Some(Ordering::Greater)
        } else if self_version > other_version {
            Some(Ordering::Less)
        } else {
            Some(Ordering::Equal)
        }
    }
}
