using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using Foundation.Cms;
using Foundation.Cms.Categories;
using Foundation.Cms.ViewModels;
using Foundation.Cms.ViewModels.Blocks;
using Geta.EpiCategories;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Foundation.Features.Blog.TagCloudBlock
{
    [TemplateDescriptor(Default = true)]
    public class TagCloudBlockController : BlockController<Cms.Blocks.TagCloudBlock>
    {
        private readonly IContentLoader _contentLoader;
        private readonly BlogTagFactory _blogTagFactory;
        private readonly ICategoryContentLoader _categoryContentLoader;
        private readonly TagCloudBlockControllerService _controllerService;

        public TagCloudBlockController(IContentLoader contentLoader,
            CategoryRepository categoryRepository,
            BlogTagRepository blogTagRepository,
            BlogTagFactory blogTagFactory,
            ICategoryContentLoader categoryContentLoader,
            TagCloudBlockControllerService controllerService)
        {
            _contentLoader = contentLoader;
            _blogTagFactory = blogTagFactory;
            _categoryContentLoader = categoryContentLoader;
            _controllerService = controllerService ?? throw new ArgumentNullException(nameof(controllerService));
        }

        public override ActionResult Index(Cms.Blocks.TagCloudBlock currentBlock)
        {
            var model = new TagCloudBlockModel(currentBlock)
            {
                Tags = _controllerService.GetTags(currentBlock.BlogTagLinkPage)
            };

            return PartialView(model);
        }
    }
}
