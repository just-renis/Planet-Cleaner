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
	public int CurrentPrestige { get; set; }
	public int CurrentHotbarSlot { get; set; }
	public double Money { get; set; }
	public double TotalMoneyEarned { get; set; }
	public string Checksum { get; set; }
	public ETool EquippedTool { get; set; }
	public Dictionary<EPrestigeUpgradeName, int> PrestigeUpgrades { get; set; }
	public Dictionary<EPlayerUpgrade, PlayerSaveUpgradesData> PlayerUpgrades { get; set; }
	public Dictionary<ETool, ToolUpgradesData> ToolUpgrades { get; set; }
	public Dictionary<ETool, int> HotbarData { get; set; }

	private bool IsPendingForSave;
	private const int LiveVersion = 1;
    private static readonly string SECRET_KEY = "CRAZY_SECRET_KEY69";
    private List<double> CustomPrestigeRequirements;
    
    public PlayerData() { }
    public static PlayerData CreateNewSave()
    {
	    return new PlayerData
	    {
		    CurrentVersion = LiveVersion,
		    CurrentHotbarSlot = 1,
		    EquippedTool = ETool.Hands,
		    PrestigeUpgrades = new Dictionary<EPrestigeUpgradeName, int>(),
		    PlayerUpgrades = new Dictionary<EPlayerUpgrade, PlayerSaveUpgradesData>(),
		    ToolUpgrades = new Dictionary<ETool, ToolUpgradesData>
		    {
			    {
				    ETool.Hands, new ToolUpgradesData
				    {
					    IsOwned = true,
					    Levels = new Dictionary<EToolStat, int>() 
				    }
			    }
		    },
		    HotbarData = new Dictionary<ETool, int>
		    {
			    {
				    ETool.Hands, 1
			    }
		    }
	    };
    }

    public bool CanPrestige() { return Money >= GetPrestigeCost(CurrentPrestige); }
    public bool IsPrestigeUpgradeUnlocked(EPrestigeUpgradeName InUpgradeName) { return PrestigeUpgrades.ContainsKey(InUpgradeName); }
    public bool HasAtLeastOneToolUpgrade(ETool InToolName)
    {
	    return ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData) && ToolData.Levels != null && ToolData.Levels.Values.Any(Level => Level > 0);
    }
    public bool IsToolUnlocked(ETool InToolName)
    {
	    return ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData) && ToolData.IsOwned;
    }
    public bool GetIsPendingForSave() { return IsPendingForSave; }
    public int GetCurrentPrestige() { return CurrentPrestige; }
    public int GetPrestigeSkillPoints() { return CurrentPrestige - PrestigeUpgrades.Count; }
    public int GetCurrentTrashAmount() { return CurrentTrashAmount; }
    public int GetCurrentHotbarSlot() { return CurrentHotbarSlot; }
    public int GetPlayerUpgradeMaxLevel(EPlayerUpgrade InUpgradeName)
    {
	    switch (InUpgradeName)
	    {
		    case EPlayerUpgrade.BagSize: return CalculatePlayerUpgradeMaxLevel(InUpgradeName, EPrestigeUpgradeName.BagMaxLimit1);
		    case EPlayerUpgrade.MovementSpeed: return CalculatePlayerUpgradeMaxLevel(InUpgradeName, EPrestigeUpgradeName.MovementMaxLimit1);
	    }
	    return 0;
    }
    public int GetToolStatLevel(ETool InToolName, EToolStat InToolStat)
    {
	    if (!ToolUpgrades.TryGetValue(InToolName, out ToolUpgradesData ToolData)) return 0;
	    return ToolData.Levels?.GetValueOrDefault(InToolStat, 0) ?? 0;
    }
    public int GetPlayerUpgradeLevel(EPlayerUpgrade InUpgradeName) 
    { 
	    return PlayerUpgrades != null && PlayerUpgrades.TryGetValue(InUpgradeName, out PlayerSaveUpgradesData Data) ? Data.Level : 0; 
    }
    public int GetToolSlot(ETool InToolName) { return HotbarData.GetValueOrDefault(InToolName, -1); }
    public float GetPlayerUpgradeValue(EPlayerUpgrade InUpgradeName) 
    { 
	    return PlayerUpgrades != null && PlayerUpgrades.TryGetValue(InUpgradeName, out PlayerSaveUpgradesData Data) ? Data.CurrentValue : 0f; 
    }
    public float GetPrestigeUpgradeValueAmount(EPrestigeUpgradeName InUpgradeName) { return GetPrestigeUpgradesAmount(InUpgradeName) * GlobalData.Instance.GetUpgradeValue(InUpgradeName); }
    public double GetMoney() { return Money; }
    public double GetTotalMoneyEarned() { return TotalMoneyEarned; }
    public double GetNextPrestigeRequirement(double InPurchasablePrestigesPrice, int InPurchasablePrestigeLevels)
    {
	    return InPurchasablePrestigesPrice + GetPrestigeCost(CurrentPrestige + InPurchasablePrestigeLevels);
    }
    public ETool GetEquippedTool() { return EquippedTool; }
    public BulkPrestigeData GetBulkPrestigeCalculation()
    {
	    BulkPrestigeData PrestigeData = new();
	    double TempMoneyPool = Money;
	    while (true)
	    {
		    int NextTargetPrestige = CurrentPrestige + PrestigeData.LevelsGained;
		    double RequiredCost = GetPrestigeCost(NextTargetPrestige);
		    if (TempMoneyPool >= RequiredCost)
		    {
			    TempMoneyPool -= RequiredCost;
			    PrestigeData.TotalCost += RequiredCost;
			    PrestigeData.LevelsGained++;
		    }
		    else
		    {
			    if (PrestigeData.LevelsGained == 0) return PrestigeData;
			    break;
		    }
	    }
	    return PrestigeData;
    }
    public Dictionary<EPrestigeUpgradeName, int> GetOwnedPrestigeUpgrades() { return PrestigeUpgrades; }
    public static PlayerData Load()
    {
	    if (!FileSystem.Data.FileExists("save.dat")) return CreateNewSave();
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
			    return CreateNewSave();
		    }
		    PlayerData ValidData = Json.Deserialize<PlayerData>(JsonData);
		    Migrate(ValidData);
		    return ValidData;
	    }
	    catch (Exception Exception)
	    {
		    Log.Warning($"Save Corrupted: {Exception.Message}");
		    return CreateNewSave();
	    }
    }

    public void ExecutePrestige()
    {
	    BulkPrestigeData PrestigeData = GetBulkPrestigeCalculation();
	    if (PrestigeData.LevelsGained <= 0 || Money < PrestigeData.TotalCost) return;
	    CurrentPrestige += PrestigeData.LevelsGained;
	    Money = 0;
	    CurrentTrashAmount = 0;
	    ToolUpgrades.Clear();
	    PlayerUpgrades.Clear();
	    ApplyPermanentPrestigeUpgrades();
	    EquippedTool = ETool.Hands;
	    ApplySave(this);
	    EventManager.TriggerOnPrestige();
    }
    public void SetCurrentHotbarSlot(int InSlot)
    {
	    CurrentHotbarSlot = InSlot;
	    Save(this);
    }
    public void SetToolHotbarSlot(ETool InTool, int InSlot)
    {
	    foreach (KeyValuePair<ETool, int> HotbarSlot in HotbarData)
	    {
		    if (HotbarSlot.Value != InSlot) continue;
		    HotbarData[HotbarSlot.Key] = -1;
		    break;
	    }
	    HotbarData[InTool] = InSlot;
	    Save(this);
    }
    public void EquipTool(ETool InToolName)
    {
	    SoundManager.Instance.Play2D(ESound.EquipTool);
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
    public void IncrementPlayerUpgradeLevel(EPlayerUpgrade InPlayerUpgradeName, float InAddedValue)
    {
	    if (!PlayerUpgrades.TryGetValue(InPlayerUpgradeName, out PlayerSaveUpgradesData SaveUpgradesData))
	    {
		    SaveUpgradesData = new PlayerSaveUpgradesData { Level = 0, CurrentValue = 0f };
		    PlayerUpgrades[InPlayerUpgradeName] = SaveUpgradesData;
	    }
	    SaveUpgradesData.Level++;
	    SaveUpgradesData.CurrentValue += InAddedValue;
	    Save(this);
	    EventManager.TriggerBasePlayerUpgradePurchased(InPlayerUpgradeName);
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
	    SoundManager.Instance.Play2D(ESound.CollectTrash);
	    CurrentTrashAmount += InAmount;
	    Save(this);
    }
    public void ResetTrash()
    {
	    CurrentTrashAmount = 0;
	    Save(this);
    }
    public void BuyPrestigeUpgrade(EPrestigeUpgradeName InUpgradeName)
    {
	    if (!PrestigeUpgrades.TryAdd(InUpgradeName, 1)) PrestigeUpgrades[InUpgradeName]++;
	    Save(this);
	    EventManager.TriggerPrestigeUpgradePurchased(InUpgradeName);
    }
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
    
    private int GetPrestigeUpgradesAmount(EPrestigeUpgradeName InUpgradeName) { return PrestigeUpgrades?.GetValueOrDefault(InUpgradeName, 0) ?? 0; }
    private int CalculatePlayerUpgradeMaxLevel(EPlayerUpgrade InUpgradeName, EPrestigeUpgradeName InPrestigeUpgradeName)
    {
	    int BaseMaxLevel = GlobalData.Instance.GetPlayerUpgradeDefaultMaxLevel(InUpgradeName);
	    int PrestigeUpgradeLevel = PrestigeUpgrades.GetValueOrDefault(InPrestigeUpgradeName, 0);
	    float ScaleValue = GlobalData.Instance.GetUpgradeValue(InPrestigeUpgradeName);
	    return BaseMaxLevel + PrestigeUpgradeLevel * (int)ScaleValue;
    }
    private double GetPrestigeCost(int InTargetPrestigeLevel)
    {
	    CustomPrestigeRequirements ??= GlobalData.Instance.GetCustomPrestigeRequirements();
	    if (InTargetPrestigeLevel < CustomPrestigeRequirements.Count)
	    {
		    return CustomPrestigeRequirements[InTargetPrestigeLevel];
	    }
	    int ScaleFactor = InTargetPrestigeLevel - CustomPrestigeRequirements.Count + 1;
	    double BaseFormulaCost = 50000 * Math.Pow(1.5, ScaleFactor); 
	    return Math.Round(BaseFormulaCost);
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

    private void MaximizeUpgrade(EPlayerUpgrade InUpgradeName)
    {
	    PlayerUpgrades.Add(InUpgradeName, new PlayerSaveUpgradesData
	    {
		    Level = 3,
		    CurrentValue = GlobalData.Instance.GetPlayerUpgradeCachedCumulatedValue(InUpgradeName, 3)
	    });
    }
    private void ApplyPermanentPrestigeUpgrades()
    {
	    if (IsPrestigeUpgradeUnlocked(EPrestigeUpgradeName.MovementBagSavedAfterPrestige1))
	    {
		    MaximizeUpgrade(EPlayerUpgrade.MovementSpeed);
		    MaximizeUpgrade(EPlayerUpgrade.BagSize);
	    }
	    ToolUpgrades.Add(ETool.Hands, new ToolUpgradesData 
	    {
		    IsOwned = true,
		    Levels = IsPrestigeUpgradeUnlocked(EPrestigeUpgradeName.HandsMaxedAfterPrestige)
			    ? new Dictionary<EToolStat, int> { { EToolStat.Power, 3 }, { EToolStat.Range, 3 }, { EToolStat.Speed, 3 } }
			    : new Dictionary<EToolStat, int>()
	    });
	    if (IsPrestigeUpgradeUnlocked(EPrestigeUpgradeName.GrabberUnlockedAfterPrestige))
	    {
		    ToolUpgrades.Add(ETool.Grabber, new ToolUpgradesData 
		    {
			    IsOwned = true,
			    Levels = new Dictionary<EToolStat, int>() 
		    });
	    }
    }
    private static void Save(PlayerData InData) { InData.IsPendingForSave = true; }
    private static void Migrate(PlayerData InData)
    {
	//    bool NeedsSave = false;
	 //   if (NeedsSave) Save(InData);
    }
}
