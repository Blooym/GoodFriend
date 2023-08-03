use rocket::serde::json::Json;
use rocket::serde::{Deserialize, Serialize};

/// Lists the available features from the specification that this API supports.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub struct FeatureSet {
    /// The features supported by this API.
    pub features: Vec<Feature>,
}

/// Represents a feature with its subcomponents.
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
pub enum Feature {
    PlayerEvents {
        /// Whether the player event stream is supported.
        stream: bool,

        /// Whether world change events are supported.
        worldchange: bool,

        /// Whether login state change events are supported.
        loginstate: bool,
    },
    Announcements {
        /// Whether the announcements stream is supported.
        stream: bool,

        /// Whether sending announcements is supported.
        send: bool,
    },
    Metadata {
        /// Whether getting metadata is supported.
        get: bool,
    },
}

/// Returns the features supported by this API.
#[get("/features")]
pub async fn get_features() -> Json<FeatureSet> {
    Json(FeatureSet {
        features: vec![
            Feature::PlayerEvents {
                stream: true,
                worldchange: true,
                loginstate: true,
            },
            Feature::Announcements {
                stream: true,
                send: true,
            },
            Feature::Metadata { get: true },
        ],
    })
}
