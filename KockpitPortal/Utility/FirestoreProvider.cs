using Google.Cloud.Firestore;
using KockpitPortal.Models.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KockpitPortal.Utility
{
    public interface IFirebaseEntity
    {
        public string Id { get; set; }
    }

    public class FirestoreProvider
    {
        private readonly FirestoreDb _fireStoreDb = null!;

        public FirestoreProvider(FirestoreDb fireStoreDb)
        {
            _fireStoreDb = fireStoreDb;
        }

        public async Task CheckOrAddCollection(string fieldPath, string entity, CancellationToken ct)
        {
            var CollectionReference = _fireStoreDb.Collection(fieldPath).Document();
            await CollectionReference.CreateAsync(entity);
        }


        public async Task Add(string fieldPath, Users entity, CancellationToken ct)
        {
            var userCollectionReference = _fireStoreDb.Collection(fieldPath);
            await userCollectionReference.AddAsync(entity);
            //var document = documentdatabase.Collection(typeof(T).Name).Document(entity.Id);
            //await document.SetAsync(entity, cancellationToken: ct);
            var res = await GetKey(fieldPath, entity.eMailId, ct);
            if (!string.IsNullOrEmpty(res))
            {
                entity.uid = res;
                await Update(fieldPath, res, entity, ct);
            }
        }

        public async Task AddMessage<T>(string fieldPath, T entity, CancellationToken ct)
        {
            var messagesCollectionReference = _fireStoreDb.Collection(fieldPath);
            await messagesCollectionReference.AddAsync(entity);
        }

        public async Task Update(string fieldPath, string key, Users entity,CancellationToken ct)
        {
            var document = _fireStoreDb.Collection(fieldPath).Document(key);
            Dictionary<FieldPath, object> updates = new Dictionary<FieldPath, object>
            {
                { new FieldPath("CompanyDomain"),  entity.CompanyDomain},
                { new FieldPath("userId"), entity.userId },
                { new FieldPath("UserName"), entity.UserName },
                { new FieldPath("eMailId"), entity.eMailId },
                { new FieldPath("IsActive"), entity.IsActive },
                { new FieldPath("uid"), entity.uid },
            };
            await document.UpdateAsync(updates);
            await document.SetAsync(entity);
        }

        public async Task Remove(string fieldPath, string key, CancellationToken ct)
        {
            var document = _fireStoreDb.Collection(fieldPath).Document(key);
            await document.DeleteAsync(null, cancellationToken: ct);
        }

        public async Task<string> GetKey(string fieldPath, string email, CancellationToken ct)
        {
            var collection = _fireStoreDb.Collection(fieldPath);
            var snapshot = await collection.GetSnapshotAsync(ct);
            var res = snapshot.Documents.Where(c => c.GetValue<string>("eMailId") == email).FirstOrDefault();
            if (res != null)
                return res.Id;
            else
                return string.Empty;
        }

        public async Task<string> GetKeyByName(string fieldPath, string username, CancellationToken ct)
        {
            var collection = _fireStoreDb.Collection(fieldPath);
            var snapshot = await collection.GetSnapshotAsync(ct);
            var res = snapshot.Documents.Where(c => c.GetValue<string>("UserName") == username).FirstOrDefault();
            if (res != null)
                return res.Id;
            else
                return string.Empty;
        }

        public async Task<T> Get<T>(string id, CancellationToken ct) where T : IFirebaseEntity
        {
            var document = _fireStoreDb.Collection(typeof(T).Name).Document(id);
            var snapshot = await document.GetSnapshotAsync(ct);
            return snapshot.ConvertTo<T>();
        }

        public async Task<IReadOnlyCollection<T>> GetAll<T>(string fieldPath, CancellationToken ct)
        {
            var collection = _fireStoreDb.Collection(fieldPath);
            var snapshot = await collection.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task<IReadOnlyCollection<T>> CheckUserByEmail<T>(string fieldPath, 
            string conditionPath, string conditionValue, CancellationToken ct)
        {
            var collection = _fireStoreDb.Collection(fieldPath).WhereEqualTo(conditionPath, conditionValue);
            var snapshot = await collection.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task<IReadOnlyCollection<T>> WhereEqualTo<T>(string fieldPath, object value, CancellationToken ct) where T : IFirebaseEntity
        {
            return await GetList<T>(_fireStoreDb.Collection(typeof(T).Name).WhereEqualTo(fieldPath, value), ct);
        }

        // just add here any method you need here WhereGreaterThan, WhereIn etc ...

        private static async Task<IReadOnlyCollection<T>> GetList<T>(Query query, CancellationToken ct) where T : IFirebaseEntity
        {
            var snapshot = await query.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }
    }
}
