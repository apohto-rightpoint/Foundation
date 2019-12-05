using EPiServer;
using EPiServer.Core;
using Foundation.Cms;
using Foundation.Cms.Categories;
using Foundation.Cms.ViewModels;
using Geta.EpiCategories;
using System;
using System.Collections.Generic;

namespace Foundation.Features.Blog.TagCloudBlock
{
    public class TagCloudBlockControllerService
    {
        private readonly ICategoryContentLoader _categoryContentLoader;
        private readonly IContentLoader _contentLoader;
        private readonly BlogTagFactory _blogTagFactory;

        public TagCloudBlockControllerService(ICategoryContentLoader categoryContentLoader,
            BlogTagFactory blogTagFactory,
            IContentLoader contentLoader)
        {
            _categoryContentLoader = categoryContentLoader ?? throw new ArgumentNullException(nameof(categoryContentLoader));
            _blogTagFactory = blogTagFactory ?? throw new ArgumentNullException(nameof(blogTagFactory));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public IEnumerable<BlogItemViewModel.TagItem> GetTags(ContentReference startTagLink)
        {
            var tags = new List<BlogItemViewModel.TagItem>();
            foreach (var item in BlogTagRepository.Instance.LoadTags())
            {
                var cat = _categoryContentLoader.GetFirstBySegment<StandardCategory>(item.TagName); // Assumes tag name == url segment
                var url = string.Empty;

                if (startTagLink != null)
                {
                    url = _blogTagFactory.GetTagUrl(_contentLoader.Get<PageData>(startTagLink.ToPageReference()), cat.ContentLink);
                }

                tags.Add(new BlogItemViewModel.TagItem() { Count = item.Count, Title = item.DisplayName, Weight = item.Weight, Url = url });
            }
            return tags;
        }
    }
}