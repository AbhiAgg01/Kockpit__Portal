using Google.Cloud.Firestore;
using KockpitPortal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Firebase
{
    [FirestoreData]
    public class Organizations : IFirebaseEntity
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string OrganizationId { get; set; }

        [FirestoreProperty]
        public string OrganizationName { get; set; }

        public Organizations()
        {
        }

        public Organizations(string organizationName)
        {
            Id = Guid.NewGuid().ToString("N");
            OrganizationName = organizationName;
        }
    }
}
