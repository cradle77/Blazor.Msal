using System;

namespace Des.Blazor.Authorization.Msal.Utils
{
    internal static class Guard
    {
        public static void ThrowIfNull(object source, string parameterName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
