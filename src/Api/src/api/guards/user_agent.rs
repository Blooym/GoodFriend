use crate::config::{get_config_cached, UserAgentBlockMode};
use rocket::{
    http::Status,
    request::{FromRequest, Outcome},
    Request,
};

/// The header name for the user agent.
const USER_AGENT_HEADER: &str = "User-Agent";

/// A guard that checks if the user agent is accepted or not.
pub struct UserAgentGuard;

#[rocket::async_trait]
impl<'r> FromRequest<'r> for UserAgentGuard {
    type Error = BannedAgentError;

    async fn from_request(req: &'r Request<'_>) -> Outcome<Self, Self::Error> {
        let sent_agent_header = req.headers().get_one(USER_AGENT_HEADER).unwrap_or_default();
        if sent_agent_header.is_empty() {
            return Outcome::Failure((Status::BadRequest, BannedAgentError::AgentMissing));
        }

        let blocklist = get_config_cached().blocked_user_agents;
        for (agent, mode) in blocklist {
            match mode {
                UserAgentBlockMode::ExactMatch => {
                    if sent_agent_header == agent {
                        return Outcome::Failure((
                            Status::Forbidden,
                            BannedAgentError::AgentBanned,
                        ));
                    }
                }
                UserAgentBlockMode::PartialMatch => {
                    if sent_agent_header.contains(&agent) {
                        return Outcome::Failure((
                            Status::Forbidden,
                            BannedAgentError::AgentBanned,
                        ));
                    }
                }
            }
        }

        Outcome::Success(UserAgentGuard)
    }
}

/// The error that can occur when checking the user agent.
#[derive(Debug)]
pub enum BannedAgentError {
    AgentBanned,
    AgentMissing,
}
