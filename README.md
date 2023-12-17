# InfoLoom
InfoLoom is a Cities Skylines 2 mod that adds new UI windows with extra information.
Currently:
- Demand factors
- (w.i.p.) Demographics
- (w.i.p.) Workforce structure

## Features

### Demand factors
- Shows individual demand factor VALUES for each demand type. Helpful to learn actual importance of each factor.
- Enables showing all demand factors, not only 5. Useful for industrial demand which is the only one that utilizes up yo 7 factors.
- Additional section shows directly building demand for each zone type and STORAGE demand. Please note that the "main" demand factor is actually a building demand, however UI shows it as moving. In fact it doesn't move, it is just a visualization.
- Also, industrial demand is a higher value from the two: industrial building demand and storage demand.

![Demand factors](https://github.com/Infixo/CS2-InfoLoom/blob/main/docs/demandfactors.png)

### Resources consumption
- This feature is disabled by default because it changes the data shown on the vanilla UI. Enable it in the config file by setting SeparateConsumption to true.
- Instead of showing surplus and deficit, the production UI will show commercial consumption and industrial consumption. It is imho much more informative than just surplus/deficit, because it also tells what causes said surplus or deficit.
- Disclaimer. I don't yet how to change existing UI, so the titles "Surplus" and "Deficit" will still be there. Sorry.

![Resources consumption](https://github.com/Infixo/CS2-InfoLoom/blob/main/docs/consumption.png)

## Technical

### Requirements and Compatibility
- Cities Skylines II v1.0.18f1 or later; check GitHub or Discord if the mod is compatible with the latest game version.
- BepInEx 5.
- HookUI v0.3.5 or later.
- The mod does NOT modify savefiles.
- The mod does NOT modify any game systems.

### Installation
1. Place the `InfoLoom.dll` file in your BepInEx `Plugins` folder.
2. The config file is automatically created in BepInEx/config folder when the game is run once.

### Known Issues
- Nothing atm.

### Changelog
- v0.2.0 (2023-12-17)
  - Demand factors window is reformatted, to be more aligned with game's visual style.
  - New features: shows all factors, building demand and resources consumption.
- v0.1.0 (2023-12-16)
  - Initial build, includes Demand Factors.

### Support
- Please report bugs and issues on [GitHub](https://github.com/Infixo/CS2-InfoLoom).
- You may also leave comments on [Discord1](https://discord.com/channels/1169011184557637825/1185664314401632306) or [Discord2](https://discord.com/channels/1024242828114673724/1185672922212347944).

## Disclaimers and Notes
