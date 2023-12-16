import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

const AlignedParagraph = ({ left, right }) => {
  const containerStyle = {
    display: 'flex',
    justifyContent: 'space-between',
    textAlign: 'justify',
	marginBottom: '0.1em', // Add some spacing between the <p> tags
  };
  const leftTextStyle = {
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

const HorizontalLine = ({ length }) => {
  const lineStyle = {
    width: `${length}px`,
    borderBottom: '5px solid white', // Adjust the border style as needed
    margin: '1px 0', // Adjust the margin as needed
  };
  return <div style={lineStyle}></div>;
};


const DemandSection = ({ title, value, factors }) => {
	// code
	// render html
	//return <div>
	//<h4>{title}</h4>
	//</div>
	
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


const $DemandFactors = ({react}) => {
	
    const [oldestCim, setOldestCim] = react.useState(0)
    useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)
	
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
	const [totals, setTotals] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureTotals', setTotals)

	const [details, setDetails] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureDetails', setDetails)

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

	// TEST
	const numbers = Array.from({ length: 20 }, (_, index) => index + 1)

	const lines = [];
	for (let i = 0; i < numbers.length; i++) {
		lines.push(<HorizontalLine key={i} length={numbers[i]*50} />);
	}
	//paragraphs.push( <HorizontalLine key={i} length={totals[i]/100} /> );

    return <$Panel react={react} title="Demand Factors">
		<DemandSection title="RESIDENTIAL LOW" value={residentialLowDemand} factors={residentialLowFactors} />
		<DemandSection title="RESIDENTIAL MEDIUM" value={residentialMediumDemand} factors={residentialMediumFactors} />
		<DemandSection title="RESIDENTIAL HIGH" value={residentialHighDemand} factors={residentialHighFactors} />
		<DemandSection title="COMMERCIAL" value={commercialDemand} factors={commercialFactors} />
		<DemandSection title="INDUSTRIAL" value={industrialDemand} factors={industrialFactors} />
		<DemandSection title="OFFICE" value={officeDemand} factors={officeFactors} />
	</$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infixo.demandfactors",
    name: "Demand Factors",
	icon: "Media/Game/Icons/CityStatistics.svg",
    component: $DemandFactors
})

