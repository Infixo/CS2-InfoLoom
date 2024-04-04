import $Demographics  from './demographics'
import $Workforce     from './workforce'
import $Workplaces    from './workplaces'
import $DemandFactors from './demandfactors'
import $Residential   from './residential'
import $Commercial    from './commercial'
import $Industrial    from './industrial'

// Gooee
const MyWindow = ({ react, setupController }) => {
    const { model, update, trigger, _L } = setupController();
    
	return (
	<>
    <div>{model.IsVisibleDemographics ? <$Demographics  react={react} /> : null}</div>
    <div>{model.IsVisibleWorkforce    ? <$Workforce     react={react} /> : null}</div>
    <div>{model.IsVisibleWorkplaces   ? <$Workplaces    react={react} /> : null}</div>
    <div>{model.IsVisibleDemand       ? <$DemandFactors react={react} /> : null}</div>
    <div>{model.IsVisibleResidential  ? <$Residential   react={react} /> : null}</div>
    <div>{model.IsVisibleCommercial   ? <$Commercial    react={react} /> : null}</div>
    <div>{model.IsVisibleIndustrial   ? <$Industrial    react={react} /> : null}</div>
	</>
	);
};
window.$_gooee.register("infoloom", "MyWindow", MyWindow, "main-container", "infoloom");
