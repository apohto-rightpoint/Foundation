using System;
using EPiServer.Core;
using Foundation.Social.Models.Comments;
using Foundation.Social.Repositories.Comments;
using Foundation.Social.Repositories.Common;
using Foundation.Social.ViewModels;

namespace Foundation.Features.Blog.BlogCommentBlock
{
    public class BlogCommentBlockControllerService
    {
        private readonly IBlogCommentRepository _commentRepository;
        private readonly IPageRepository _pageRepository;

        public BlogCommentBlockControllerService(IBlogCommentRepository commentRepository, IPageRepository pageRepository)
        {
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
            _pageRepository = pageRepository ?? throw new ArgumentNullException(nameof(pageRepository));
        }

        public BlogCommentsBlockViewModel GetViewModel(PagingInfo pagingInfo)
        {
            var pageId = pagingInfo.PageId;
            var pageIndex = pagingInfo.PageNumber;
            var pageSize = pagingInfo.PageSize;

            var pageReference = new PageReference(pageId);
            var pageContentGuid = _pageRepository.GetPageId(pageReference);

            // Create a comments block view model to fill the frontend block view
            var blockViewModel = new BlogCommentsBlockViewModel(pageReference);

            // Try to get recent comments
            try
            {
                var blogComments = _commentRepository.Get(
                    new PageCommentFilter
                    {
                        Target = pageContentGuid.ToString(),
                        PageSize = pageSize,
                        PageOffset = pageIndex - 1
                    },
                    out var totalComments
                );

                blockViewModel.Comments = blogComments;
                blockViewModel.PagingInfo = pagingInfo;
                blockViewModel.PagingInfo.TotalRecord = (int)totalComments;
            }
            catch (Exception ex)
            {
                blockViewModel.Messages.Add(ex.Message);
            }

            return blockViewModel;
        }
    }
}