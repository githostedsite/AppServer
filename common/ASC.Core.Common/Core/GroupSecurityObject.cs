using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASC.Common.Security;
using ASC.Common.Security.Authorizing;

namespace ASC.Core
{
    public class GroupSecurityObject : ISecurityObject
    {
        public object SecurityId { get; set; }

        public Type ObjectType { get; set; }

        public bool InheritSupported => false;

        public bool ObjectRolesSupported => false;

        public GroupSecurityObject(Guid groupId)
        {
            SecurityId = groupId;
            ObjectType = typeof(Guid);
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            throw new NotImplementedException();
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }
    }
}
