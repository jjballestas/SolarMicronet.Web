using System.Numerics;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using SolarMicronet.Web.Models;

namespace SolarMicronet.Web.Services;

public interface IMicrogridReadService
{
    Task<ParticipantInfo> GetParticipantInfoAsync(string address);
    Task<BigInteger> GetEnergonBalanceAsync(string address);
    Task<BigInteger> GetTotalSupplyAsync();
    Task<BigInteger> GetFundBalanceAsync();
    Task<string[]> GetCurrentValidatorsAsync();
    Task<bool> IsSmartMeterAuthorizedAsync(string meterAddress);
    Task<BigInteger> GetMeterNonceAsync(string meterAddress);
    Task<Activity?> GetActivityAsync(BigInteger activityId);
    Task<List<Activity>> GetActivitiesAsync(int count = 20);
    Task<BigInteger> GetParticipantCountAsync();
}

public class MicrogridReadService : IMicrogridReadService
{
    private readonly ILogger<MicrogridReadService> _logger;
    private readonly Web3 _web3;
    private readonly Contract _energonContract;
    private readonly Contract _microgridContract;

    public MicrogridReadService(ILogger<MicrogridReadService> logger)
    {
        _logger = logger;
        _web3 = new Web3(BlockchainConfig.RPC_URL);

        // Inicializar contratos con ABIs
        _energonContract = _web3.Eth.GetContract(ContractABIs.EnergonTokenABI, BlockchainConfig.ENERGON_TOKEN_ADDRESS);
        _microgridContract = _web3.Eth.GetContract(ContractABIs.MicrogridManagerABI, BlockchainConfig.MICROGRID_MANAGER_ADDRESS);
    }

    public async Task<ParticipantInfo> GetParticipantInfoAsync(string address)
    {
        try
        {
            var isRegisteredFunc = _microgridContract.GetFunction("isRegistered");
            var participantTypeFunc = _microgridContract.GetFunction("participantType");
            var balanceFunc = _energonContract.GetFunction("balanceOf");

            var isRegistered = await isRegisteredFunc.CallAsync<bool>(address);
            var participantType = await participantTypeFunc.CallAsync<int>(address);
            var balance = await balanceFunc.CallAsync<BigInteger>(address);

            return new ParticipantInfo
            {
                Address = address,
                IsRegistered = isRegistered,
                Type = (ParticipantType)participantType,
                Balance = balance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participant info for {Address}", address);
            throw;
        }
    }

    public async Task<BigInteger> GetEnergonBalanceAsync(string address)
    {
        try
        {
            var balanceFunc = _energonContract.GetFunction("balanceOf");
            return await balanceFunc.CallAsync<BigInteger>(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Energon balance for {Address}", address);
            throw;
        }
    }

    public async Task<BigInteger> GetTotalSupplyAsync()
    {
        try
        {
            var totalSupplyFunc = _energonContract.GetFunction("totalSupply");
            return await totalSupplyFunc.CallAsync<BigInteger>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total supply");
            throw;
        }
    }

    public async Task<BigInteger> GetFundBalanceAsync()
    {
        try
        {
            var balanceFunc = _energonContract.GetFunction("balanceOf");
            return await balanceFunc.CallAsync<BigInteger>(BlockchainConfig.MICROGRID_MANAGER_ADDRESS);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fund balance");
            throw;
        }
    }

    public async Task<string[]> GetCurrentValidatorsAsync()
    {
        try
        {
            var validatorsFunc = _microgridContract.GetFunction("getCurrentValidators");
            var validators = await validatorsFunc.CallAsync<List<string>>();
            return validators.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current validators");
            return new string[3];
        }
    }

    public async Task<bool> IsSmartMeterAuthorizedAsync(string meterAddress)
    {
        try
        {
            var authorizedFunc = _microgridContract.GetFunction("authorizedMeters");
            return await authorizedFunc.CallAsync<bool>(meterAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking meter authorization");
            return false;
        }
    }

    public async Task<BigInteger> GetMeterNonceAsync(string meterAddress)
    {
        try
        {
            var nonceFunc = _microgridContract.GetFunction("meterNonce");
            return await nonceFunc.CallAsync<BigInteger>(meterAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meter nonce");
            return BigInteger.Zero;
        }
    }

    public async Task<Activity?> GetActivityAsync(BigInteger activityId)
    {
        try
        {
            var getActivityFunc = _microgridContract.GetFunction("getActivity");
            var result = await getActivityFunc.CallDeserializingToObjectAsync<ActivityDTO>(activityId);

            if (result.Id == BigInteger.Zero)
            {
                return null;
            }

            return new Activity
            {
                Id = result.Id,
                Description = result.Description,
                Reward = result.Reward,
                Executor = result.Executor,
                State = (ActivityState)result.State,
                ValidatorsSnapshot = result.ValidatorsSnapshot,
                Approvals = result.Approvals
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity {ActivityId}", activityId);
            return null;
        }
    }

    public async Task<List<Activity>> GetActivitiesAsync(int count = 20)
    {
        var activities = new List<Activity>();

        try
        {
            // Obtener el próximo ID de actividad
            var nextIdFunc = _microgridContract.GetFunction("nextActivityId");
            var nextId = await nextIdFunc.CallAsync<BigInteger>();

            // Leer las últimas 'count' actividades (si existen)
            var startId = nextId > count ? nextId - count : BigInteger.One;

            for (var id = startId; id < nextId; id++)
            {
                var activity = await GetActivityAsync(id);
                if (activity != null)
                {
                    activities.Add(activity);
                }
            }

            return activities.OrderByDescending(a => a.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities list");
            return activities;
        }
    }

    public async Task<BigInteger> GetParticipantCountAsync()
    {
        try
        {
            var countFunc = _microgridContract.GetFunction("getParticipantCount");
            return await countFunc.CallAsync<BigInteger>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participant count");
            return BigInteger.Zero;
        }
    }

    // DTO para deserialización de getActivity
    public class ActivityDTO
    {
        public BigInteger Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public BigInteger Reward { get; set; }
        public string Executor { get; set; } = string.Empty;
        public int State { get; set; }
        public string[] ValidatorsSnapshot { get; set; } = new string[3];
        public byte Approvals { get; set; }
    }
}
