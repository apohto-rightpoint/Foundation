using System;
using System.Collections.Specialized;
using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Framework;
using EPiServer.Personalization;
using Foundation.Find.Cms;
using Foundation.Find.Cms.Locations;
using Foundation.Find.Cms.Locations.ViewModels;

namespace Foundation.Features.Locations.LocationListPage
{
    public class LocationListPageControllerService
    {
        private readonly IContentLoader _contentLoader;

        public LocationListPageControllerService(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        }

        public LocationListViewModel GetViewModel(Find.Cms.Models.Pages.LocationListPage currentPage, NameValueCollection queryString)
        {
            var query = SearchClient.Instance.Search<Find.Cms.Models.Pages.LocationItemPage>()
                .PublishedInCurrentLanguage()
                .FilterOnReadAccess();

            if (currentPage.FilterArea != null)
            {
                foreach (var filterBlock in currentPage.FilterArea.FilteredItems)
                {
                    var b = _contentLoader.Get<BlockData>(filterBlock.ContentLink) as IFilterBlock;
                    if (b != null)
                    {
                        query = b.AddFilter(query);
                    }
                }

                foreach (var filterBlock in currentPage.FilterArea.FilteredItems)
                {
                    var b = _contentLoader.Get<BlockData>(filterBlock.ContentLink) as IFilterBlock;
                    if (b != null)
                    {
                        query = b.ApplyFilter(query, queryString);
                    }
                }
            }

            var locations = query.OrderBy(x => x.PageName)
                .Take(500)
                .StaticallyCacheFor(new System.TimeSpan(0, 1, 0)).GetContentResult();

            var model = new LocationListViewModel(currentPage)
            {
                Locations = locations,
                MapCenter = GetMapCenter(),
                UserLocation = GeoPosition.GetUsersLocation(),
                QueryString = queryString
            };

            return model;
        }

        private static GeoCoordinate GetMapCenter()
        {
            var userLocation = GeoPosition.GetUsersPosition();
            if (userLocation != null)
            {
                return new GeoCoordinate(30, userLocation.Longitude);
            }
            return new GeoCoordinate(30, 0);
        }
    }
}