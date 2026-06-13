using System.Security.Cryptography;
using Sandbox.Enums;
using Sandbox.Managers;
namespace Sandbox.Data;
using System;
using System.Text;

public sealed class PlayerData
{
	public int CurrentVersion { get; set; }
	public int CurrentTrashAmount { get; set; }
	public double Money { get; set; }
	public double TotalMoneyEarned { get; set; }
	public string Checksum { get; set; }
	public ETool EquippedTool { get; set; }
	public Dictionary<EPrestigeUpgradeName, int> PrestigeUpgrades { get; set; }
	public Dictionary<ETool, ToolUpgradesData> ToolUpgrades { get; set; }

	private bool IsPendingForSave;
	private const int LiveVersion = 1;
    private static readonly string SECRET_KEY = "CRAZY_SECRET_KEY69";
    
    public PlayerData()
    {
	    CurrentVersion = LiveVersion;
	    EquippedTool = ETool.Hands;
	    PrestigeUpgrades = new Dictionary<EPrestigeUpgradeName, int>();
	    ToolUpgrades = new Dictionary<ETool, ToolUpgradesData>
	    {
		    {
			    ETool.Hands, new ToolUpgradesData 
			    {
				    IsOwned = true,
				    Levels = new Dictionary<EToolStat, int>() 
			    }
		    }
	    };
    }

    public bool IsToolUnlocked(ETool InToolName)
    {
	    return ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData) && ToolData.IsOwned;
    }
    public bool GetIsPendingForSave() { return IsPendingForSave; }
    public int GetCurrentTrashAmount() { return CurrentTrashAmount; }
    public int GetToolStatLevel(ETool InToolName, EToolStat InToolStat)
    {
	    if (!ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData)) return 0;
	    return ToolData.Levels?.GetValueOrDefault(InToolStat, 0) ?? 0;
    }
    public int GetUpgradeAmount(EPrestigeUpgradeName InUpgradeName) { return PrestigeUpgrades?.GetValueOrDefault(InUpgradeName, 0) ?? 0; }
    public double GetMoney() { return Money; }
    public double GetTotalMoneyEarned() { return TotalMoneyEarned; }
    public ETool GetEquippedTool() { return EquippedTool; }
    public Dictionary<EPrestigeUpgradeName, int> GetOwnedPrestigeUpgrades() { return PrestigeUpgrades; }

    public void EquipTool(ETool InToolName)
    {
	    EquippedTool = InToolName;
	    Save(this);
	    EventManager.TriggerToolChange();
    }
    public void IncrementToolStatLevel(ETool InToolName, EToolStat InToolStat)
    {
	    if (!ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData))
	    {
		    ToolData = new ToolUpgradesData { IsOwned = true };
		    ToolUpgrades.Add(InToolName, ToolData);
	    }
	    ToolData.Levels ??= new Dictionary<EToolStat, int>();
	    ToolData.Levels.TryGetValue(InToolStat, out int CurrentLevel);
	    ToolData.Levels[InToolStat] = CurrentLevel + 1;
	    Save(this);
	    EventManager.TriggerToolUpgradePurchased(InToolStat);
    }
    public void UnlockTool(ETool InToolType)
    {
	    if (ToolUpgrades.TryGetValue(InToolType, out ToolUpgradesData ToolData)) ToolData.IsOwned = true;
	    else
	    {
		    ToolUpgradesData NewToolData = new() { IsOwned = true };
		    ToolUpgrades.Add(InToolType, NewToolData);
	    }
	    Save(this);
    }
    public void AddMoney(double InAmount)
    {
	    Money += InAmount;
	    TotalMoneyEarned += InAmount;
	    Save(this);
	    EventManager.TriggerMoneyChange(Money);
    }
    public void SubtractMoney(double InAmount)
    {
		Money -= InAmount;
		Save(this);
		EventManager.TriggerMoneyChange(Money);
    }
    public void AddTrash(int InAmount)
    {
	    CurrentTrashAmount += InAmount;
	    Save(this);
    }
    public void ResetTrash()
    {
	    CurrentTrashAmount = 0;
	    Save(this);
    }
    public void BuyUpgrade(EPrestigeUpgradeName InUpgradeName)
    {
	    if (!PrestigeUpgrades.TryAdd(InUpgradeName, 1)) PrestigeUpgrades[InUpgradeName]++;
	    Save(this);
	    EventManager.TriggerUpgradePurchased(InUpgradeName);
    }
    public static void Save(PlayerData InData) { InData.IsPendingForSave = true; }
    public static void ApplySave(PlayerData InData)
    {
	    InData.IsPendingForSave = false;
        InData.Checksum = "";
        string RawJson = Json.Serialize(InData);
        string Hash = CreateMd5(RawJson + SECRET_KEY);
        InData.Checksum = Hash;

        string FinalJson = Json.Serialize(InData);
        string Noise = Guid.NewGuid().ToString()[..Random.Shared.Int(1, 15)];
        
        string Header = Noise.Length.ToString("D2");
        string ContentToHide = Header + Noise + FinalJson;
        
        string Base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(ContentToHide));
        string Obfuscated = XorInput(Base64, SECRET_KEY);
        FileSystem.Data.WriteAllText("save.dat", Obfuscated);
        EventManager.TriggerAutoSaveFinished();
    }
    
    private static string XorInput(string InInput, string InKey)
    {
	    StringBuilder Output = new();
	    for (int Index = 0; Index < InInput.Length; Index++) Output.Append((char)(InInput[Index] ^ InKey[Index % InKey.Length]));
	    return Output.ToString();
    }
    private static string CreateMd5(string InInput)
    {
	    using MD5 Md5 = MD5.Create();
	    byte[] InputBytes = Encoding.ASCII.GetBytes(InInput);
	    byte[] HashBytes = Md5.ComputeHash(InputBytes);
	    return Convert.ToHexString(HashBytes);
    }
    public static PlayerData Load()
    {
	    if (!FileSystem.Data.FileExists("save.dat")) return new PlayerData();
	    try
	    {
		    string Obfuscated = FileSystem.Data.ReadAllText("save.dat");
		    string Base64 = XorInput(Obfuscated, SECRET_KEY);
		    string Decrypted = Encoding.UTF8.GetString(Convert.FromBase64String(Base64));
		    if (!int.TryParse(Decrypted[..2], out int NoiseLength)) throw new Exception("Header Corrupt");
		    string JsonData = Decrypted[(2 + NoiseLength)..];
		    Dictionary<string, object> RawDataMap = Json.Deserialize<Dictionary<string, object>>(JsonData);
		    if (RawDataMap == null || !RawDataMap.TryGetValue("Checksum", out object OutValue)) throw new Exception("Invalid Save Structure");
		    string StoredChecksum = OutValue?.ToString();
		    RawDataMap["Checksum"] = "";
		    string VerificationJson = Json.Serialize(RawDataMap);
		    string CalculatedHash = CreateMd5(VerificationJson + SECRET_KEY);
		    if (StoredChecksum != CalculatedHash)
		    {
			    Log.Error("CRITICAL: Save file tampered with! Hash mismatch.");
			    FileSystem.Data.DeleteFile("save.dat");
			    return new PlayerData();
		    }
		    PlayerData ValidData = Json.Deserialize<PlayerData>(JsonData);
		    Migrate(ValidData);
		    return ValidData;
	    }
	    catch (Exception Exception)
	    {
		    Log.Warning($"Save Corrupted: {Exception.Message}");
		    return new PlayerData();
	    }
    }
    
    private static void Migrate(PlayerData InData)
    {
	//    bool NeedsSave = false;
	 //   if (NeedsSave) Save(InData);
    }
}
