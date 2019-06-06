namespace Blogroll.Common.Media
{
    public interface IMedia
    {
        IMedia With(string key, string value);
        IMedia With(string key, double value);
        IMedia With(string key, object value);
        byte[] Bytes();
    }
}
