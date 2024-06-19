using Realms;
using Realms.Sync;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;



namespace GestureSample.Maui.Data
{
    public class RealmService
    {
        private readonly Realms.Sync.App _app;
        private Realm _realm;
        private User _user;

        public RealmService()
        {
            var appId = "mathopiano-rxznboi"; // Replace with your Realm App ID

            //string connectionUri = "mongodb+srv://alexshifrin:Aligercargogmailcom@mathopiano.r268cgf.mongodb.net/?retryWrites=true&w=majority&appName=MathOPiano";

            //var connectionString = "your-mongodb-connection-string"; // Replace with your connection string
            //var client = new MongoDB.Driver.MongoClient(connectionString);
            try
            {
                _app = Realms.Sync.App.Create(appId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Creation failed: {ex.Message}");
            }
            //var database = client.GetDatabase("YourDatabaseName"); // Replace with your database name


            //var settings = MongoClientSettings.FromConnectionString(connectionUri);
            // Set the ServerApi field of the settings object to set the version of the Stable API on the client
            //settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            // Create a new client and connect to the server
            //var client = new MongoDB.Driver.MongoClient(settings);

            // Send a ping to confirm a successful connection
            /*try
            {
                //var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                //Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }*/

            //var realm = Realm.GetInstanceAsync();



            //_app = Realms.Sync.App.Create(appId);
            InitializeRealm();
        }

        private  async Task InitializeRealm()
        {
            
            //var credential = Credentials.Anonymous();
            try
            {_user = await _app.LogInAsync(Credentials.EmailPassword("alex.shifrin@mail.huji.ac.il", "Aligercargogmailcom"));
                //_user = await _app.LogInAsync(credential);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
            }
            var config = new FlexibleSyncConfiguration(_user);
            _realm = await Realm.GetInstanceAsync(config);
        }

        public async Task AddStateAsync(State state)
        {
            await _realm.WriteAsync(() =>
            {
                /*var item = new Item { Name = name };
                foreach (var tag in tags)
                {
                    item.Tags.Add(tag);
                }*/
                //_realm.Add(state);
            });
        }
        /*
        public async Task LogButtonPressAsync(string Id)
        {
            var item = _realm.Find<State>(Id);
            if (item != null)
            {
                await _realm.WriteAsync(realm =>
                {
                    item.PressTimes.Add(DateTimeOffset.Now);
                });
            }
        }*/

        public IList<State> GetItems()
        {
            return null;// _realm.All<State>().ToList();
        }
    }
}
