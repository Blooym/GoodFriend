use rocket::http::Status;

/// Get the health of the API.
#[get("/health")]
pub async fn get_health() -> Status {
    Status::Ok
}
