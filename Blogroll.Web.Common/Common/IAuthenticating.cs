namespace Blogroll.Web.Common.Common
{
    /// <summary>
    /// Contract for rather simple password authentication.
    /// </summary>
    public interface IAuthenticating
    {
        bool Authenticated(string password);
    }
}