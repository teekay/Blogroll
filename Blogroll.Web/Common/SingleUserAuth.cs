using Blogroll.Common.Commons;

namespace Blogroll.Web.Common
{
    /// <summary>
    /// Encapsulates a password, and can tell you whether or not the provided password
    /// matches it.
    /// If the provided password is empty, then all attempts to authenticate will succeed.
    /// </summary>
    internal sealed class SingleUserAuth: IAuthenticating
    {
        public SingleUserAuth(string configuredPassword)
        {
            _configuredPassword = new SolidString(configuredPassword);
        }

        private readonly string _configuredPassword;


        public bool Authenticated(string password) =>
            string.IsNullOrEmpty(_configuredPassword) ||
            password == _configuredPassword;
    }
}