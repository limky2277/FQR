using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Libmongocrypt;

namespace TicketBOT.Core.Helpers
{
    public static class DBHelper
    {
        //private const string LocalMasterKey = "Mng0NCt41N3YkQ5aXRRMkhGRkxNkVyNURNR6ZHVUYUJCaNmdWMDFBMUGdQV09wOGVNYUMxT2k3XVyZG9jZKelhaQmRCZGJkT1QURhZ2h2UzR2d2RrZzh0cFBwM3uSjFk";
        private static MongoClient client = null;

        public static MongoClient getCient(TicketBOT.Core.Models.ApplicationSettings sett)
        {
            //var localMasterKey = Convert.FromBase64String(LocalMasterKey);

            //var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
            //var localKey = new Dictionary<string, object>
            //{
            //    { "key", localMasterKey }
            //};
            //kmsProviders.Add("local", localKey);

            //var keyVaultNamespace = CollectionNamespace.FromFullName("admin.datakeys");
            //var autoEncryptionOptions = new AutoEncryptionOptions(keyVaultNamespace, kmsProviders);

            if (client == null)
            {

                string mnb = sett.General.SysInfo;
                string cn = string.Format(sett.TicketBOTDb.ConnectionString, Utility.ParseDInfo(sett.TicketBOTDb.DBPass, mnb));
                
                //var url = MongoUrl.Create(cn);
                //var mongoClientSettings = new MongoClientSettings
                //{
                //    AutoEncryptionOptions = autoEncryptionOptions,
                //    Server = url.Server,
                //    Credential = url.GetCredential()
                //};
                //client = new MongoClient(mongoClientSettings);
                client = new MongoClient(cn);
            }

            return client;
        }
      
    }
}
