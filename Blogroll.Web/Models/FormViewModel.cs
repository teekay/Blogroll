using System.IO;
using Blogroll.Common.Links;
using Blogroll.Web.Common.Media;

namespace Blogroll.Web.Models
{
    public class FormViewModel
    {
        public FormViewModel(string templatePath, ILink link)
        {
            _templatePath = templatePath;
            _link = link;
        }

        private readonly string _templatePath;
        private readonly ILink _link;

        public override string ToString() => _link.PrintedTo(new HtmlMedia(File.ReadAllText(_templatePath)));
    }
}
