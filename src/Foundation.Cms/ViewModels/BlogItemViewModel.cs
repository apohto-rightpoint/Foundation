using EPiServer.Core;
using Foundation.Cms.Pages;
using System;
using System.Collections.Generic;

namespace Foundation.Cms.ViewModels
{
    public class BlogItemViewModel : ContentViewModel<BlogItemPage>
    {
        public BlogItemViewModel(BlogItemPage currentPage) : base(currentPage) { }

        public IEnumerable<TagItem> Tags { get; set; }
        public string PreviewText { get; set; }
        public DateTime StartPublish { get; set; }
        public XhtmlString MainBody { get; set; }
        public bool ShowPublishDate { get; set; }
        public bool ShowIntroduction { get; set; }
        public string Template { get; set; }
        public string PreviewOption { get; set; }
        public CategoryList Category { get; set; }
        public List<KeyValuePair<string, string>> BreadCrumbs { get; set; }

        public class TagItem
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public int Weight { get; set; }
            public int Count { get; set; }
            public string DisplayName { get; set; }
        }

    }
}
