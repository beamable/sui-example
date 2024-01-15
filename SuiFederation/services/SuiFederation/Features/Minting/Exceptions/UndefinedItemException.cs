using System.Net;
using Beamable.Server;

namespace Beamable.SuiFederation.Features.Minting.Exceptions;

public class UndefinedItemException : MicroserviceException
{
    public UndefinedItemException(string message) : base((int)HttpStatusCode.BadRequest, "UndefinedItemException",
        message)
    {
    }
}