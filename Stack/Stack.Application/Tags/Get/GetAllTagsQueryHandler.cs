﻿using Microsoft.Extensions.Logging;
using Stack.Application.Abstractions;
using Stack.Application.Abstractions.Queries;
using Stack.Application.Dtos;
using Stack.Application.Tags.ForceRedownload;
using Stack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Stack.Application.Tags.Get
{
    public sealed class GetAllTagsQueryHandler : IQueryHandler<GetAllTagsQuery, PagedResult<TagDto>>
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<GetAllTagsQueryHandler> _logger;

        public GetAllTagsQueryHandler(ITagRepository tagRepository,ILogger<GetAllTagsQueryHandler> logger)
        {
            _tagRepository = tagRepository;
            _logger = logger;
        }

        public async Task<PagedResult<TagDto>> HandleAsync(GetAllTagsQuery query, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Start processing {nameof(GetAllTagsQuery)}");
            var tags = _tagRepository.GetAllTags();

            _logger.LogInformation("Successfully gathered from database.");

            if (query.SortOrder == SortOrder.desc)
            {
                _logger.LogInformation("Tags will be sorted in desc way.");
                tags = tags.OrderByDescending(GetSortProperty(query));
            }
            else
            {
                _logger.LogInformation("Tags will be sorted in asc way.");
                tags = tags.OrderBy(GetSortProperty(query));
            }

            var tagsDtos = tags.Select(t => new TagDto(
                t.Count,
                t.HasSynonyms,
                t.IsModeratorOnly,
                t.IsRequired,
                t.Name,
                t.UserId));

            var pagedTags = await PagedResult<TagDto>.CreateAsync(tagsDtos,query.Page,query.PageSize);
            return pagedTags;
        }

        private static Expression<Func<Tag,Object>> GetSortProperty(GetAllTagsQuery query)
        {
            Expression<Func<Tag, object>> keySelector = query.SortColumn switch
            {
                SortProperty.Name => Tag => Tag.Name,
                SortProperty.Count => Tag => Tag.Count
            };
            return keySelector;
        }
    }
}
