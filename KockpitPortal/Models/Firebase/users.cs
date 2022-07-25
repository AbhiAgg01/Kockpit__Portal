using Google.Cloud.Firestore;
using KockpitPortal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KockpitPortal.Models.Firebase
{
    [FirestoreData]
    public class Users
    {
        [FirestoreProperty]
        public string CompanyDomain { get; set; }

        [FirestoreProperty]
        public string userId { get; set; }
        [FirestoreProperty]
        public string UserName { get; set; }

        [FirestoreProperty]
        public string eMailId { get; set; }

        [FirestoreProperty]
        public bool IsActive { get; set; }

        [FirestoreProperty]
        public string uid { get; set; }
    }
}
