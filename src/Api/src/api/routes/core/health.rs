use rocket::http::Status;

/// Get the health of the API, always returns a 200 OK if the API is running.
#[get("/health")]
pub async fn get_health() -> Status {
    Status::Ok
}
