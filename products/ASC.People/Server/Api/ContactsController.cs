﻿using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.People.Api;

public class ContactsController : BasePeopleController
{
    private readonly EmployeeWraperFullHelper _employeeWraperFullHelper;

    public ContactsController(
        UserManager userManager,
        AuthContext authContext,
        ApiContext apiContext,
        PermissionContext permissionContext,
        SecurityContext securityContext,
        MessageService messageService,
        MessageTarget messageTarget,
        StudioNotifyService studioNotifyService,
        EmployeeWraperFullHelper employeeWraperFullHelper) 
        : base(
            userManager,
            authContext,
            apiContext,
            permissionContext,
            securityContext,
            messageService,
            messageTarget,
            studioNotifyService)
    {
        _employeeWraperFullHelper = employeeWraperFullHelper;
    }

    [Delete("{userid}/contacts")]
    public EmployeeFullDto DeleteMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
    {
        return DeleteMemberContacts(userid, memberModel);
    }

    [Delete("{userid}/contacts")]
    [Consumes("application/x-www-form-urlencoded")]
    public EmployeeFullDto DeleteMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
    {
        return DeleteMemberContacts(userid, memberModel);
    }

    [Create("{userid}/contacts")]
    public EmployeeFullDto SetMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
    {
        return SetMemberContacts(userid, memberModel);
    }

    [Create("{userid}/contacts")]
    [Consumes("application/x-www-form-urlencoded")]
    public EmployeeFullDto SetMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
    {
        return SetMemberContacts(userid, memberModel);
    }

    [Update("{userid}/contacts")]
    public EmployeeFullDto UpdateMemberContactsFromBody(string userid, [FromBody] UpdateMemberModel memberModel)
    {
        return UpdateMemberContacts(userid, memberModel);
    }

    [Update("{userid}/contacts")]
    [Consumes("application/x-www-form-urlencoded")]
    public EmployeeFullDto UpdateMemberContactsFromForm(string userid, [FromForm] UpdateMemberModel memberModel)
    {
        return UpdateMemberContacts(userid, memberModel);
    }

    private void DeleteContacts(IEnumerable<Contact> contacts, UserInfo user)
    {
        PermissionContext.DemandPermissions(new UserSecurityProvider(user.ID), Constants.Action_EditUser);
        if (contacts == null)
        {
            return;
        }

        if (user.ContactsList == null)
        {
            user.ContactsList = new List<string>();
        }

        foreach (var contact in contacts)
        {
            var index = user.ContactsList.IndexOf(contact.Type);
            if (index != -1)
            {
                //Remove existing
                user.ContactsList.RemoveRange(index, 2);
            }
        }
    }

    private EmployeeFullDto DeleteMemberContacts(string userid, UpdateMemberModel memberModel)
    {
        var user = GetUserInfo(userid);

        if (UserManager.IsSystemUser(user.ID))
        {
            throw new SecurityException();
        }

        DeleteContacts(memberModel.Contacts, user);
        UserManager.SaveUserInfo(user);

        return _employeeWraperFullHelper.GetFull(user);
    }

    private EmployeeFullDto SetMemberContacts(string userid, UpdateMemberModel memberModel)
    {
        var user = GetUserInfo(userid);

        if (UserManager.IsSystemUser(user.ID))
        {
            throw new SecurityException();
        }

        user.ContactsList.Clear();
        UpdateContacts(memberModel.Contacts, user);
        UserManager.SaveUserInfo(user);

        return _employeeWraperFullHelper.GetFull(user);
    }

    private EmployeeFullDto UpdateMemberContacts(string userid, UpdateMemberModel memberModel)
    {
        var user = GetUserInfo(userid);

        if (UserManager.IsSystemUser(user.ID))
        {
            throw new SecurityException();
        }

        UpdateContacts(memberModel.Contacts, user);
        UserManager.SaveUserInfo(user);

        return _employeeWraperFullHelper.GetFull(user);
    }
}