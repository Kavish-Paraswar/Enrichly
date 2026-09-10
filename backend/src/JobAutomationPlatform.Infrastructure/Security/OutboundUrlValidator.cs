using System.Net;

namespace JobAutomationPlatform.Infrastructure.Security;

public static class OutboundUrlValidator
{
    private static readonly IPAddress[] BlockedIps =
    [
        IPAddress.Parse("127.0.0.0"),
        IPAddress.Parse("10.0.0.0"),
        IPAddress.Parse("172.16.0.0"),
        IPAddress.Parse("192.168.0.0"),
        IPAddress.Parse("169.254.169.254"),
        IPAddress.IPv6Loopback,
        IPAddress.IPv6None,
    ];

    public static async Task EnsureSafeAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (uri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("Only http and https outbound URLs are allowed.");
        }

        if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) || uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Localhost outbound URLs are not allowed.");
        }

        if (IPAddress.TryParse(uri.Host, out var address))
        {
            if (IsBlocked(address))
            {
                throw new InvalidOperationException("Outbound URLs that resolve to private or metadata IP ranges are not allowed.");
            }

            return;
        }

        var addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
        if (addresses.Any(IsBlocked))
        {
            throw new InvalidOperationException("Outbound URLs that resolve to private or metadata IP ranges are not allowed.");
        }
    }

    private static bool IsBlocked(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (BlockedIps.Any(blocked => address.Equals(blocked)))
        {
            return true;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 10
                || (bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || (bytes[0] == 169 && bytes[1] == 254);
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast || address.IsIPv6Teredo || address.IsIPv6UniqueLocal;
        }

        return false;
    }
}
