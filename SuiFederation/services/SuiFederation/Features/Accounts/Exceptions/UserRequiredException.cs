using System.Net;
using Beamable.Server;

namespace SuiFederation.Features.Accounts.Exceptions;

public class UserRequiredException : MicroserviceException

{
    public UserRequiredException() : base((int)HttpStatusCode.Unauthorized, "UserRequired", "")
    {
    }
}