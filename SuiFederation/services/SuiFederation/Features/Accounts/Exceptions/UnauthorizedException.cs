using System.Net;
using Beamable.Server;

namespace SuiFederation.Features.Accounts.Exceptions;

public class UnauthorizedException : MicroserviceException
{
    public UnauthorizedException() : base((int)HttpStatusCode.Unauthorized, "Unauthorized", "")
    {
    }
}