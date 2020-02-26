using System;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

using Models;


namespace Controllers
{
    public class UserController
    {
        private DocumentClient client;
        public UserController(DocumentClient client)
        {   
            this.client = client;
        }
        public void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        public async Task DeleteUserDocument(string databaseName, string collectionName, UserSchema deleteuser)
        {
            try
            {
                await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, deleteuser.Id), new RequestOptions { PartitionKey = new PartitionKey(deleteuser.UserId) });
                this.WriteToConsoleAndPromptToContinue("Deleted user {0}", deleteuser.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    this.WriteToConsoleAndPromptToContinue("User {0} not found for deletion", deleteuser.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task ReplaceUserDocument(string databaseName, string collectionName, UserSchema updatedUser)
        {
            try
            {
                await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, updatedUser.Id), updatedUser, new RequestOptions { PartitionKey = new PartitionKey(updatedUser.UserId) });
                this.WriteToConsoleAndPromptToContinue("Replaced last name for {0}", updatedUser.LastName);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    WriteToConsoleAndPromptToContinue("User {0} not found to replacement", updatedUser.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task ReadUserDocument(string databaseName, string collectionName, UserSchema user)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, user.Id), new RequestOptions { PartitionKey = new PartitionKey(user.UserId) });
                this.WriteToConsoleAndPromptToContinue("Read user {0}", user.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    this.WriteToConsoleAndPromptToContinue("User {0} not read", user.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateUserDocumentIfNotExists(string databaseName, string collectionName, UserSchema user)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, user.Id), new RequestOptions { PartitionKey = new PartitionKey(user.UserId) });
                this.WriteToConsoleAndPromptToContinue("User {0} already exists in the database", user.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), user);
                    this.WriteToConsoleAndPromptToContinue("Created User {0}", user.Id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task RunStoredProcedure(string databaseName, string collectionName, UserSchema user)
        {
            await client.ExecuteStoredProcedureAsync<string>(UriFactory.CreateStoredProcedureUri(databaseName, collectionName, "UpdateOrderTotal"), new RequestOptions { PartitionKey = new PartitionKey(user.UserId) });
            Console.WriteLine("Stored procedure complete");
        }
    }
}