/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Core;
using ASC.Api.CRM;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.CRM.ApiModels;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;
using ASC.MessagingSystem;
using ASC.Web.Api.Models;
using ASC.Web.Api.Routing;
using ASC.Web.Core.Users;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Services.NotifyService;

using Microsoft.AspNetCore.Mvc;

namespace ASC.CRM.Api
{
    public class DealsController : BaseApiController
    {
        public DealsController(CRMSecurity cRMSecurity,
                   DaoFactory daoFactory,
                   ApiContext apiContext,
                   MessageTarget messageTarget,
                   MessageService messageService,
                   NotifyClient notifyClient,
                   ContactDtoHelper  contactDtoHelper,
                   OpportunityDtoHelper opportunityDtoHelper,
                   CurrencyProvider currencyProvider,
                   CurrencyInfoDtoHelper currencyInfoDtoHelper,
                   UserManager userManager,
                   EmployeeWraperHelper employeeWraperHelper,
                   DisplayUserSettingsHelper displayUserSettingsHelper)
            : base(daoFactory, cRMSecurity)
        {
            ApiContext = apiContext;
            MessageTarget = messageTarget;
            MessageService = messageService;
            NotifyClient = notifyClient;
            OpportunityDtoHelper = opportunityDtoHelper;
            CurrencyProvider = currencyProvider;
            CurrencyInfoDtoHelper = currencyInfoDtoHelper;
            UserManager = userManager;
            EmployeeWraperHelper = employeeWraperHelper;
            DisplayUserSettingsHelper = displayUserSettingsHelper;
            ContactDtoHelper = contactDtoHelper;
        }

        public ContactDtoHelper ContactDtoHelper { get; }
        public DisplayUserSettingsHelper DisplayUserSettingsHelper { get; }
        public NotifyClient NotifyClient { get; }
        private ApiContext ApiContext { get; }
        public MessageService MessageService { get; }
        public MessageTarget MessageTarget { get; }
        public OpportunityDtoHelper OpportunityDtoHelper { get; }
        public SecurityContext SecurityContext { get; }
        public CurrencyProvider CurrencyProvider { get; }
        public CurrencyInfoDtoHelper CurrencyInfoDtoHelper { get; }
        public UserManager UserManager { get; }
        public EmployeeWraperHelper EmployeeWraperHelper { get; }

        /// <summary>
        ///    Returns the detailed information about the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <returns>
        ///    Opportunity
        /// </returns>
        /// <short>Get opportunity by ID</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"opportunity/{opportunityid:int}")]
        public OpportunityDto GetDealByID(int opportunityid)
        {
            if (opportunityid <= 0) throw new ArgumentException();

            var deal = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (deal == null || !CRMSecurity.CanAccessTo(deal)) throw new ItemNotFoundException();

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///    Updates the selected opportunity to the stage with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="stageid">Opportunity stage ID</param>
        /// <returns>
        ///    Opportunity
        /// </returns>
        /// <short>Update opportunity stage</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"opportunity/{opportunityid:int}/stage/{id:int}")]
        public OpportunityDto UpdateToDealMilestone(int opportunityid, int stageid)
        {
            if (opportunityid <= 0 || stageid <= 0) throw new ArgumentException();

            var deal = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (deal == null || !CRMSecurity.CanEdit(deal)) throw new ItemNotFoundException();

            var stage = DaoFactory.GetDealMilestoneDao().GetByID(stageid);
            if (stage == null) throw new ItemNotFoundException();

            deal.DealMilestoneID = stageid;
            deal.DealMilestoneProbability = stage.Probability;

            deal.ActualCloseDate = stage.Status != DealMilestoneStatus.Open ? DateTime.UtcNow : DateTime.MinValue;
            DaoFactory.GetDealDao().EditDeal(deal);
            MessageService.Send(MessageAction.OpportunityUpdatedStage, MessageTarget.Create(deal.ID), deal.Title);

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///   Sets access rights for the selected opportunity with the parameters specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="isPrivate">Opportunity privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set rights to opportunity</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Opportunity 
        /// </returns>
        [Update(@"opportunity/{opportunityid:int}/access")]
        public OpportunityDto SetAccessToDeal(int opportunityid, bool isPrivate, IEnumerable<Guid> accessList)
        {
            if (opportunityid <= 0) throw new ArgumentException();

            var deal = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (deal == null) throw new ItemNotFoundException();

            if (!(CRMSecurity.IsAdmin || deal.CreateBy == SecurityContext.CurrentAccount.ID)) throw CRMSecurity.CreateSecurityException();
            return SetAccessToDeal(deal, isPrivate, accessList, false, true);
        }

        private OpportunityDto SetAccessToDeal(Deal deal, bool isPrivate, IEnumerable<Guid> accessList, bool isNotify, bool isMessageServicSende)
        {
            var accessListLocal = accessList != null ? accessList.Distinct().ToList() : new List<Guid>();
            if (isPrivate && accessListLocal.Count > 0)
            {

                if (isNotify)
                {
                    accessListLocal = accessListLocal.Where(u => u != SecurityContext.CurrentAccount.ID).ToList();
                    NotifyClient.SendAboutSetAccess(EntityType.Opportunity, deal.ID, DaoFactory, accessListLocal.ToArray());
                }

                if (!accessListLocal.Contains(SecurityContext.CurrentAccount.ID))
                {
                    accessListLocal.Add(SecurityContext.CurrentAccount.ID);
                }

                CRMSecurity.SetAccessTo(deal, accessListLocal);

                if (isMessageServicSende)
                {
                    var users = UserManager.GetUsers().Where(x => accessListLocal.Contains(x.ID));
                    MessageService.Send(MessageAction.OpportunityRestrictedAccess, MessageTarget.Create(deal.ID), deal.Title, users.Select(x => x.DisplayUserName(false, DisplayUserSettingsHelper)));
                }
            }
            else
            {
                CRMSecurity.MakePublic(deal);
                if (isMessageServicSende)
                {
                    MessageService.Send(MessageAction.OpportunityOpenedAccess, MessageTarget.Create(deal.ID), deal.Title);
                }
            }

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of all opportunities matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="responsibleid">Opportunity responsible</param>
        /// <param optional="true" name="opportunityStagesid">Opportunity stage ID</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="contactAlsoIsParticipant">Participation status: take into account opportunities where the contact is a participant or not</param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param optional="true" name="stageType" remark="Allowed values: {Open, ClosedAndWon, ClosedAndLost}">Opportunity stage type</param>
        /// <param name="isPrivate">Opportunity privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set opportunity access rights</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Update(@"opportunity/filter/access")]
        public IEnumerable<OpportunityDto> SetAccessToBatchDeal([FromForm] SetAccessToBatchDealByFilterRequestDto inDto)
        {
            var responsibleid = inDto.Responsibleid;
            var opportunityStagesid = inDto.OpportunityStagesid;
            var contactid = inDto.Contactid;
            var tags = inDto.Tags;
            var stageType = inDto.StageType;
            var contactAlsoIsParticipant = inDto.ContactAlsoIsParticipant;
            var fromDate = inDto.FromDate;
            var toDate = inDto.ToDate;
            var isPrivate = inDto.isPrivate;
            var accessList = inDto.AccessList;

            var result = new List<Deal>();

            var deals = DaoFactory.GetDealDao()
                                  .GetDeals(ApiContext.FilterValue,
                                            responsibleid,
                                            opportunityStagesid,
                                            tags,
                                            contactid,
                                            stageType,
                                            contactAlsoIsParticipant,
                                            fromDate, toDate, 0, 0, null);

            if (!deals.Any()) return Enumerable.Empty<OpportunityDto>();

            foreach (var deal in deals)
            {
                if (deal == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || deal.CreateBy == SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToDeal(deal.ID, isPrivate, accessList);

                result.Add(deal);

            }

            return ToListOpportunityDto(result);
        }

        /// <summary>
        ///   Sets access rights for other users to the list of opportunities with the IDs specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID list</param>
        /// <param name="isPrivate">Opportunity privacy: private or not</param>
        /// <param name="accessList">List of users with access</param>
        /// <short>Set opportunity access rights</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Update(@"opportunity/access")]
        public IEnumerable<OpportunityDto> SetAccessToBatchDeal(SetAccessToBatchDealRequestDto inDto)
        {
            var opportunityid = inDto.Opportunityid;
            var isPrivate = inDto.isPrivate;
            var accessList = inDto.AccessList;

            if (opportunityid == null) throw new ArgumentException();

            var result = new List<Deal>();

            var deals = DaoFactory.GetDealDao().GetDeals(opportunityid.ToArray());

            if (!deals.Any()) return new List<OpportunityDto>();

            foreach (var d in deals)
            {
                if (d == null) throw new ItemNotFoundException();

                if (!(CRMSecurity.IsAdmin || d.CreateBy == SecurityContext.CurrentAccount.ID)) continue;

                SetAccessToDeal(d, isPrivate, accessList, false, true);
                result.Add(d);
            }

            return ToListOpportunityDto(result);
        }


        /// <summary>
        ///   Deletes the group of opportunities with the IDs specified in the request
        /// </summary>
        /// <param name="opportunityids">Opportunity ID list</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete opportunity group</short> 
        /// <category>Opportunities</category>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Update(@"opportunity")]
        public IEnumerable<OpportunityDto> DeleteBatchDeals(IEnumerable<int> opportunityids)
        {
            if (opportunityids == null || !opportunityids.Any()) throw new ArgumentException();

            var opportunities = DaoFactory.GetDealDao().DeleteBatchDeals(opportunityids.ToArray());
            MessageService.Send(MessageAction.OpportunitiesDeleted, MessageTarget.Create(opportunityids), opportunities.Select(o => o.Title));

            return ToListOpportunityDto(opportunities);
        }

        /// <summary>
        ///   Deletes the list of all opportunities matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="responsibleid">Opportunity responsible</param>
        /// <param optional="true" name="opportunityStagesid">Opportunity stage ID</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="contactAlsoIsParticipant">Participation status: take into account opportunities where the contact is a participant or not</param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param optional="true" name="stageType" remark="Allowed values: {Open, ClosedAndWon, ClosedAndLost}">Opportunity stage type</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <short>Delete opportunity group</short> 
        /// <category>Opportunities</category>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Delete(@"opportunity/filter")]
        public IEnumerable<OpportunityDto> DeleteBatchDeals(
            Guid responsibleid,
            int opportunityStagesid,
            IEnumerable<string> tags,
            int contactid,
            DealMilestoneStatus? stageType,
            bool? contactAlsoIsParticipant,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            var deals = DaoFactory.GetDealDao().GetDeals(ApiContext.FilterValue,
                                                         responsibleid,
                                                         opportunityStagesid,
                                                         tags,
                                                         contactid,
                                                         stageType,
                                                         contactAlsoIsParticipant,
                                                         fromDate, toDate, 0, 0, null);

            if (!deals.Any()) return Enumerable.Empty<OpportunityDto>();

            deals = DaoFactory.GetDealDao().DeleteBatchDeals(deals);
            MessageService.Send(MessageAction.OpportunitiesDeleted, MessageTarget.Create(deals.Select(x => x.ID)), deals.Select(d => d.Title));

            return ToListOpportunityDto(deals);
        }

        /// <summary>
        ///   Returns the list of all opportunities matching the parameters specified in the request
        /// </summary>
        /// <param optional="true" name="responsibleid">Opportunity responsible</param>
        /// <param optional="true" name="opportunityStagesid">Opportunity stage ID</param>
        /// <param optional="true" name="tags">Tags</param>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <param optional="true" name="contactAlsoIsParticipant">Participation status: take into account opportunities where the contact is a participant or not</param>
        /// <param optional="true" name="fromDate">Start date</param>
        /// <param optional="true" name="toDate">End date</param>
        /// <param optional="true" name="stageType" remark="Allowed values: {Open, ClosedAndWon, ClosedAndLost}">Opportunity stage type</param>
        /// <short>Get opportunity list</short> 
        /// <category>Opportunities</category>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Read(@"opportunity/filter")]
        public IEnumerable<OpportunityDto> GetDeals(
            Guid responsibleid,
            int opportunityStagesid,
            IEnumerable<string> tags,
            int contactid,
            DealMilestoneStatus? stageType,
            bool? contactAlsoIsParticipant,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            DealSortedByType dealSortedByType;

            IEnumerable<OpportunityDto> result;

            var searchString = ApiContext.FilterValue;

            OrderBy dealsOrderBy;

            if (ASC.CRM.Classes.EnumExtension.TryParse(ApiContext.SortBy, true, out dealSortedByType))
            {
                dealsOrderBy = new OrderBy(dealSortedByType, !ApiContext.SortDescending);
            }
            else if (string.IsNullOrEmpty(ApiContext.SortBy))
            {
                dealsOrderBy = new OrderBy(DealSortedByType.Stage, true);
            }
            else
            {
                dealsOrderBy = null;
            }

            var fromIndex = (int)ApiContext.StartIndex;
            var count = (int)ApiContext.Count;

            if (dealsOrderBy != null)
            {
                result = ToListOpportunityDto(DaoFactory.GetDealDao().GetDeals(
                    searchString,
                    responsibleid,
                    opportunityStagesid,
                    tags,
                    contactid,
                    stageType,
                    contactAlsoIsParticipant,
                    fromDate,
                    toDate,
                    fromIndex,
                    count,
                    dealsOrderBy)).ToList();

                ApiContext.SetDataPaginated();
                ApiContext.SetDataFiltered();
                ApiContext.SetDataSorted();
            }
            else
            {
                result = ToListOpportunityDto(DaoFactory.GetDealDao().GetDeals(
                    searchString,
                    responsibleid,
                    opportunityStagesid,
                    tags,
                    contactid,
                    stageType,
                    contactAlsoIsParticipant,
                    fromDate,
                    toDate,
                    0, 0, null)).ToList();
            }


            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory
                    .GetDealDao()
                    .GetDealsCount(searchString,
                                   responsibleid,
                                   opportunityStagesid,
                                   tags,
                                   contactid,
                                   stageType,
                                   contactAlsoIsParticipant,
                                   fromDate,
                                   toDate);
            }

            ApiContext.SetTotalCount(totalCount);

            return result;
        }

        /// <summary>
        ///    Deletes the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <short>Delete opportunity</short> 
        /// <category>Opportunities</category>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Opportunity
        /// </returns>
        [Delete(@"opportunity/{opportunityid:int}")]
        public OpportunityDto DeleteDeal(int opportunityid)
        {
            if (opportunityid <= 0) throw new ArgumentException();

            var deal = DaoFactory.GetDealDao().DeleteDeal(opportunityid);
            if (deal == null) throw new ItemNotFoundException();

            MessageService.Send(MessageAction.OpportunityDeleted, MessageTarget.Create(deal.ID), deal.Title);

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///    Creates the opportunity with the parameters specified in the request
        /// </summary>
        /// <short>Create opportunity</short> 
        /// <param name="contactid">Opportunity primary contact</param>
        /// <param optional="true" name="members">Participants</param>
        /// <param name="title">Opportunity title</param>
        /// <param optional="true" name="description">Opportunity description</param>
        /// <param name="responsibleid">Opportunity responsible</param>
        /// <param name="bidType" remark="Allowed values: FixedBid, PerHour, PerDay,PerWeek, PerMonth, PerYear">Bid</param>
        /// <param optional="true" name="bidValue">Amount of transaction</param>
        /// <param name="bidCurrencyAbbr">Currency (Abbreviation)</param>
        /// <param name="perPeriodValue">Period</param>
        /// <param name="stageid">Stage ID</param>
        /// <param optional="true" name="successProbability">Opportunity success probability</param>
        /// <param optional="true" name="actualCloseDate">Actual opportunity closure date</param>
        /// <param optional="true" name="expectedCloseDate">Expected opportunity closure date</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <param name="isPrivate">Opportunity privacy: private or not</param>
        /// <param optional="true" name="accessList">List of users with access to the opportunity</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the opportunity</param>
        /// <category>Opportunities</category>
        /// <returns>
        ///  Opportunity
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Create(@"opportunity")]
        public OpportunityDto CreateDeal(CreateOrUpdateDealRequestDto inDto)
        {

            var title = inDto.Title;
            var description = inDto.Description;
            var responsibleid = inDto.Responsibleid;
            var bidType = inDto.BidType;
            var bidValue = inDto.BidValue;
            var perPeriodValue = inDto.PerPeriodValue;
            var stageid = inDto.Stageid;
            var successProbability = inDto.SuccessProbability;
            var contactid = inDto.Contactid;
            var actualCloseDate = inDto.ActualCloseDate;
            var expectedCloseDate = inDto.ExpectedCloseDate;
            var bidCurrencyAbbr = inDto.BidCurrencyAbbr;
            var isPrivate = inDto.isPrivate;
            var isNotify = inDto.isNotify;
            var accessList = inDto.AccessList;
            var customFieldList = inDto.CustomFieldList;
            var members = inDto.Members;

            var deal = new Deal
            {
                Title = title,
                Description = description,
                ResponsibleID = responsibleid,
                BidType = bidType,
                BidValue = bidValue,
                PerPeriodValue = perPeriodValue,
                DealMilestoneID = stageid,
                DealMilestoneProbability = successProbability < 0 ? 0 : (successProbability > 100 ? 100 : successProbability),
                ContactID = contactid,
                ActualCloseDate = actualCloseDate,
                ExpectedCloseDate = expectedCloseDate,
                BidCurrency = !String.IsNullOrEmpty(bidCurrencyAbbr) ? bidCurrencyAbbr.ToUpper() : null,
            };

            CRMSecurity.DemandCreateOrUpdate(deal);

            deal.ID = DaoFactory.GetDealDao().CreateNewDeal(deal);

            deal.CreateBy = SecurityContext.CurrentAccount.ID;
            deal.CreateOn = DateTime.UtcNow;

            SetAccessToDeal(deal, isPrivate, accessList, isNotify, false);

            var membersList = members != null ? members.ToList() : new List<int>();

            if (deal.ContactID > 0)
                membersList.Add(deal.ContactID);

            if (membersList.Any())
            {
                var contacts = DaoFactory.GetContactDao().GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();
                DaoFactory.GetDealDao().SetMembers(deal.ID, membersList.ToArray());
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Opportunity).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.GetCustomFieldDao().SetFieldValue(EntityType.Opportunity, deal.ID, field.Key, field.Value);
                }
            }

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///    Updates the selected opportunity with the parameters specified in the request
        /// </summary>
        /// <short>Update opportunity</short>
        ///<param name="opportunityid">Opportunity ID</param>
        ///<param name="contactid">Opportunity primary contact</param>
        /// <param optional="true" name="members">Participants</param>
        /// <param name="title">Opportunity title</param>
        /// <param optional="true" name="description">Opportunity description</param>
        /// <param name="responsibleid">Opportunity responsible</param>
        /// <param name="bidType" remark="Allowed values: FixedBid, PerHour, PerDay,PerWeek, PerMonth, PerYear">Bid</param>
        /// <param optional="true" name="bidValue">Amount of transaction</param>
        /// <param optional="true" name="bidCurrencyAbbr">Currency (Abbreviation)</param>
        /// <param name="perPeriodValue">Period</param>
        /// <param name="stageid">Stage ID</param>
        /// <param optional="true" name="successProbability">Opportunity success probability</param>
        /// <param optional="true" name="actualCloseDate">Actual opportunity closure date</param>
        /// <param optional="true" name="expectedCloseDate">Expected opportunity closure date</param>
        /// <param optional="true" name="customFieldList">User field list</param>
        /// <param name="isPrivate">Opportunity privacy: private or not</param>
        /// <param optional="true" name="accessList">List of users with access to the opportunity</param>
        /// <param name="isNotify" optional="true">Notify users in accessList about the opportunity</param>
        /// <category>Opportunities</category>
        /// <returns>
        ///  Opportunity
        /// </returns>
        ///<exception cref="ArgumentException"></exception>
        [Update(@"opportunity/{opportunityid:int}")]
        public OpportunityDto UpdateDeal(
            int opportunityid, CreateOrUpdateDealRequestDto inDto)
        {

            var title = inDto.Title;
            var description = inDto.Description;
            var responsibleid = inDto.Responsibleid;
            var bidType = inDto.BidType;
            var bidValue = inDto.BidValue;
            var perPeriodValue = inDto.PerPeriodValue;
            var stageid = inDto.Stageid;
            var successProbability = inDto.SuccessProbability;
            var contactid = inDto.Contactid;
            var actualCloseDate = inDto.ActualCloseDate;
            var expectedCloseDate = inDto.ExpectedCloseDate;
            var bidCurrencyAbbr = inDto.BidCurrencyAbbr;
            var isPrivate = inDto.isPrivate;
            var isNotify = inDto.isNotify;
            var accessList = inDto.AccessList;
            var customFieldList = inDto.CustomFieldList;
            var members = inDto.Members;

            var deal = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (deal == null) throw new ItemNotFoundException();

            deal.Title = title;
            deal.Description = description;
            deal.ResponsibleID = responsibleid;
            deal.BidType = bidType;
            deal.BidValue = bidValue;
            deal.PerPeriodValue = perPeriodValue;
            deal.DealMilestoneID = stageid;
            deal.DealMilestoneProbability = successProbability < 0 ? 0 : (successProbability > 100 ? 100 : successProbability);
            deal.ContactID = contactid;
            deal.ActualCloseDate = actualCloseDate;
            deal.ExpectedCloseDate = expectedCloseDate;
            deal.BidCurrency = !String.IsNullOrEmpty(bidCurrencyAbbr) ? bidCurrencyAbbr.ToUpper() : null;

            CRMSecurity.DemandCreateOrUpdate(deal);

            DaoFactory.GetDealDao().EditDeal(deal);

            deal = DaoFactory.GetDealDao().GetByID(opportunityid);

            var membersList = members != null ? members.ToList() : new List<int>();
            if (membersList.Any())
            {
                var contacts = DaoFactory.GetContactDao().GetContacts(membersList.ToArray()).Where(CRMSecurity.CanAccessTo).ToList();
                membersList = contacts.Select(m => m.ID).ToList();

                DaoFactory.GetDealDao().SetMembers(deal.ID, membersList.ToArray());
            }


            if (CRMSecurity.IsAdmin || deal.CreateBy == SecurityContext.CurrentAccount.ID)
            {
                SetAccessToDeal(deal, isPrivate, accessList, isNotify, false);
            }

            if (customFieldList != null)
            {
                var existingCustomFieldList = DaoFactory.GetCustomFieldDao().GetFieldsDescription(EntityType.Opportunity).Select(fd => fd.ID).ToList();
                foreach (var field in customFieldList)
                {
                    if (string.IsNullOrEmpty(field.Value) || !existingCustomFieldList.Contains(field.Key)) continue;
                    DaoFactory.GetCustomFieldDao().SetFieldValue(EntityType.Opportunity, deal.ID, field.Key, field.Value);
                }
            }

            return OpportunityDtoHelper.Get(deal);
        }

        /// <summary>
        ///    Returns the list of all contacts associated with the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <short>Get all opportunity contacts</short> 
        /// <category>Opportunities</category>
        /// <returns>Contact list</returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"opportunity/{opportunityid:int}/contact")]
        public IEnumerable<ContactDto> GetDealMembers(int opportunityid)
        {
            var opportunity = DaoFactory.GetDealDao().GetByID(opportunityid);

            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();

            var contactIDs = DaoFactory.GetDealDao().GetMembers(opportunityid);
            
            if (contactIDs == null) return new List<ContactDto>();

            var result = ContactDtoHelper.ToListContactDto(DaoFactory.GetContactDao().GetContacts(contactIDs)).ToList();

            result.ForEach(item => { if (item.Id == opportunity.ContactID) item.CanEdit = false; });

            return result;
        }

        /// <summary>
        ///   Adds the selected contact to the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <short>Add opportunity contact</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Create(@"opportunity/{opportunityid:int}/contact/{contactid:int}")]
        public ContactDto AddMemberToDeal(int opportunityid, int contactid)
        {
            if (opportunityid <= 0 || contactid <= 0) throw new ArgumentException();

            var opportunity = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var result = ContactDtoHelper.ToContactDto(contact);

            DaoFactory.GetDealDao().AddMember(opportunityid, contactid);

            var messageAction = contact is Company ? MessageAction.OpportunityLinkedCompany : MessageAction.OpportunityLinkedPerson;
            MessageService.Send(messageAction, MessageTarget.Create(opportunity.ID), opportunity.Title, contact.GetTitle());

            return result;
        }

        /// <summary>
        ///   Deletes the selected contact from the opportunity with the ID specified in the request
        /// </summary>
        /// <param name="opportunityid">Opportunity ID</param>
        /// <param name="contactid">Contact ID</param>
        /// <short>Delete opportunity contact</short> 
        /// <category>Opportunities</category>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    Participant
        /// </returns>
        [Delete(@"opportunity/{opportunityid:int}/contact/{contactid:int}")]
        public ContactDto DeleteMemberFromDeal(int opportunityid, int contactid)
        {
            if ((opportunityid <= 0) || (contactid <= 0)) throw new ArgumentException();

            var opportunity = DaoFactory.GetDealDao().GetByID(opportunityid);
            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity)) throw new ItemNotFoundException();

            var contact = DaoFactory.GetContactDao().GetByID(contactid);
            if (contact == null || !CRMSecurity.CanAccessTo(contact)) throw new ItemNotFoundException();

            var result = ContactDtoHelper.ToContactDto(contact);

            DaoFactory.GetDealDao().RemoveMember(opportunityid, contactid);

            var messageAction = contact is Company ? MessageAction.OpportunityUnlinkedCompany : MessageAction.OpportunityUnlinkedPerson;
            MessageService.Send(messageAction, MessageTarget.Create(opportunity.ID), opportunity.Title, contact.GetTitle());

            return result;
        }

        /// <summary>
        ///    Returns the list of 30 opportunities in the CRM module with prefix
        /// </summary>
        /// <param optional="true" name="prefix"></param>
        /// <param optional="true" name="contactID"></param>
        /// <param optional="true" name="internalSearch"></param>
        /// <category>Opportunities</category>
        /// <returns>
        ///    Opportunities list
        /// </returns>
        /// <visible>false</visible>
        [Read(@"opportunity/byprefix")]
        public IEnumerable<OpportunityDto> GetDealsByPrefix(string prefix, int contactID, bool internalSearch = true)
        {
            var result = new List<OpportunityDto>();

            if (contactID > 0 && internalSearch)
            {
                var findedDeals = DaoFactory.GetDealDao().GetDealsByContactID(contactID);
                foreach (var item in findedDeals)
                {
                    if (item.Title.IndexOf(prefix, StringComparison.Ordinal) != -1)
                    {
                        result.Add(OpportunityDtoHelper.Get(item));
                    }
                }

                ApiContext.SetTotalCount(result.Count);
            }
            else
            {
                const int maxItemCount = 30;
                var findedDeals = DaoFactory.GetDealDao().GetDealsByPrefix(prefix, 0, maxItemCount, contactID, internalSearch);
                foreach (var item in findedDeals)
                {
                    result.Add(OpportunityDtoHelper.Get(item));
                }
            }

            return result;
        }

        /// <summary>
        ///   Returns the list of all contact opportunities
        /// </summary>
        /// <param optional="true" name="contactid">Contact ID</param>
        /// <short>Get opportunity list</short> 
        /// <category>Opportunities</category>
        /// <returns>
        ///   Opportunity list
        /// </returns>
        [Read(@"opportunity/bycontact/{contactid:int}")]
        public IEnumerable<OpportunityDto> GetDeals(int contactid)
        {
            var deals = DaoFactory.GetDealDao().GetDealsByContactID(contactid);
            return ToListOpportunityDto(deals);
        }

        /// <visible>false</visible>
        [Update(@"opportunity/{opportunityid:int}/creationdate")]
        public void SetDealCreationDate(int opportunityid, ApiDateTime creationDate)
        {
            var dao = DaoFactory.GetDealDao();
            var opportunity = dao.GetByID(opportunityid);

            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity))
                throw new ItemNotFoundException();

            dao.SetDealCreationDate(opportunityid, creationDate);
        }

        /// <visible>false</visible>
        [Update(@"opportunity/{opportunityid:int}/lastmodifeddate")]
        public void SetDealLastModifedDate(int opportunityid, ApiDateTime lastModifedDate)
        {
            var dao = DaoFactory.GetDealDao();
            var opportunity = dao.GetByID(opportunityid);

            if (opportunity == null || !CRMSecurity.CanAccessTo(opportunity))
                throw new ItemNotFoundException();

            dao.SetDealLastModifedDate(opportunityid, lastModifedDate);
        }


        private IEnumerable<OpportunityDto> ToListOpportunityDto(ICollection<Deal> deals)
        {
            if (deals == null || deals.Count == 0) return new List<OpportunityDto>();

            var result = new List<OpportunityDto>();

            var contactIDs = new List<int>();
            var dealIDs = new List<int>();
            var dealMilestoneIDs = new List<int>();

            foreach (var deal in deals)
            {
                contactIDs.Add(deal.ContactID);
                dealIDs.Add(deal.ID);
                dealMilestoneIDs.Add(deal.DealMilestoneID);
            }

            dealMilestoneIDs = dealMilestoneIDs.Distinct().ToList();

            var contacts = new Dictionary<int, ContactBaseDto>();

            var customFields = DaoFactory.GetCustomFieldDao().GetEnityFields(EntityType.Opportunity, dealIDs.ToArray())
                                         .GroupBy(item => item.EntityID)
                                         .ToDictionary(item => item.Key, item => item.Select(x=> new CustomFieldBaseDto(x)));

            var dealMilestones = DaoFactory.GetDealMilestoneDao().GetAll(dealMilestoneIDs.ToArray())
                                           .ToDictionary(item => item.ID, item => new DealMilestoneBaseDto(item));


            var dealMembers = DaoFactory.GetDealDao().GetMembers(dealIDs.ToArray());

            foreach (var value in dealMembers.Values)
            {
                contactIDs.AddRange(value);
            }

            contactIDs = contactIDs.Distinct().ToList();

            if (contactIDs.Count > 0)
            {
                DaoFactory.GetContactDao().GetContacts(contactIDs.ToArray()).ForEach(item =>
                    {
                        if (item == null) return;
                        contacts.Add(item.ID, ContactDtoHelper.GetContactBaseDto(item));
                    });
            }

            foreach (var deal in deals)
            {
                var dealDto = OpportunityDtoHelper.Get(deal);

                if (contacts.ContainsKey(deal.ContactID))
                {
                    dealDto.Contact = contacts[deal.ContactID];
                }

                dealDto.CustomFields = customFields.ContainsKey(deal.ID)
                                               ? customFields[deal.ID]
                                               : new List<CustomFieldBaseDto>();

                dealDto.Members = dealMembers.ContainsKey(dealDto.Id)
                                          ? dealMembers[dealDto.Id].Where(contacts.ContainsKey).Select(item => contacts[item])
                                          : new List<ContactBaseDto>();

                if (dealMilestones.ContainsKey(deal.DealMilestoneID))
                {
                    dealDto.Stage = dealMilestones[deal.DealMilestoneID];
                }

                dealDto.IsPrivate = CRMSecurity.IsPrivate(deal);

                if (dealDto.IsPrivate)
                {
                    dealDto.AccessList = CRMSecurity.GetAccessSubjectTo(deal).Select(item => EmployeeWraperHelper.Get(item.Key));
                }

                if (!string.IsNullOrEmpty(deal.BidCurrency))
                {
                    dealDto.BidCurrency = CurrencyInfoDtoHelper.Get(CurrencyProvider.Get(deal.BidCurrency));
                }

                result.Add(dealDto);
            }

            return result;
        }
    }
}