
using Pulse.Models.Customers;
using Pulse.Models.Users;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pulse.Web.Services
{
    public class DataTransferService : IDataTransferService
    {
        //public string gv_clientId { get; set; } = string.Empty;
        public Customer gv_client { get; set; }
        public User gv_user { get; set; }
        public Customer gv_Selected_Client { get; set; }
        public ClientRollerSpecification gv_Selected_Roll_Spec { get; set; }
        public async Task SetgvUser(User user)
        {
            gv_user = user;
            await Task.CompletedTask;
        }
        public async Task<User> GetgvUser()
        {
            return await Task.FromResult(gv_user);
        }

        //public async Task SetClientId(string clientId)
        //{
        //    gv_clientId = clientId;
        //    await Task.CompletedTask;

        //    gv_client = await PulseApiClient.GetAsync<Customer>(gv_clientId);
        //}

        public async Task SetgvClient(Customer client)
        {
            gv_client = client;
            await Task.CompletedTask;
        }

        public async Task<Customer> GetgvClient()
        {
            return await Task.FromResult(gv_client);
        }

        public async Task SetgvSelectedClient(Customer client)
        {
            gv_Selected_Client = client;
            await Task.CompletedTask;
        }

        public async Task<Customer> GetgvSelectedClient()
        {
            return await Task.FromResult(gv_Selected_Client);
        }
        public async Task SetgvSelectedRollSpec(ClientRollerSpecification rollSpec)
        {
            gv_Selected_Roll_Spec = rollSpec;
            await Task.CompletedTask;
        }

        public async Task<ClientRollerSpecification> GetgvSelectedRollSpec()
        {
            return await Task.FromResult(gv_Selected_Roll_Spec);
        }
    }

    public interface IDataTransferService
    {
        public Task SetgvClient(Customer client);
        public Task<Customer> GetgvClient();
        public Task SetgvSelectedClient(Customer client);
        public Task<Customer> GetgvSelectedClient();
        public Task SetgvUser(User user);
        public Task<User> GetgvUser();
        public Task SetgvSelectedRollSpec(ClientRollerSpecification rollSpec);
        public Task<ClientRollerSpecification> GetgvSelectedRollSpec();
    }
    public class TypeConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Deserialization not supported for security reasons.");
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName); // Serialize as string (e.g., "System.String")
        }
    }
}
