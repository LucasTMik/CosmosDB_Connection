using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Configuration;

using Models;
using Controllers;

namespace learning_module
{
    class Program
    {
        private DocumentClient client;
        private UserController UserController;
        static void Main(string[] args)
        {
            try
            {
                Program p = new Program();
                p.BasicOperations().Wait();
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, MEssage: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        private async Task BasicOperations()
        {
            this.client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["accountEndpoint"]), ConfigurationManager.AppSettings["accountKey"]);

            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = "Users" });

            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Users"), new DocumentCollection { Id = "WebCustomers" });

            this.UserController = new UserController(this.client);

            Console.WriteLine("Database and collection validation complete");



            UserSchema yanhe = new UserSchema
            {
                Id = "1",
                UserId = "yanhe",
                LastName = "He",
                FirstName = "Yan",
                Email = "yanhe@contoso.com",
                OrderHistory = new OrderHistory[]
                        {
                            new OrderHistory {
                                OrderId = "1000",
                                DateShipped = "08/17/2018",
                                Total = "52.49"
                            }
                        },
                ShippingPreference = new ShippingPreference[]
                        {
                                new ShippingPreference {
                                        Priority = 1,
                                        AddressLine1 = "90 W 8th St",
                                        City = "New York",
                                        State = "NY",
                                        ZipCode = "10001",
                                        Country = "USA"
                                }
                        },
            };

            await this.UserController.CreateUserDocumentIfNotExists("Users", "WebCustomers", yanhe);

            UserSchema nelapin = new UserSchema
            {
                Id = "2",
                UserId = "nelapin",
                LastName = "Pindakova",
                FirstName = "Nela",
                Email = "nelapin@contoso.com",
                Dividend = "8.50",
                OrderHistory = new OrderHistory[]
                 {
                    new OrderHistory {
                        OrderId = "1001",
                        DateShipped = "08/17/2018",
                        Total = "105.89"
                    }
                 },
                ShippingPreference = new ShippingPreference[]
                 {
                    new ShippingPreference {
                            Priority = 1,
                            AddressLine1 = "505 NW 5th St",
                            City = "New York",
                            State = "NY",
                            ZipCode = "10001",
                            Country = "USA"
                    },
                    new ShippingPreference {
                            Priority = 2,
                            AddressLine1 = "505 NW 5th St",
                            City = "New York",
                            State = "NY",
                            ZipCode = "10001",
                            Country = "USA"
                    }
                 },
                Coupons = new CouponsUsed[]
                 {
                    new CouponsUsed{
                        CouponCode = "Fall2018"
                    }
                 }
            };

            await this.UserController.CreateUserDocumentIfNotExists("Users", "WebCustomers", nelapin);

            await this.UserController.ReadUserDocument("Users", "WebCustomers", yanhe);

            yanhe.LastName = "Cunha";

            await this.UserController.ReplaceUserDocument("Users", "WebCustomers", yanhe);

            this.ExecuteSimpleQuery("Users", "WebCustomers");

            await this.UserController.RunStoredProcedure("Users", "WebCustomers", yanhe);

            await this.UserController.DeleteUserDocument("Users", "WebCustomers", yanhe);

        }




        //QUERIES
        private void ExecuteSimpleQuery(string databaseName, string collectionName)
        {
            //Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            //Here we find nelapin via their LastName
            IQueryable<UserSchema> userQuery = this.client.CreateDocumentQuery<UserSchema>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                queryOptions)
                .Where(u => u.LastName == "Pindakova");

            // The query is executed synchrously here, but can also executed asynchrously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (UserSchema user in userQuery)
            {
                Console.WriteLine("\tRead {0}", user);
            }

            //Now execute the same query in SQL
            IQueryable<UserSchema> userQuerySql = this.client.CreateDocumentQuery<UserSchema>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                "SELECT * FROM User WHERE User.lastName = 'Pindakova'", queryOptions);

            Console.WriteLine("Running SQL query...");
            foreach (UserSchema user in userQuerySql)
            {
                Console.WriteLine("\tRead {0}", user);
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
