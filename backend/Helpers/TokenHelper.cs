using System.Security.Claims;

namespace backend.Helpers
{
    public static class TokenHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : throw new Exception("UserId not found in token.");
        }
    }
}