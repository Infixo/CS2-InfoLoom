import React from 'react'
import {useDataUpdate} from 'hookui-framework'
import $Panel from './panel'

const AlignedParagraph = ({ left, right }) => {
  const containerStyle = {
    display: 'flex',
    justifyContent: 'space-between',
    textAlign: 'justify',
	marginBottom: '0.25em', // Add some spacing between the <p> tags
  };
  const leftTextStyle = {
    width: '40%',
	marginLeft: '10%', // Start 10% from the left edge
  };
  const rightTextStyle = {
    width: '40%',
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

// Define styles outside the component
const tableStyles = {
  tableContainer: {
    margin: '20px',
  },
  customTable: {
    borderCollapse: 'collapse',
    //width: '100%',
  },
  tableCell: {
    border: '1px solid #ddd',
    padding: '8px',
    textAlign: 'center',
	width: '10%',
  },
  tableHeader: {
    backgroundColor: '#f2f2f2',
  },
};


const WorkforceLevel = ({levelColor, levelName, levelValues}) => {
  //console.log(levelColor); console.log(levelName); console.log(levelValues);
  // <div class="legend_fqG" style={{justifyContent: 'spaceEvenly'}}>
  return (

    <div class="labels_L7Q row_S2v" style={{width: '95%', paddingTop: '1rem', paddingBottom: '1rem'}}>
		<div style={{width: '1%'}}></div>
		<div style={{ display: 'flex', alignItems: 'center', width: '20%' }}>
			<div class="symbol_aAH" style={{backgroundColor: levelColor }}></div>
			<div>{levelName}</div>
		</div>
		  <div class="row_S2v"           style={{width: '11%', justifyContent: 'center'}}>{levelValues["total"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["service"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["commercial"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["leisure"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["extractor"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["industry"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["office"]}</div>
		  <div class="row_S2v"           style={{width: '11%', justifyContent: 'center'}}>{levelValues["employee"]}</div>
		  <div class="row_S2v"           style={{width: '10%', justifyContent: 'center'}}>{levelValues["open"]}</div>
	</div>
  );
};

const $Workplaces = ({react}) => {
	
    //const [oldestCim, setOldestCim] = react.useState(0)
    //useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)
	
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
	const [workplaces, setWorkplaces] = react.useState([])
	useDataUpdate(react, 'workplaces.ilWorkplaces', setWorkplaces)

	//const [details, setDetails] = react.useState([])
	//useDataUpdate(react, 'populationInfo.structureDetails', setDetails)


	// TEST
	//const numbers = Array.from({ length: 20 }, (_, index) => index + 1)

	//const lines = [];
	//for (let i = 0; i < numbers.length; i++) {
		//lines.push(<HorizontalLine key={i} length={numbers[i]*50} />);
	//}
	//paragraphs.push( <HorizontalLine key={i} length={totals[i]/100} /> );
	
	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.workplaces' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

    const headers = {
		total: 'Total',
		service: 'City',
		commercial: 'Sales',
		leisure: 'Leisure',
		extractor: 'Extract',
		industry: 'Industry',
		office: 'Office',
		employee: 'Employees',
		open: 'Open',
	};
	
	//if (workplaces.length !== 0) {
//		console.log("workplaces=", workplaces);
		//console.log(workplaces[0]); console.log(workplaces[1]);
	//} else {
//		console.log("workplaces has 0 elements");
//	}

	/*
	return <$Panel react={react} title="Workforce" onClose={onClose} initialSize={{ width: window.innerWidth * 0.5, height: window.innerHeight * 0.3 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
	<p>TEST</p>
	</$Panel>
	*/
	return <$Panel react={react} title="Workplace Distribution" onClose={onClose} initialSize={{ width: window.innerWidth * 0.35, height: window.innerHeight * 0.20 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
		{workplaces.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div>
	  <div style={{height: '10rem'}}></div>
	  <WorkforceLevel                      levelName='Education' levelValues={headers} />
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel levelColor='#808080' levelName='Uneducated' levelValues={workplaces[0]} />
	  <WorkforceLevel levelColor='#B09868' levelName='Poorly Educated' levelValues={workplaces[1]} />
	  <WorkforceLevel levelColor='#368A2E' levelName='Educated' levelValues={workplaces[2]} />
	  <WorkforceLevel levelColor='#B981C0' levelName='Well Educated' levelValues={workplaces[3]} />
	  <WorkforceLevel levelColor='#5796D1' levelName='Highly Educated' levelValues={workplaces[4]} />
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel                      levelName='TOTAL' levelValues={workplaces[5]} />
	  </div>
		)}
	</$Panel>

}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infoloom.workplaces",
    name: "InfoLoom: Workplaces",
	icon: "Media/Game/Icons/Workers.svg",
    component: $Workplaces
})
