using System.Net;
using Beamable.Server;

namespace Beamable.SuiFederation.Features.SuiApi.Exceptions;

public class SuiApiException : MicroserviceException
{
    public SuiApiException(string message) : base((int)HttpStatusCode.InternalServerError, "SuiApiException",
        message)
    {
    }
}