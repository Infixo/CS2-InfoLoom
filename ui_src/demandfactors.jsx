import React from 'react'
import {useDataUpdate} from 'hookui-framework'
import $Panel from './panel'

const AlignedParagraph = ({ left, right }) => {
  // Set color based on value of left
  let color;
  if (left < -50) {
    color = 'red';
  } else if (left > 50) {
    color = '#00CC00';
  } else {
    color = 'white'; // default
  };
  const containerStyle = {
    display: 'flex',
    justifyContent: 'space-between',
    textAlign: 'justify',
	marginBottom: '0.1em', // Add some spacing between the <p> tags
  };
  const leftTextStyle = {
	color: color,  
    fontSize: '80%',
    width: '20%',
	marginLeft: '10%', // Start 10% from the left edge
  };
  const rightTextStyle = {
    fontSize: '80%',
    width: '60%',
	marginRight: '10%', // Start 10% from the right edge
    textAlign: 'right',
  };	
  return (
    <p style={containerStyle}>
      <span style={leftTextStyle}>{left}</span>
      <span style={rightTextStyle}>{right}</span>
    </p>
  );
};


  
const DemandSection2 = ({title, value, factors }) => {
  return (
    <div class="infoview-panel-section_RXJ" style={{width: '95%', paddingTop: '3rem', paddingBottom: '3rem'}}>
	  {/* title */}
	  <div class="labels_L7Q row_S2v uppercase_RJI">
	    <div class="left_Lgw row_S2v">{title}</div>
		{ value >= 0 && ( <div class="right_k30 row_S2v">{Math.round(value*100)}</div> ) }
	  </div>
	  <div class="space_uKL" style={{height: '3rem'}}></div>
	  {/* factors */}
      {factors.map((item, index) => (
		<div key={index} class="labels_L7Q row_S2v small_ExK" style={{marginTop: '1rem'}}>
		  <div class="left_Lgw row_S2v">{item["factor"]}</div>
		  <div class="right_k30 row_S2v">
		  {item["weight"] < 0 ?
		  <div class="negative_YWY">{item["weight"]}</div> :
		  <div class="positive_zrK">{item["weight"]}</div>}
		  </div>  
		</div>
      ))}
	</div>
  );  	
};

const DemandSection1 = ({ title, value, factors }) => {
  // this is for 2 columns
  //<div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '10px' }}>
  return (
    <div style={{ display: 'grid', gridTemplateRows: 'auto auto', gap: '10px' }}>
      <div>
        <p>{title}: <strong>{Math.round(value*100)}</strong></p>
      </div>
      <div>
        <ol>
          {factors.map((item, index) => (
            <li key={index}>       
			<AlignedParagraph left={item["weight"]} right={item["factor"]} />
			</li>
          ))}
        </ol>
      </div>
    </div>
  );
};



const $DemandFactors = ({ react }) => {

	// demand values are just single numbers
	const [residentialLowDemand, setResidentialLowDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.residentialLowDemand', setResidentialLowDemand)
	const [residentialMediumDemand, setResidentialMediumDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.residentialMediumDemand', setResidentialMediumDemand)
	const [residentialHighDemand, setResidentialHighDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.residentialHighDemand', setResidentialHighDemand)
	const [commercialDemand, setCommercialDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.commercialDemand', setCommercialDemand)
	const [industrialDemand, setIndustrialDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.industrialDemand', setIndustrialDemand)
	const [officeDemand, setOfficeDemand] = react.useState(0)
	useDataUpdate(react, 'cityInfo.officeDemand', setOfficeDemand)

	// demand factors: an array of variable number of elements with properties: __Type, factor, weight
	const [residentialLowFactors, setResidentialLowFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.residentialLowFactors', setResidentialLowFactors)
	const [residentialMediumFactors, setResidentialMediumFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.residentialMediumFactors', setResidentialMediumFactors)
	const [residentialHighFactors, setResidentialHighFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.residentialHighFactors', setResidentialHighFactors)
	const [commercialFactors, setCommercialFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.commercialFactors', setCommercialFactors)
	const [industrialFactors, setIndustrialFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.industrialFactors', setIndustrialFactors)
	const [officeFactors, setOfficeFactors] = react.useState([])
	useDataUpdate(react, 'cityInfo.officeFactors', setOfficeFactors)

	// building demand
	const [buildingDemand, setbuildingDemand] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilBuildingDemand', setbuildingDemand)
	
	// convert buildingDemand array into "demand factors"
	const titles = ['Residential Low','Residential Medium','Residential High','Commercial','Industrial','Storage','Office'];
	const buildingDemandFactors = titles.map((factor, index) => ({ factor, weight: buildingDemand[index] }));

	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.demandfactors' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

	return <$Panel react={react} title="Demand" onClose={onClose} initialSize={{ width: window.innerWidth * 0.1, height: window.innerHeight * 0.83 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
		<DemandSection2 title="BUILDING DEMAND" value={-1} factors={buildingDemandFactors} />
		<DemandSection2 title="RESIDENTIAL LOW" value={residentialLowDemand} factors={residentialLowFactors} />
		<DemandSection2 title="RESIDENTIAL MEDIUM" value={residentialMediumDemand} factors={residentialMediumFactors} />
		<DemandSection2 title="RESIDENTIAL HIGH" value={residentialHighDemand} factors={residentialHighFactors} />
		<DemandSection2 title="COMMERCIAL" value={commercialDemand} factors={commercialFactors} />
		<DemandSection2 title="INDUSTRIAL" value={industrialDemand} factors={industrialFactors} />
		<DemandSection2 title="OFFICE" value={officeDemand} factors={officeFactors} />
	</$Panel>
};

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
	id: "infoloom.demandfactors",
	name: "InfoLoom: Demand Factors",
	icon: "Media/Game/Icons/ZoningDemand.svg",
	component: $DemandFactors
});
