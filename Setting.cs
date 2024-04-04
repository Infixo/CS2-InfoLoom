using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.InGame;

namespace InfoLoom;

[FileLocation(nameof(InfoLoom))]
[SettingsUIGroupOrder(kToggleGroup)]
[SettingsUIShowGroupName(kToggleGroup)]
public class Setting : ModSetting
{
    public const string kSection = "Main";

    public const string kToggleGroup = "Options";

    public Setting(IMod mod) : base(mod)
    {
        SetDefaults();
    }
		
    /// <summary>
    /// Gets or sets a value indicating whether: Used to force saving of Modsettings if settings would result in empty Json.
    /// </summary>
    [SettingsUIHidden]
    public bool _Hidden { get; set; }

    [SettingsUISection(kSection, kToggleGroup)]
    public bool SeparateConsumption { get; set; }

    public override void SetDefaults()
    {
        _Hidden = true;
        SeparateConsumption = true;
    }
}

public class LocaleEN : IDictionarySource
{
    private readonly Setting m_Setting;
    public LocaleEN(Setting setting)
    {
        m_Setting = setting;
    }
    public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            { m_Setting.GetSettingsLocaleID(), "InfoLoom v0.9" },
            { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

            { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Options" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SeparateConsumption)), "Separate consumption" },
            { m_Setting.GetOptionDescLocaleID(nameof(Setting.SeparateConsumption)), "Enables showing commercial and industrial consumption instead of surplus/deficit in the Production UI." },
        };

        if (Mod.setting.SeparateConsumption)
        {
            dict.Add("EconomyPanel.PRODUCTION_PAGE_SURPLUS", "Population");
            dict.Add("EconomyPanel.PRODUCTION_PAGE_DEFICIT", "Companies");
        }

        return dict;
    }
    public void Unload()
    {
    }
}
