/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Core;
using ASC.Core.Common.EF;
using ASC.Core.Common.EF.Context;
using ASC.Projects.EF;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Core.Common.Settings;
using ASC.Projects.Core.Domain.Reports;
using ASC.Common;

namespace ASC.Projects.Data.DAO
{
    /*
    internal class CachedMessageDao : MessageDao
    {
        private readonly HttpRequestDictionary<DbMessage> messageCache = new HttpRequestDictionary<DbMessage>("message");

        public CachedMessageDao(int tenant) : base(tenant)
        {
        }

        public override DbMessage GetById(int id)
        {
            return messageCache.Get(id.ToString(CultureInfo.InvariantCulture), () => GetBaseById(id));
        }

        private DbMessage GetBaseById(int id)
        {
            return base.GetById(id);
        }

        public override void Save(Message msg)
        {
            if (msg != null)
            {
                ResetCache(msg.Id);
            }
            base.Save(msg);
        }

        public override void Delete(int id)
        {
            ResetCache(id);
            base.Delete(id);
        }

        private void ResetCache(int messageId)
        {
            messageCache.Reset(messageId.ToString(CultureInfo.InvariantCulture));
        }
    }
    */
    [Scope]
    public class MessageDao : BaseDao, IMessageDao
    {
        private TenantUtil TenantUtil { get; set; }
        private FactoryIndexer<DbMessage> FactoryIndexer { get; set; }
        private SettingsManager SettingsManager { get; set; }
        private NotifySource NotifySource { get; set; }
        private FilterHelper FilerHelper { get; set; }
        private IDaoFactory DaoFactory { get; set; }


        public MessageDao(SecurityContext securityContext, DbContextManager<WebProjectsContext> dbContextManager, TenantUtil tenantUtil, FactoryIndexer<DbMessage> factoryIndexer, IDaoFactory daoFactory, SettingsManager settingsManager, TenantManager tenantManager, NotifySource notifySource)
            : base(securityContext, dbContextManager, tenantManager)
        {
            TenantUtil = tenantUtil;
            FactoryIndexer = factoryIndexer;
            SettingsManager = settingsManager;
            DaoFactory = daoFactory;
            NotifySource = notifySource;
        }

        public List<Message> GetAll()
        {
            return CreateQuery()
                .Select(r => ToMessage(r.Message, r.Project))
                .ToList();
        }

        public List<Message> GetByProject(int projectId)
        {
            return CreateQuery()
                .Where(q=> q.Message.ProjectId == projectId)
                .AsEnumerable()
                .GroupBy(q=> new {q.Message, q.Project })
                .ToList()
                .ConvertAll(q => ToMessage(q.Key.Message, q.Key.Project));
        }

        public List<Message> GetMessages(int startIndex, int max)
        {
            return CreateQuery()
                .OrderBy(q=> q.Message.CreateOn)
                .Skip(startIndex)
                .Take(max)
                .ToList()
                .ConvertAll(r => ToMessage(r.Message, r.Project));
        }

        public List<Message> GetRecentMessages(int offset, int max, params int[] projects)
        {
           return CreateQuery()
                .Where(q=> projects == null || projects.Length == 0 || projects.Contains(q.Message.ProjectId))
                .Select(r => ToMessage(r.Message, r.Project))
                .Skip(offset)
                .OrderBy(r=> r.CreateOn)
                .Take(max)
                .ToList();
        }

        public List<Message> GetByFilter(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = CreateQuery();

            if (filter.Max > 0 && filter.Max < 150000)
            {
                query = query.Skip((int)filter.Offset);
                query = query.Take((int)filter.Max);
            }

            //query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var sortQuery = query.OrderBy(r => r.Message.Status);

            if (!string.IsNullOrEmpty(filter.SortBy) || true)
            {
               // var sortColumns = filter.SortColumns["Message"];
            //    sortColumns.Remove(filter.SortBy);

                sortQuery = SortBy("title", false, query);

               // foreach (var sort in sortColumns.Keys)
               // {
                    //query = query.OrderBy(GetSortFilter(sort), sortColumns[sort]);
              //  }
            }

            return sortQuery
                //.AsEnumerable()
                //.GroupBy(q => new { q.Message, q.Project })
                .ToList()
                .ConvertAll(q => ToMessage(q.Message, q.Project));
        }
        
        private IOrderedQueryable<QueryMessages> SortBy(string sortBy,bool sortOrder, IQueryable<QueryMessages> query)
        {
            switch (sortBy)
            {
                case "title":
                    return query.OrderBy(q=> q.Message.Title);
            }
            return null;
        }

        private IOrderedQueryable<QueryMessages> SortBy(string sortBy, bool sortOrder, IOrderedQueryable<QueryMessages> query)
        {
            switch (sortBy)
            {
                case "title":
                    return query.ThenBy(q => q.Message.Title);
            }
            return null;
        }

        public int GetByFilterCount(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            var query = WebProjectsContext.Message.Join(
                WebProjectsContext.Project,
                m => m.ProjectId,
                p => p.Id,
                (m, p) => new QueryMessages()
                {
                    Message = m,
                    Project = p
                })
                .Where(q => q.Message.TenantId == q.Project.TenantId && q.Message.TenantId == Tenant);
            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);


            return query
                .AsEnumerable()
                .GroupBy(q => new { q.Message})
                .Count();
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            filter = (TaskFilter)filter.Clone();

            var query = WebProjectsContext.Message.Join(WebProjectsContext.Project,
                m => m.ProjectId,
                p => p.Id,
                (m, p) => new QueryMessages() { Project = p, Message = m })
                .Where(q => q.Message.TenantId == q.Project.TenantId && q.Message.TenantId == Tenant && q.Message.CreateOn >= FilerHelper.GetFromDate(filter) && q.Message.CreateOn <= FilerHelper.GetToDate(filter));

            if (filter.HasUserId)
            {
                query = query.Where(q=> FilerHelper.GetUserIds(filter).Contains(q.Message.CreateBy));
                filter.UserId = Guid.Empty;
                filter.DepartmentId = Guid.Empty;
            }

            query = CreateQueryFilter(query, filter, isAdmin, checkAccess);

            var queryCount = query.GroupBy(q => new { q.Message.CreateBy, q.Message.ProjectId })
                .Select(q => new Tuple<Guid, int, int>(ToGuid(q.Key.CreateBy), q.Key.ProjectId, q.Count())).ToList();
            return queryCount;
        }

        public virtual Message GetById(int id)
        {
            var query = CreateQuery()
                .Where(q => q.Message.Id == id)
                .AsEnumerable()
                .GroupBy(q => new { q.Message, q.Project })
                .SingleOrDefault();
            return ToMessage(query.Key.Message, query.Key.Project);
        }

        public bool IsExists(int id)
        {
            var count = WebProjectsContext.Message
                .Where(m => m.Id == id)
                .Count();
            return 0 < count;
        }

        public virtual Message SaveOrUpdate(Message msg)
        {
            msg.CreateOn = TenantUtil.DateTimeToUtc(msg.CreateOn);
            msg.LastModifiedOn = TenantUtil.DateTimeToUtc(msg.LastModifiedOn);
            if (WebProjectsContext.Message.Where(m => m.Id == msg.ID).Any())
            {
                var db = WebProjectsContext.Message.Where(m => m.Id == msg.ID).SingleOrDefault();
                db.Title = msg.Title;
                db.Status = (int)msg.Status;
                db.CreateBy = msg.CreateBy.ToString();
                db.CreateOn = TenantUtil.DateTimeFromUtc(msg.CreateOn);
                db.LastModifiedBy = msg.LastModifiedBy.ToString();
                db.LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(msg.LastModifiedOn));
                db.Content = msg.Description;
                db.TenantId = Tenant;
                db.ProjectId = msg.Project.ID;
                WebProjectsContext.Message.Update(db);
                WebProjectsContext.SaveChanges();
                return ToMessage(db);
            }
            else
            {
                var db = ToDbMessage(msg);
                WebProjectsContext.Message.Add(db);
                WebProjectsContext.SaveChanges();
                return ToMessage(db);
            }
        }

        public virtual Message Delete(int id)
        {
            var message = WebProjectsContext.Message.Where(m => m.Id == id).SingleOrDefault();
            WebProjectsContext.Message.Remove(message);
            var comments = WebProjectsContext.Comment.Where(c => c.TargetUniqId == ProjectEntity.BuildUniqId<DbMessage>(id));
            WebProjectsContext.Comment.RemoveRange(comments);
            WebProjectsContext.SaveChanges();
            return ToMessage(message);
        }


        private IQueryable<QueryMessages> CreateQuery()
        {

            return WebProjectsContext.Project.Join(WebProjectsContext.Message,
               p => p.Id,
               m => m.ProjectId,
               (p, m) => new
               {
                   Project = p,
                   Message = m
               })
               .Where(r => r.Project.TenantId == Tenant && r.Message.TenantId == Tenant)
               .Select(q=> new QueryMessages() 
                   {
                    Message = q.Message,
                    Project = q.Project,
                    CountComments = WebProjectsContext.Comment.Where(c=> "Message_" + q.Message.Id == c.TargetUniqId && c.TenantId == Tenant).Count()
                   });
        }

        private IQueryable<QueryMessages> CreateQueryFilter(IQueryable<QueryMessages> query, TaskFilter filter, bool isAdmin, bool checkAccess)
        {
            if (filter.Follow)
            {
                IEnumerable<string> objects = NotifySource.GetSubscriptionProvider().GetSubscriptions(NotifyConstants.Event_NewCommentForMessage, NotifySource.GetRecipientsProvider().GetRecipient(CurrentUserID.ToString()));

                if (filter.ProjectIds.Count != 0)
                {
                    objects = objects.Where(r => r.Split('_')[2] == filter.ProjectIds[0].ToString(CultureInfo.InvariantCulture));
                }

                var ids = objects.Select(r => r.Split('_')[1]).ToArray();
                query = query.Where(r=> ids.Contains(r.Message.Id.ToString()));
                var q = query.ToList();
            }

            if (filter.ProjectIds.Count != 0)
            {
                query = query.Where(r=> filter.ProjectIds.Contains(r.Message.ProjectId));
            }
            else
            {
                if (SettingsManager.Load<ProjectsCommonSettings>().HideEntitiesInPausedProjects)
                {
                    query = query.Where(r=> (ProjectStatus)r.Project.Status != ProjectStatus.Paused);
                }

                if (filter.MyProjects)
                {
                    query = query.Join(WebProjectsContext.Participant,
                        q => q.Project.Id,
                        ppp => ppp.ProjectId,
                        (q, ppp) => new
                        {
                            Message = q.Message,
                            Project = q.Project,
                            CountComments = q.CountComments,
                            Participant = ppp
                        })
                        .Where(q => q.Participant.Tenant == q.Message.TenantId && q.Participant.Removed == 0 && ToGuid(q.Participant.ParticipantId) == CurrentUserID)
                        .Select(q=> new QueryMessages() { Project = q.Project, Message = q.Message, CountComments = q.CountComments });
                }
            }

            if (filter.TagId != 0)
            {
                if (filter.TagId == -1)
                {
                    query = query.Join(WebProjectsContext.TagToProject,
                        q => q.Message.ProjectId,
                        tp => tp.ProjectId,
                        (q, tp) => new 
                        {
                            Message = q.Message,
                            Project = q.Project,
                            CountComments = q.CountComments,
                            TagToProject = tp
                        })
                        .Where(q=> q.TagToProject.TagId == null)
                        .Select(q => new QueryMessages() { Project = q.Project, Message = q.Message, CountComments = q.CountComments });
                }
                else
                {
                    query = query.Join(WebProjectsContext.TagToProject,
                        q => q.Message.ProjectId,
                        tp => tp.ProjectId,
                        (q, tp) => new
                        {
                            Message = q.Message,
                            Project = q.Project,
                            CountComments = q.CountComments,
                            TagToProject = tp
                        })
                        .Where(q => q.TagToProject.TagId == filter.TagId)
                        .Select(q => new QueryMessages() { Project = q.Project, Message = q.Message, CountComments = q.CountComments });
                }
            }

            if (filter.UserId != Guid.Empty)
            {
                query = query.Where(q=> ToGuid(q.Message.CreateBy) == filter.UserId);
            }

            if (filter.DepartmentId != Guid.Empty)
            {
                query = query.Join(WebProjectsContext.UserGroup,
                    q => ToGuid(q.Message.CreateBy),
                    u => u.GroupId,
                    (q, u) => new
                    {
                        Message = q.Message,
                        Project = q.Project,
                        CountComments = q.CountComments,
                        UserGroup = u
                    })
                    .Where(q=> q.UserGroup.GroupId == filter.DepartmentId && q.UserGroup.Tenant == q.Message.TenantId && q.UserGroup.Removed == false)
                    .Select(q => new QueryMessages() { Project = q.Project, Message = q.Message, CountComments = q.CountComments });
            }

            if (!filter.FromDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MinValue) && !filter.ToDate.Equals(DateTime.MaxValue))
            {
                query = query.Where(q=> q.Message.CreateOn >= filter.FromDate && q.Message.CreateOn <= filter.ToDate.AddDays(1));
            }

            if (filter.MessageStatus.HasValue)
            {
                query = query.Where(q=> (MessageStatus)q.Message.Status == filter.MessageStatus.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                List<int> mIds;
                if (FactoryIndexer.TrySelectIds(s => s.MatchAll(filter.SearchText), out mIds))
                {
                   query = query.Where(q=>mIds.Contains(q.Message.Id));
                }
                else
                {
                    query = query.Where(q=> q.Message.Title.Contains(filter.SearchText));
                }
            }

            if (checkAccess)
            {
                query = query.Where(q=> q.Project.Private == 0);
            }
            else if (!isAdmin)
            {
                var isInTeam = WebProjectsContext.Participant.Any(p => p.ProjectId.ToString() == p.ParticipantId && p.Removed == 0 && ToGuid(p.ParticipantId) == CurrentUserID);
                    query.Where(q => q.Project.Private == 0 || q.Project.ResponsibleId == CurrentUserID.ToString() || q.Project.Private == 1 && isInTeam);
            }

            return query;
        }

        /*private string GetSortFilter(string sortBy)
        {
            if (sortBy != "comments") return "t." + sortBy;

            return "comments";

        }*/

        private Message ToMessage(DbMessage message, DbProject project = null)
        {
            return new Message
            {
                Project = project != null ? DaoFactory.GetProjectDao().ToProject(project) : null,
                ID = message.Id,
                Title = message.Title,
                Status = (MessageStatus)message.Status,
                CreateBy = ToGuid(message.CreateBy),
                CreateOn = TenantUtil.DateTimeFromUtc(message.CreateOn),
                LastModifiedBy = ToGuid(message.LastModifiedBy),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(message.LastModifiedOn)),
                Description = message.Content
            };
        }

        public DbMessage ToDbMessage(Message message)
        {
            return new DbMessage
            {
                Id = message.ID,
                Title = message.Title,
                Status = (int)message.Status,
                CreateBy = message.CreateBy.ToString(),
                CreateOn = TenantUtil.DateTimeFromUtc(message.CreateOn),
                LastModifiedBy = message.LastModifiedBy.ToString(),
                LastModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(message.LastModifiedOn)),
                Content = message.Description,
                TenantId = Tenant,
                ProjectId = message.Project.ID
            };
        }
    }

    internal class QueryMessages
    {
        public DbProject Project { get; set; }
        public DbMessage Message { get; set; }
        public int CountComments { get; set; }
        
        public QueryMessages()
        {

        }
    }
}
