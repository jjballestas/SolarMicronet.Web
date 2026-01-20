namespace SolarMicronet.Web.Services;

/// <summary>
/// Configuración de direcciones de contratos y endpoints
/// </summary>
public class BlockchainConfig
{
    // Direcciones de contratos (red BLOCK-LAB)
    public const string ENERGON_TOKEN_ADDRESS = "0x9EB2074A0a4038f5A5e8a03d64B0EA9031159882";
    public const string MICROGRID_MANAGER_ADDRESS = "0xC63Dec757Bc85D78117320c2BC3Cc580989CbAFd";
    public const string SMART_METER_ADDRESS = "0xDbC1f6ee28C545ebd291D1D2d49646Bc834549eF";

    // Configuración de red
    public const int EXPECTED_CHAIN_ID = 1337; // BLOCK-LAB
    public const string RPC_URL = "http://virtual.lab.inf.uva.es:60022";

    // SmartMeter API
    public const string SMART_METER_API_BASE = "https://smartmeterapi.ingenas.com";

    // Configuración de operaciones
    public const int COMMISSION_RATE = 1;  // 1%
    public const int COMMISSION_BASE = 100;
    public const byte QUORUM = 2;  // 2 de 3 validadores
}
