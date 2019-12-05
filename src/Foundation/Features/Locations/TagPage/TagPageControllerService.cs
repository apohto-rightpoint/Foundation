using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using Foundation.Cms.Media;
using Foundation.Find.Cms.Locations.ViewModels;

namespace Foundation.Features.Locations.TagPage
{
    public class TagPageControllerService
    {
        private readonly IContentLoader _contentLoader;

        public TagPageControllerService(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public TagsViewModel GetViewModel(Cms.Pages.TagPage currentPage, string continent, string addcat)
        {
            var model = new TagsViewModel(currentPage)
            {
                Continent = continent
            };
            
            if (addcat != null)
            {
                model.AdditionalCategories = addcat.Split(',');
            }

            var query = SearchClient.Instance.Search<Find.Cms.Models.Pages.LocationItemPage>()
                .Filter(f => f.TagString().Match(currentPage.Name));
            if (model.AdditionalCategories != null)
            {
                query = model.AdditionalCategories.Aggregate(query, (current, c) => current.Filter(f => f.TagString().Match(c)));
            }
            if (model.Continent != null)
            {
                query = query.Filter(dp => dp.Continent.MatchCaseInsensitive(model.Continent));
            }
            model.Locations = query.StaticallyCacheFor(new System.TimeSpan(0, 1, 0)).GetContentResult().ToList();

            //Add theme images from results
            var carousel = new TagsCarouselViewModel
            {
                Items = new List<TagsCarouselItem>()
            };
            foreach (var location in model.Locations)
            {
                if (location.Image != null)
                {
                    carousel.Items.Add(new TagsCarouselItem
                    {
                        Image = location.Image,
                        Heading = location.Name,
                        Description = location.MainIntro,
                        ItemURL = location.ContentLink
                    });
                }
            }
            if (carousel.Items.All(item => item.Image == null) || currentPage.Images != null)
            {
                foreach (var image in currentPage.Images.FilteredItems.Select(ci => ci.ContentLink))
                {
                    var title = _contentLoader.Get<ImageMediaData>(image).Title;
                    carousel.Items.Add(new TagsCarouselItem { Image = image, Heading = title });
                }
            }
            model.Carousel = carousel;

            return model;
        }
    }
}