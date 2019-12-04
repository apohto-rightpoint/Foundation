using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EPiServer;
using EPiServer.Cms.Shell;
using EPiServer.Core.Html;
using EPiServer.Web.Routing;
using Foundation.Cms;
using Foundation.Cms.Categories;
using Foundation.Cms.Pages;
using Foundation.Cms.ViewModels;

namespace Foundation.Features.Blog.BlogItem
{
    public class BlogItemControllerService
    {
        private readonly IContentLoader _contentLoader;
        private readonly BlogTagFactory _blogTagFactory;
        private readonly UrlResolver _urlResolver;

        public BlogItemControllerService(IContentLoader contentLoader, BlogTagFactory blogTagFactory, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _blogTagFactory = blogTagFactory ?? throw new ArgumentNullException(nameof(blogTagFactory));
            _urlResolver = urlResolver ?? throw new ArgumentNullException(nameof(urlResolver));
        }

        public BlogItemViewModel GetViewModel(BlogItemPage currentPage, int previewTextLength)
        {
            var model = new BlogItemViewModel(currentPage)
            {
                Category = currentPage.Category,
                Tags = GetTags(currentPage),
                PreviewText = GetPreviewText(currentPage, previewTextLength),
                MainBody = currentPage.MainBody,
                StartPublish = currentPage.StartPublish ?? DateTime.UtcNow,
                BreadCrumbs = GetBreadCrumb(currentPage)
            };

            return model;
        }

        public IEnumerable<BlogItemViewModel.TagItem> GetTags(BlogItemPage currentPage)
        {
            if (currentPage.Categories != null)
            {
                var allCategories = _contentLoader.GetItems(currentPage.Categories, CultureInfo.CurrentUICulture);
                return allCategories.
                    Select(cat => new BlogItemViewModel.TagItem()
                    {
                        Title = cat.Name,
                        Url = _blogTagFactory.GetTagUrl(currentPage, cat.ContentLink),
                        DisplayName = (cat as StandardCategory)?.Description,
                    }).ToList();
            }
            return new List<BlogItemViewModel.TagItem>();
        }


        private string GetPreviewText(BlogItemPage page, int previewTextLength)
        {
            if (previewTextLength <= 0)
            {
                return string.Empty;
            }

            var previewText = string.Empty;

            if (page.MainBody != null)
            {
                previewText = page.MainBody.ToHtmlString();
            }

            if (string.IsNullOrEmpty(previewText))
            {
                return string.Empty;
            }

            var regexPattern = new StringBuilder(@"<span[\s\W\w]*?classid=""");
            //regexPattern.Append(DynamicContentFactory.Instance.DynamicContentId.ToString());
            regexPattern.Append(@"""[\s\W\w]*?</span>");
            previewText = Regex.Replace(previewText, regexPattern.ToString(), string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return TextIndexer.StripHtml(previewText, previewTextLength);
        }

        private List<KeyValuePair<string, string>> GetBreadCrumb(BlogItemPage currentPage)
        {
            var ancestors = _contentLoader.GetAncestors(currentPage.ContentLink)
                .Select(x => x as Cms.Pages.BlogListPage)
                .Where(x => x != null);
            var breadCrumb = ancestors.Reverse().Select(x => new KeyValuePair<string, string>(x.MetaTitle, x.PublicUrl(_urlResolver))).ToList();

            return breadCrumb;
        }
    }
}