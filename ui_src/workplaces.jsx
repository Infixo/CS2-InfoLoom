import React from 'react'
import useDataUpdate from './use-data-update.js'
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


const WorkforceLevel = ({levelColor, levelName, levelValues, total, showAll}) => {
  //console.log(levelColor); console.log(levelName); console.log(levelValues);
  // <div class="legend_fqG" style={{justifyContent: 'spaceEvenly'}}>
  const percent = ( total > 0 ? (100*levelValues.total/total).toFixed(1)+"%" : "");
  return (
    <div class="labels_L7Q row_S2v" style={{width: '99%', paddingTop: '1rem', paddingBottom: '1rem'}}>
		<div style={{width: '1%'}}></div>
		<div style={{ display: 'flex', alignItems: 'center', width: '20%' }}>
			<div class="symbol_aAH" style={{backgroundColor: levelColor, width: '1.2em' }}></div>
			<div>{levelName}</div>
		</div>
		  <div class="row_S2v"           style={{width: '8%', justifyContent: 'center'}}>{levelValues["total"]}</div>
		  <div class="row_S2v"           style={{width: '7%', justifyContent: 'center'}}>{percent}</div>
		  <div class="row_S2v small_ExK" style={{width: '6%', justifyContent: 'center'}}>{levelValues["service"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '6%', justifyContent: 'center'}}>{levelValues["commercial"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '6%', justifyContent: 'center'}}>{levelValues["leisure"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '7%', justifyContent: 'center'}}>{levelValues["extractor"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '8%', justifyContent: 'center'}}>{levelValues["industry"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '6%', justifyContent: 'center'}}>{levelValues["office"]}</div>
		  <div class="row_S2v"           style={{width: '10%', justifyContent: 'center'}}>{showAll ?? levelValues["employee"]}</div>
		  <div class="row_S2v small_ExK" style={{width: '9%', justifyContent: 'center'}}>{showAll ?? levelValues["commuter"]}</div>
		  <div class="row_S2v"           style={{width: '7%', justifyContent: 'center'}}>{showAll ?? levelValues["open"]}</div>
	</div>
  );
};

const $Workplaces = ({react}) => {
	
    // 0..4 - data by education levels
    // 5 - totals
    // 6 - companies
	const [workplaces, setWorkplaces] = react.useState([])
	useDataUpdate(react, 'workplaces.ilWorkplaces', setWorkplaces)
	
	const onClose = () => {
		// HookUI
		//const data = { type: "toggle_visibility", id: 'infoloom.workplaces' };
		//const event = new CustomEvent('hookui', { detail: data });
		//window.dispatchEvent(event);
		// Gooee
        engine.trigger("infoloom.infoloom.OnToggleVisibleWorkplaces", "Workplaces");
        engine.trigger("audio.playSound", "close-panel", 1);
		
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
		commuter: 'Commute',
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
	return <$Panel react={react} title="Workplace Distribution" onClose={onClose} initialSize={{ width: window.innerWidth * 0.38, height: window.innerHeight * 0.22 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
		{workplaces.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div>
	  <div style={{height: '10rem'}}></div>
	  <WorkforceLevel                      levelName='Education' levelValues={headers} total={0} />
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel levelColor='#808080' levelName='Uneducated' levelValues={workplaces[0]} total={workplaces[5].total} />
	  <WorkforceLevel levelColor='#B09868' levelName='Poorly Educated' levelValues={workplaces[1]} total={workplaces[5].total} />
	  <WorkforceLevel levelColor='#368A2E' levelName='Educated' levelValues={workplaces[2]} total={workplaces[5].total} />
	  <WorkforceLevel levelColor='#B981C0' levelName='Well Educated' levelValues={workplaces[3]} total={workplaces[5].total} />
	  <WorkforceLevel levelColor='#5796D1' levelName='Highly Educated' levelValues={workplaces[4]} total={workplaces[5].total} />
	  <div style={{height: '5rem'}}></div>
	  <WorkforceLevel                      levelName='TOTAL' levelValues={workplaces[5]} total={0} />
	  <WorkforceLevel                      levelName='Companies' levelValues={workplaces[6]} total={0} showAll={false}/>
	  </div>
		)}
	</$Panel>

}

export default $Workplaces

// Registering the panel with HookUI so it shows up in the menu
/*
window._$hookui.registerPanel({
    id: "infoloom.workplaces",
    name: "InfoLoom: Workplaces",
	icon: "Media/Game/Icons/Workers.svg",
    component: $Workplaces
})
*/