
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.FluentUI.AspNetCore.Components;
using Pulse.Models.AI.Ali;
using Pulse.Models.AI.Tables;
using Pulse.Models.AI.Flapper;
using Pulse.Models.Api;
using Pulse.Models.Communication;
using Pulse.Models.Customers;
using Pulse.Models.Misc;
using Pulse.Models.Organizational;
using Pulse.Models.Production;
using Pulse.Models.Users;
using System.Text.Json;
using System.Text.Json.Serialization;
using Toolbelt.Blazor.SpeechSynthesis;

namespace Pulse.Web.Services
{
    public class DataTransferService : IDataTransferService
    {
        //public string gv_clientId { get; set; } = string.Empty;
        public Customer gv_client { get; set; }
        //public User gv_user { get; set; }
        public ApplicationUserSettings gvUserSettings { get; set; }
        public Company gv_Company { get; set; }
        public Period gv_Current_Period {  get; set; }
        public Tables gv_Tables { get; set; }
        public Flapper gv_Flapper { get; set; }
        public Ali gv_Ali {  get; set; }
        public Customer gv_Selected_Client { get; set; }
        public ClientCurrentStats gv_Selected_ClientStats { get; set; }
        public List<PulseMessage> gv_UserMessages { get; set; }
        public Division gv_Selected_Division { get; set; }
        public List<WorkInProgressDto> gv_Selected_Division_WIP { get; set; }
        public ClientRollerSpecification gv_Selected_Roll_Spec { get; set; }
        public string gv_AIContext_Mode { get; set; }
        public bool gv_IsMuted { get; set; } = false;

        public string gv_AIModel { get; set; } = "gpt-oss:latest";
        public string gv_User_Theme { get; set; } = "pulse";
        public    int gv_Context_Area { get; set; }
        public ShowAiIcon gvShowAI {  get; set; }
        public SpeechSynthesisVoice gv_User_AI_Voice { get; set; }
        public string gvUserFirstName { get; set; }

        public async Task SetgvAIModel(string aimodel)
        {
            gv_AIModel = aimodel;
            await Task.CompletedTask;
        }
        public async Task SetgvUserMessages (List<PulseMessage> messages)
        {
            gv_UserMessages = messages;
            await Task.CompletedTask;
        }
        public async Task<List<PulseMessage>> GetgvUserMessages()
        {
            return gv_UserMessages;
        }
        public async Task AddgvUserMessage(PulseMessage msg)
        {
            gv_UserMessages.Add(msg);
            await Task.CompletedTask;
        }
        public async Task<string> GetgvAIModel()
        {
            return gv_AIModel;
        }
        public async Task SetgvShowAI(ShowAiIcon showAiIcon)
        {
            gvShowAI = showAiIcon;
            await Task.CompletedTask;
        }
        public async Task<ShowAiIcon> GetgvShowAI()
        {
            return gvShowAI;
        }
        public async Task SetgvUserSettings(ApplicationUserSettings userSettings)
        {
            gvUserSettings = userSettings;
            gvUserFirstName = userSettings.PreferredUserName;
            await Task.CompletedTask;
        }
        public async Task<ApplicationUserSettings> GetgvUserSettings()
        {
            return await Task.FromResult(gvUserSettings);
        }
        //public async Task SetgvUser(User user)
        //{
        //    gv_user = user;
            //gvUserFirstName = gv_user.UserName.Split(' ')[0];
        //    await Task.CompletedTask;
        //}
        //public async Task<User> GetgvUser()
        //{
        //    return await Task.FromResult(gv_user);
        //}
        public async Task<Tables> GetgvTables()
        {
            return await Task.FromResult(gv_Tables);
        }
        public async Task<Ali> GetgvAli()
        {
            return await Task.FromResult(gv_Ali);
        }
        public async Task<Flapper> GetgvFlapper()
        {
            return await Task.FromResult(gv_Flapper);
        }
        public async Task<bool> GetgvIsMuted()
        {
            return await Task.FromResult(gv_IsMuted);
        }
        public async Task SetgvIsMuted(bool isMuted)
        {
            gv_IsMuted = isMuted;
            await Task.CompletedTask;
        }
        public async Task SetgvCurrentPeriod(Period period)
        {
            gv_Current_Period = period;
            await Task.CompletedTask;
        }
        public async Task<Period> GetgvCurrentPeriod()
        {
            return gv_Current_Period; 
        }
        public async Task SetgvUserFirstName(string firstName)
        {
            gvUserFirstName = firstName;
            await Task.CompletedTask;
        }
        public async Task SetgvTables(Tables tables)
        {
            gv_Tables = tables;
            await Task.CompletedTask;
        }
        public async Task SetgvAli(Ali ali)
        {
            gv_Ali = ali;
            await Task.CompletedTask;
        }
        public async Task SetgvFlapper(Flapper flapper)
        {
            gv_Flapper = flapper;
            await Task.CompletedTask;
        }
        public async Task<string> GetgvUserFirstName()
        {
            return await Task.FromResult(gvUserFirstName);
        }
        public async Task<string> GetgvUserTheme()
        {
            return await Task.FromResult(gv_User_Theme);
        }
        public async Task SetgvUserTheme(string uTheme)
        {
            gv_User_Theme = uTheme;
            await Task.CompletedTask;
        }
        public async Task SetgvUserAIVoice(SpeechSynthesisVoice aiVoice)
        {
            gv_User_AI_Voice = aiVoice;
            await Task.CompletedTask;
        }
        public async Task<SpeechSynthesisVoice> GetgvUserAIVoice()
        {
            return await Task.FromResult(gv_User_AI_Voice);
        }
        public async Task SetgvContextArea(int areaID)
        {
            gv_Context_Area = areaID;
            await Task.CompletedTask;
        }
        public async Task<int> GetgvContextArea()
        {
            return await Task.FromResult(gv_Context_Area);
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
        public async Task SetgvSelectedClientStats(ClientCurrentStats stats)
        {
            gv_Selected_ClientStats = stats;
            await Task.CompletedTask;
        }

        public async Task<ClientCurrentStats> GetgvSelectedClientStats()
        {
            return await Task.FromResult(gv_Selected_ClientStats);
        }
        public async Task SetgvSelectedDivision(Division div)
        {
            gv_Selected_Division = div;
            await Task.CompletedTask;
        }
        public async Task SetgvCompany(Company company)
        {
            gv_Company = company;
            await Task.CompletedTask;
        }
        public async Task<Company> GetgvCompany()
        {
            return await Task.FromResult(gv_Company);
        }
        public async Task<Division> GetgvSelectedDivision()
        {
            return await Task.FromResult(gv_Selected_Division);
        }
        public async Task SetgvSelectedDivisionWIP(List<WorkInProgressDto> wip)
        {
            gv_Selected_Division_WIP = wip;
            
            await Task.CompletedTask;
        }

        public async Task<List<WorkInProgressDto>> GetgvSelectedDivisionWIP()
        {
            return await Task.FromResult(gv_Selected_Division_WIP);
        }
        public async Task SetgvAIContextMode(string mode)
        {
            gv_AIContext_Mode = mode;
            await Task.CompletedTask;
        }

        public async Task<string> GetgvAIContextMode()
        {
            return await Task.FromResult(gv_AIContext_Mode);
        }
    }

    public interface IDataTransferService
    {
        public Task SetgvClient(Customer client);
        public Task<Customer> GetgvClient();
        public Task SetgvSelectedClient(Customer client);
        public Task<Customer> GetgvSelectedClient();
        //public Task SetgvUser(User user);
        //public Task<User> GetgvUser();
        public Task<List<PulseMessage>> GetgvUserMessages();
        public Task SetgvUserMessages(List<PulseMessage> messages);
        public Task SetgvIsMuted(bool isMuted);
        public Task<bool> GetgvIsMuted();
        public Task AddgvUserMessage(PulseMessage msg);
        public Task SetgvUserSettings(ApplicationUserSettings userSettings);
        public Task<ApplicationUserSettings> GetgvUserSettings();
        public Task SetgvSelectedRollSpec(ClientRollerSpecification rollSpec);
        public Task<ClientRollerSpecification> GetgvSelectedRollSpec();
        public Task SetgvSelectedClientStats(ClientCurrentStats stats);
        public Task<ClientCurrentStats> GetgvSelectedClientStats();
        public Task SetgvSelectedDivision(Division div);
        public Task<Division> GetgvSelectedDivision();
        public Task SetgvCompany(Company company);
        public Task<Company> GetgvCompany();
        public Task<ShowAiIcon> GetgvShowAI();
        public Task SetgvShowAI(ShowAiIcon showAiIcon);
        public Task<Period> GetgvCurrentPeriod();
        public Task SetgvCurrentPeriod(Period period);
        public Task SetgvSelectedDivisionWIP(List<WorkInProgressDto> wip);
        public Task<List<WorkInProgressDto>> GetgvSelectedDivisionWIP();
        public Task<string> GetgvAIContextMode();
        public Task SetgvAIContextMode(string mode);
        public Task<string> GetgvAIModel();
        public Task SetgvAIModel(string aimodel);
        public Task<string> GetgvUserTheme();
        public Task SetgvUserTheme(string uTheme);
        public Task SetgvContextArea(int areaID);
        public Task<int> GetgvContextArea();

        public Task SetgvTables(Tables tables);
        public Task<Tables> GetgvTables();
        public Task SetgvAli(Ali ali);
        public Task<Ali> GetgvAli();
        public Task SetgvFlapper(Flapper flapper);
        public Task<Flapper> GetgvFlapper();
        public Task SetgvUserAIVoice(SpeechSynthesisVoice aiVoice);
        public Task<SpeechSynthesisVoice> GetgvUserAIVoice();

        public Task SetgvUserFirstName(string firstName);
        public Task<string> GetgvUserFirstName();
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
