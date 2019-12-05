using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using Foundation.Find.Cms.Locations.ViewModels;
using System;
using System.Linq;

namespace Foundation.Features.Locations.LocationItemPage
{
    public class LocationItemPageControllerService
    {
        private readonly IContentRepository _contentRepository;

        public LocationItemPageControllerService(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
        }

        public LocationViewModel GetViewModel(Find.Cms.Models.Pages.LocationItemPage currentPage)
        {
            var model = new LocationViewModel(currentPage);
            if (!ContentReference.IsNullOrEmpty(currentPage.Image))
            {
                model.Image = _contentRepository.Get<ImageData>(currentPage.Image);
            }

            model.LocationNavigation.ContinentLocations = SearchClient.Instance
                .Search<Find.Cms.Models.Pages.LocationItemPage>()
                .Filter(x => x.Continent.Match(currentPage.Continent))
                .PublishedInCurrentLanguage()
                .OrderBy(x => x.PageName)
                .FilterForVisitor()
                .Take(100)
                .StaticallyCacheFor(new System.TimeSpan(0, 10, 0))
                .GetContentResult();

            model.LocationNavigation.CloseBy = SearchClient.Instance
                .Search<Find.Cms.Models.Pages.LocationItemPage>()
                .Filter(x => x.Continent.Match(currentPage.Continent)
                             & !x.PageLink.Match(currentPage.PageLink))
                .PublishedInCurrentLanguage()
                .FilterForVisitor()
                .OrderBy(x => x.Coordinates)
                .DistanceFrom(currentPage.Coordinates)
                .Take(5)
                .StaticallyCacheFor(new System.TimeSpan(0, 10, 0))
                .GetContentResult();

            return model;
        }
    }
}