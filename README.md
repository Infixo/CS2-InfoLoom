# InfoLoom
InfoLoom is a Cities Skylines II mod that adds several new UI windows with extra information.
Currently:
- Demographics
- Workforce structure
- Workplaces distribution
- Residential data
- Commercial data
- Industrial and Office data
- Demand factors
- Resources consumption (optional)

## Features

### Demographics
- Key information about current city demographics in one place.
- Age distribution histogram that also shows cims' activity structure.

![Demographics](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/demographics.png)

### Workforce structure
- Shows workforce structure by education level.
- Total: all potential workers, i.e. teens and adults, that are moved in and not moving away, excluding students.
- Unemployment by education level and structure also shown as percentage.
- Under: cims working on positions below their education level.
- Outside: cims working outside of the city.
- Homeless: unemployed cims with no home.

### Workplaces distribution
- Shows available workplaces split into: city services, commercial sales, commercial leisure, industry extractors, industry manufacturing, and office.
- Leisure are commercial companies selling immaterial resources.
- Commute are cims employed from outside of the city, they commute to work.
- Shows number of companies in each category.

![Workforce and Workplaces](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/workforce_jobs.png)

### Residential data
- Shows residential data used to calculate Residential Demand Factors.
- Residential properties by density: total count, occupied, free.
- Total households and homeless households.
- Average happiness, weighted tax rate, open study positions.
- Household demand which is hidden in the game, it controls if new households spawn.
- For new households, a chance that a Student Household will spawn.

![Residential data](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/residential.png)

### Commercial data
- Shows commercial data used to calculate Commercial Demand Factors.
- Empty buildings and companies that have no property, and average tax rate.
- Service capacity utilization and sales capacity to consumption ratio - these are crucial in calculating LocalDemand, TouristDemand and PetrolLocalDemand.
- Actual numbers of available workforce, and employee capacity ratio.
- Lists resources that currently are not demanded, thus the engine ignores them when calculating Commercial Demand Factors.

![Commercial data](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/commercial.png)

### Industrial and Office data
- Shows industrial and office data used to calculate Industrial and Office Demand Factors.
- Empty buildings and companies that have no property, and average tax rate.
- Local Demand (which is production capacity in fact) and Input Utilization (which tells if resources are available as input).
- Actual numbers of available workforce, and employee capacity ratio.
- Lists resources that currently are not demanded, thus the engine ignores them when calculating Industrial and Office Factors.
- Storage section that shows Demanded Types info which controls if new warehouses are spawned.

![Industrial and Office data](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/industrial.png)

### Demand factors
- Shows individual demand factor VALUES for each demand type. Helpful to learn actual importance of each factor.
- Enables showing all demand factors, not only 5. Useful for industrial demand which is the only one that utilizes up yo 7 factors.
- Additional section shows directly building demand for each zone type and STORAGE demand. Please note that the "main" demand factor is actually a building demand, however UI shows it as moving. In fact it doesn't move, it is just a visualization.
- Also, industrial demand is a higher value from the two: industrial building demand and storage demand.

![Demand factors](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/demandfactors.png)

### Resources consumption
- This feature is disabled by default because it changes the data shown on the vanilla UI. Enable it in the config file by setting SeparateConsumption to true.
- Instead of showing surplus and deficit, the production UI will show commercial consumption and industrial consumption. It is imho much more informative than just surplus/deficit, because it also tells what causes said surplus or deficit.
- Disclaimer. I don't yet how to change existing UI, so the titles "Surplus" and "Deficit" will still be there. Sorry.

![Resources consumption](https://raw.githubusercontent.com/infixo/cs2-infoloom/main/docs/consumption.png)

## Technical

### Requirements and Compatibility
- Cities Skylines II v1.1.0f1 or later; check GitHub or Discord if the mod is compatible with the latest game version.
- Gooee framework by Cities2Modding.
- The mod does NOT modify savefiles.
- The mod does NOT modify any game systems.

### Known Issues
- Nothing atm.

### [Changelog](./CHANGELOG.md)

### Support
- Please report bugs and issues on [GitHub](https://github.com/Infixo/CS2-InfoLoom).
- You may also leave comments on [Discord](https://discord.com/channels/1169011184557637825/1198627819475976263).
