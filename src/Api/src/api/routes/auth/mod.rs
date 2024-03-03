mod validate;

use crate::api::routes::auth::validate::get_validate_auth;

pub fn routes() -> Vec<rocket::Route> {
    routes![get_validate_auth]
}
