using EPiServer.Core;

namespace Foundation.Social.ViewModels
{
    public class CommentFormViewModel
    {
        public string Body { get; set; }

        public bool SendActivity { get; set; }

        public ContentReference CurrentLink { get; set; }
    }
}