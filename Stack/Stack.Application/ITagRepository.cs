﻿using Stack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stack.Application;

public interface ITagRepository
{
    Task<bool> AnyAsync();
    Task<bool> BulkInsertToDbAsync(List<Tag> tags);
    Task<List<Tag>> GetAllTags();
}
